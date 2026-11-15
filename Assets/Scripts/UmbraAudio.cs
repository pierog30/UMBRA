using System;
using UnityEngine;

public class UmbraAudio : MonoBehaviour
{
    public enum SoundEvent
    {
        Jump,
        Step,
        Death,
        Pickup,
        Mechanism,
        Landing,
        Scare,
        Breath
    }

    public static UmbraAudio Instance { get; private set; }

    public bool IsConfigured => ambienceSource != null && effectsSource != null &&
        ambienceSource.clip != null && ambienceVolume > 0f && effectsVolume > 0f;
    public float AmbienceVolume => ambienceVolume;
    public float EffectsVolume => effectsVolume;
    public string LastEvent { get; private set; } = "None";
    public event Action<SoundEvent> SoundPlayed;

    [Header("Mix de audio")]
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float scareDuckVolume = 0.16f;
    [SerializeField, Min(0.05f)] private float scareDuckDuration = 0.7f;
    [SerializeField, Min(0.1f)] private float ambienceFadeSpeed = 2.8f;

    private AudioSource ambienceSource;
    private AudioSource effectsSource;
    private AudioClip jumpClip;
    private AudioClip stepClip;
    private AudioClip deathClip;
    private AudioClip pickupClip;
    private AudioClip mechanismClip;
    private AudioClip landingClip;
    private AudioClip scareClip;
    private AudioClip breathClip;
    private float breathTimer;
    private float ambienceDuckTimer;

    private const string AmbienceVolumeKey = "UmbraAmbienceVolume";
    private const string EffectsVolumeKey = "UmbraEffectsVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ambienceSource = gameObject.AddComponent<AudioSource>();
        effectsSource = gameObject.AddComponent<AudioSource>();

        ambienceSource.clip = CreateWindClip();
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.priority = 128;
        ambienceSource.Play();

        effectsSource.loop = false;
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.priority = 64;

        jumpClip = CreateTone("Jump", 0.14f, 210f, 430f, 0.42f);
        stepClip = CreateTone("Step", 0.08f, 115f, 68f, 0.36f);
        deathClip = CreateTone("Death", 0.45f, 165f, 42f, 0.55f);
        pickupClip = CreateTone("Pickup", 0.3f, 420f, 820f, 0.45f);
        mechanismClip = CreateTone("Mechanism", 0.26f, 135f, 62f, 0.5f);
        landingClip = CreateTone("Heavy landing", 0.18f, 92f, 48f, 0.5f);
        scareClip = CreateTone("Terror sting", 0.55f, 72f, 610f, 0.58f);
        breathClip = CreateTone("Breath", 0.7f, 58f, 42f, 0.18f);
        ApplySavedVolume();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlayerMove)
        {
            return;
        }

        float deltaTime = Time.unscaledDeltaTime;
        breathTimer -= deltaTime;
        ambienceDuckTimer = Mathf.Max(0f, ambienceDuckTimer - deltaTime);
        float targetAmbience = ambienceDuckTimer > 0f
            ? Mathf.Min(ambienceVolume, scareDuckVolume)
            : ambienceVolume;
        ambienceSource.volume = Mathf.MoveTowards(
            ambienceSource.volume,
            targetAmbience,
            ambienceFadeSpeed * deltaTime);

        if (breathTimer <= 0f)
        {
            breathTimer = 3.4f;
            Play(SoundEvent.Breath, breathClip);
        }
    }

    public void PlayJump() => Play(SoundEvent.Jump, jumpClip);
    public void PlayStep() => Play(SoundEvent.Step, stepClip);
    public void PlayDeath() => Play(SoundEvent.Death, deathClip, true);
    public void PlayPickup() => Play(SoundEvent.Pickup, pickupClip);
    public void PlayMechanism() => Play(SoundEvent.Mechanism, mechanismClip);
    public void PlayLanding() => Play(SoundEvent.Landing, landingClip);
    public void PlayScare() => Play(SoundEvent.Scare, scareClip, true);

    public void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("UmbralMasterVolume", 0.8f);
        ambienceVolume = PlayerPrefs.GetFloat(AmbienceVolumeKey, ambienceVolume);
        effectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, effectsVolume);
        if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
        if (effectsSource != null) effectsSource.volume = effectsVolume;
    }

    public void SetMixVolumes(float ambience, float effects, bool save = true)
    {
        ambienceVolume = Mathf.Clamp01(ambience);
        effectsVolume = Mathf.Clamp01(effects);
        if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
        if (effectsSource != null) effectsSource.volume = effectsVolume;

        if (!save) return;
        PlayerPrefs.SetFloat(AmbienceVolumeKey, ambienceVolume);
        PlayerPrefs.SetFloat(EffectsVolumeKey, effectsVolume);
        PlayerPrefs.Save();
    }

    private void Play(SoundEvent soundEvent, AudioClip clip, bool duckAmbience = false)
    {
        if (this != null && effectsSource != null && clip != null)
        {
            effectsSource.PlayOneShot(clip);
            LastEvent = soundEvent.ToString();
            SoundPlayed?.Invoke(soundEvent);
            if (duckAmbience)
            {
                ambienceDuckTimer = scareDuckDuration;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private static AudioClip CreateWindClip()
    {
        const int sampleRate = 44100;
        const float duration = 6f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        var random = new System.Random(27);
        float filteredNoise = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float noise = ((float)random.NextDouble() * 2f) - 1f;
            filteredNoise = Mathf.Lerp(filteredNoise, noise, 0.04f);
            float time = i / (float)sampleRate;
            float breath = 0.78f + (Mathf.Sin(time * Mathf.PI * 2f * 0.12f) * 0.22f);
            float drone = (Mathf.Sin(time * Mathf.PI * 2f * 92f) * 0.09f) +
                (Mathf.Sin(time * Mathf.PI * 2f * 138f) * 0.035f);
            samples[i] = ((filteredNoise * 0.55f) + drone) * breath;
        }

        AudioClip clip = AudioClip.Create("Umbra Wind", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateTone(
        string name,
        float duration,
        float fromFrequency,
        float toFrequency,
        float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float frequency = Mathf.Lerp(fromFrequency, toFrequency, progress);
            phase += (Mathf.PI * 2f * frequency) / sampleRate;
            float envelope = Mathf.Sin(progress * Mathf.PI);
            samples[i] = Mathf.Sin(phase) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

