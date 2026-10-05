using UnityEngine;

public class UmbraAudio : MonoBehaviour
{
    public static UmbraAudio Instance { get; private set; }

    public bool IsConfigured => ambienceSource != null && effectsSource != null &&
        ambienceSource.clip != null && ambienceSource.volume >= 0.35f &&
        effectsSource.volume >= 0.65f;

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
        ambienceSource.volume = 0.42f;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.Play();

        effectsSource.volume = 0.8f;
        effectsSource.spatialBlend = 0f;

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
        breathTimer -= Time.unscaledDeltaTime;
        if (breathTimer <= 0f)
        {
            breathTimer = 3.4f;
            Play(breathClip);
        }
    }

    public void PlayJump() => Play(jumpClip);
    public void PlayStep() => Play(stepClip);
    public void PlayDeath() => Play(deathClip);
    public void PlayPickup() => Play(pickupClip);
    public void PlayMechanism() => Play(mechanismClip);
    public void PlayLanding() => Play(landingClip);
    public void PlayScare() => Play(scareClip);

    public void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("UmbralMasterVolume", 0.8f);
    }

    private void Play(AudioClip clip)
    {
        if (this != null && effectsSource != null && clip != null)
        {
            effectsSource.PlayOneShot(clip);
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

    private static AudioClip CreateTone(string name, float duration, float fromFrequency, float toFrequency, float volume)
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
