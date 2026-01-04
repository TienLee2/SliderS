using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Components")]
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Tooltip("Fade duration")]
    [SerializeField] private float crossFadeDuration = 1.5f;

    private AudioClip currentIntroClip;
    private AudioClip currentLoopClip;
    private Coroutine musicCoroutine;

    private void Awake()
    {
        SetupSingleton();
        SetupAudioSources();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (musicCoroutine != null)
            {
                musicCoroutine = null;
                StopMusic();
                return;
            }

            if (currentIntroClip != null || currentLoopClip != null)
            {
                PlayBGM(currentIntroClip, currentLoopClip);
                Debug.Log("P pressed: Starting Music...");
            }
        }
    }

    public void PlayBGM(AudioClip introClip, AudioClip loopClip)
    {
        //Reset trạng thái
        introSource.volume = masterVolume;
        loopSource.volume = masterVolume;

        currentIntroClip = introClip;
        currentLoopClip = loopClip;

        //Reset Coroutine cũ
        StopCurrentRoutine();

        //Bắt đầu luồng nhạc mới
        if (introClip != null || loopClip != null)
        {
            musicCoroutine = StartCoroutine(Routine_MusicSequence(introClip, loopClip));
        }
    }

    public void StopMusic()
    {
        StopCurrentRoutine();
        introSource.Stop();
        loopSource.Stop();
    }

    public IEnumerator FadeOutMusic(float duration)
    {
        float startIntroVol = introSource.volume;
        float startLoopVol = loopSource.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;

            introSource.volume = Mathf.Lerp(startIntroVol, 0f, progress);
            loopSource.volume = Mathf.Lerp(startLoopVol, 0f, progress);
            yield return null;
        }

        StopMusic();
        // Reset volume cho lần sau
        introSource.volume = masterVolume;
        loopSource.volume = masterVolume;
    }

    private IEnumerator Routine_MusicSequence(AudioClip intro, AudioClip loop)
    {
        // TRƯỜNG HỢP 1: Có đoạn Intro
        if (intro != null)
        {
            introSource.clip = intro;
            introSource.volume = masterVolume;
            introSource.Play();

            float waitTime = intro.length - crossFadeDuration;

            if (waitTime > 0)
            {
                // Chờ đến thời điểm bắt đầu Fade
                yield return new WaitForSecondsRealtime(waitTime);

                PlayLoopImmediately(loop);

                //// C. Nếu có Loop -> Thực hiện Crossfade
                //if (loop != null)
                //{
                //    yield return StartCoroutine(Routine_CrossFadeToLoop(loop));
                //}
            }
            else
            {
                yield return new WaitForSecondsRealtime(intro.length);
                if (loop != null) PlayLoopImmediately(loop);
            }

            introSource.Stop();
        }
        // TRƯỜNG HỢP 2: Không có Intro, chỉ có Loop
        else if (loop != null)
        {
            PlayLoopImmediately(loop);
        }
    }

    private IEnumerator Routine_CrossFadeToLoop(AudioClip loop)
    {
        loopSource.clip = loop;
        loopSource.volume = 0;
        loopSource.Play();

        float timer = 0f;
        while (timer < crossFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / crossFadeDuration;

            // Intro nhỏ dần, Loop to dần
            introSource.volume = Mathf.Lerp(masterVolume, 0f, t);
            loopSource.volume = Mathf.Lerp(0f, masterVolume, t);

            yield return null;
        }

        loopSource.volume = masterVolume;
    }


    private void PlayLoopImmediately(AudioClip loop)
    {
        loopSource.clip = loop;
        loopSource.volume = masterVolume;
        loopSource.Play();
    }

    private void StopCurrentRoutine()
    {
        if (musicCoroutine != null) StopCoroutine(musicCoroutine);
    }

    private void SetupSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void SetupAudioSources()
    {
        if (introSource == null) introSource = gameObject.AddComponent<AudioSource>();
        if (loopSource == null) loopSource = gameObject.AddComponent<AudioSource>();

        introSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        loopSource.loop = true;

        currentIntroClip = introSource.clip;
        currentLoopClip = loopSource.clip;
    }
}