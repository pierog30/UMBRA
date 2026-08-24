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

        ambienceSource.clip = CreateArchiveAmbience();
        ambienceSource.loop = true;
        ambienceSource.volume = 0.42f;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.Play();

        effectsSource.volume = 0.8f;
        effectsSource.spatialBlend = 0f;

        jumpClip = CreateTone("Ribbon Jump", 0.14f, 260f, 510f, 0.38f);
        stepClip = CreateTone("Paper Step", 0.08f, 135f, 82f, 0.30f);
        deathClip = CreateTone("Lost Memory", 0.42f, 210f, 72f, 0.48f);
        pickupClip = CreateTone("Echo Recovered", 0.38f, 440f, 980f, 0.46f);
        mechanismClip = CreateTone("Resonance", 0.28f, 170f, 285f, 0.46f);
    }

    public void PlayJump() => Play(jumpClip);
    public void PlayStep() => Play(stepClip);
    public void PlayDeath() => Play(deathClip);
    public void PlayPickup() => Play(pickupClip);
    public void PlayMechanism() => Play(mechanismClip);

    private void Play(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static AudioClip CreateArchiveAmbience()
    {
        const int sampleRate = 44100;
        const float duration = 6f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        var random = new System.Random(27);
        float filteredPaper = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float noise = ((float)random.NextDouble() * 2f) - 1f;
            filteredPaper = Mathf.Lerp(filteredPaper, noise, 0.025f);
            float time = i / (float)sampleRate;
            float breath = 0.78f + (Mathf.Sin(time * Mathf.PI * 2f * 0.10f) * 0.22f);
            float chord = (Mathf.Sin(time * Mathf.PI * 2f * 110f) * 0.07f) +
                (Mathf.Sin(time * Mathf.PI * 2f * 165f) * 0.04f) +
                (Mathf.Sin(time * Mathf.PI * 2f * 220f) * 0.025f);
            samples[i] = ((filteredPaper * 0.12f) + chord) * breath;
        }

        AudioClip clip = AudioClip.Create("Archive Ambience", sampleCount, 1, sampleRate, false);
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
