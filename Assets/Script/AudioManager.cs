using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class MusicTrack
    {
        public string trackName;
        public AudioClip introClip;
        public AudioClip loopClip;
    }

    [Header("Configuration")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    public double lookAheadTime = 1.0f;

    [Header("Playlist Data")]
    // Element 0 = Nút U
    // Element 1 = Nút I
    // Element 2 = Nút O
    // Element 3 = Nút P
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    private AudioSource[] audioSourcePool;
    private int toggle = 0;

    private AudioClip currentIntro;
    private AudioClip currentLoop;

    private double nextStartTime;
    private bool isPlaying = false;
    private bool hasIntroPlayed = false;

    // Theo dõi bài nào đang phát (-1 là không có bài nào trong playlist)
    private int currentTrackIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSourcePool = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSourcePool[i] = gameObject.AddComponent<AudioSource>();
            audioSourcePool[i].playOnAwake = false;
            audioSourcePool[i].loop = false;
        }
    }

    private void Update()
    {
        // --- XỬ LÝ INPUT ---
        if (Input.GetKeyDown(KeyCode.U)) ToggleTrack(0);
        if (Input.GetKeyDown(KeyCode.I)) ToggleTrack(1);
        if (Input.GetKeyDown(KeyCode.O)) ToggleTrack(2);
        if (Input.GetKeyDown(KeyCode.P)) ToggleTrack(3);

        if (isPlaying)
        {
            if (AudioSettings.dspTime > nextStartTime - lookAheadTime)
            {
                ScheduleNextClip();
            }
        }
    }

    public void ToggleTrack(int index)
    {
        if (index < 0 || index >= musicTracks.Count)
        {
            Debug.LogWarning($"[AudioManager] Chưa thiết lập nhạc cho vị trí số {index}!");
            return;
        }

        if (isPlaying && currentTrackIndex == index)
        {
            StopMusic();
            Debug.Log($">>> [AudioManager] Đã tắt nhạc (Track {index})");
        }
        else
        {
            PlayTrackByIndex(index);
            Debug.Log($">>> [AudioManager] Đang phát Track {index}: {musicTracks[index].trackName}");
        }
    }

    private void PlayTrackByIndex(int index)
    {
        MusicTrack track = musicTracks[index];
        PlayBGM(track.introClip, track.loopClip);

        currentTrackIndex = index;
    }

    public void PlayBGM(AudioClip introClip, AudioClip loopClip)
    {
        if (isPlaying && currentIntro == introClip && currentLoop == loopClip) return;

        StopMusic();

        currentIntro = introClip;
        currentLoop = loopClip;

        if (currentIntro != null || currentLoop != null)
        {
            PlayMusicInternal();
        }
    }

    public void StopMusic()
    {
        isPlaying = false;
        currentTrackIndex = -1;
        foreach (var source in audioSourcePool)
        {
            source.Stop();
            source.clip = null;
        }
    }

    private void PlayMusicInternal()
    {
        isPlaying = true;
        hasIntroPlayed = false;
        toggle = 0;

        nextStartTime = AudioSettings.dspTime + 0.2;

        audioSourcePool[0].volume = masterVolume;
        audioSourcePool[1].volume = masterVolume;

        ScheduleNextClip();
    }

    private void ScheduleNextClip()
    {
        AudioClip clipToPlay = null;

        if (currentIntro != null && !hasIntroPlayed)
        {
            clipToPlay = currentIntro;
            hasIntroPlayed = true;
        }
        else
        {
            clipToPlay = currentLoop;
        }

        if (clipToPlay == null) return;

        AudioSource source = audioSourcePool[toggle];
        source.clip = clipToPlay;
        source.volume = masterVolume;
        source.PlayScheduled(nextStartTime);

        double duration = (double)clipToPlay.samples / clipToPlay.frequency;
        nextStartTime += duration;

        toggle = 1 - toggle;
    }
}