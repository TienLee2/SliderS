using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Configuration")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    public double lookAheadTime = 1.0f;

    [Header("Data")]
    public AudioClip defaultIntroClip;
    public AudioClip defaultLoopClip;

    private AudioSource[] audioSourcePool;
    private int toggle = 0;

    private AudioClip currentIntro;
    private AudioClip currentLoop;

    private double nextStartTime; 
    private bool isPlaying = false;
    [SerializeField] private bool hasIntroPlayed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tạo 2 AudioSource để thay phiên nhau phát 
        audioSourcePool = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSourcePool[i] = gameObject.AddComponent<AudioSource>();
            audioSourcePool[i].playOnAwake = false;
            audioSourcePool[i].loop = false;
        }

        // Load data mặc định
        currentIntro = defaultIntroClip;
        currentLoop = defaultLoopClip;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!isPlaying)
            {
                if (defaultIntroClip != null || defaultLoopClip != null)
                {
                    PlayBGM(defaultIntroClip, defaultLoopClip);
                    Debug.Log(">>> [AudioManager] Bắt đầu phát nhạc (Intro -> Loop)...");
                }
            }
            else
            {
                StopMusic();
            }
        }

        if (isPlaying)
        {
            if (AudioSettings.dspTime > nextStartTime - lookAheadTime)
            {
                ScheduleNextClip();
            }
        }
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

        // THỰC HIỆN LÊN LỊCH (Gapless)

        // 1. Chọn AudioSource đang rảnh
        AudioSource source = audioSourcePool[toggle];
        source.clip = clipToPlay;
        source.volume = masterVolume;

        // 2. PlayScheduled: nối nhạc không bị khựng
        source.PlayScheduled(nextStartTime);

        // 3. Cộng dồn thời gian cho lần tiếp theo
        double duration = (double)clipToPlay.samples / clipToPlay.frequency;
        nextStartTime += duration;

        // 4. Đảo chiều nguồn phát (0 -> 1 -> 0...)
        toggle = 1 - toggle;
    }
}