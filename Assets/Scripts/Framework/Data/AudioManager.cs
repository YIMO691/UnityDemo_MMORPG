using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance => instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource soundSource;

    [Header("Default Audio")]
    [SerializeField] private AudioClip defaultBGM;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
    }

    private void Start()
    {
        ApplySetting();

        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    private void InitAudioSources()
    {
        // 1. 优先使用 Inspector 拖好的引用
        // 2. 如果没拖，尝试从自己和子物体中查找
        if (bgmSource == null || soundSource == null)
        {
            AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);

            if (bgmSource == null && sources.Length > 0)
                bgmSource = sources[0];

            if (soundSource == null && sources.Length > 1)
                soundSource = sources[1];
        }

        // 3. 如果还没找到，再自动创建
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGMSource");
            bgmObj.transform.SetParent(transform);
            bgmObj.transform.localPosition = Vector3.zero;
            bgmSource = bgmObj.AddComponent<AudioSource>();
        }

        if (soundSource == null)
        {
            GameObject soundObj = new GameObject("SoundSource");
            soundObj.transform.SetParent(transform);
            soundObj.transform.localPosition = Vector3.zero;
            soundSource = soundObj.AddComponent<AudioSource>();
        }

        // 统一初始化参数
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        soundSource.playOnAwake = false;
        soundSource.loop = false;
        soundSource.spatialBlend = 0f;

        Debug.Log("[AudioManager] Audio sources initialized.");
    }

    public void ApplySetting()
    {
        if (DataManager.Instance.GetMusicOn())
            SetMusicVolume(DataManager.Instance.GetMusicVolume());
        else
            SetMusicVolume(0f);

        if (DataManager.Instance.GetSoundOn())
            SetSoundVolume(DataManager.Instance.GetSoundVolume());
        else
            SetSoundVolume(0f);
    }

    public void SetMusicVolume(float value)
    {
        if (bgmSource == null)
        {
            Debug.LogError("[AudioManager] bgmSource is null.");
            return;
        }

        bgmSource.volume = value;
    }

    public void SetSoundVolume(float value)
    {
        if (soundSource == null)
        {
            Debug.LogError("[AudioManager] soundSource is null.");
            return;
        }

        soundSource.volume = value;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null || soundSource == null)
            return;

        soundSource.PlayOneShot(clip);
    }
}
