using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Makes simple retro sound effects and music in code, so the game
// doesn't need any audio files. Other scripts call SoundManager.Play("coin").
public class SoundManager : MonoBehaviour
{
    private const int SampleRate = 22050;

    private static SoundManager instance;

    private AudioSource effects;
    private AudioSource music;
    private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private AudioClip townMusic;
    private AudioClip dungeonMusic;

    // creates itself when the game starts so it doesn't need adding to every scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        GameObject sound = new GameObject("SoundManager");
        instance = sound.AddComponent<SoundManager>();
        DontDestroyOnLoad(sound);
    }

    public static void Play(string soundName)
    {
        if (instance == null)
            return;

        AudioClip clip;
        if (instance.clips.TryGetValue(soundName, out clip))
        {
            instance.effects.PlayOneShot(clip);
        }
    }

    private void Awake()
    {
        effects = gameObject.AddComponent<AudioSource>();
        effects.volume = 0.5f;

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.volume = 0.18f;

        MakeSoundEffects();
        townMusic = MakeTune(new[] { 72, 76, 79, 76, 74, 77, 81, 77, 72, 76, 79, 84, 83, 79, 76, 74 }, new[] { 48, 53, 48, 55 }, 0.28f);
        dungeonMusic = MakeTune(new[] { 57, 0, 60, 59, 57, 0, 52, 0, 57, 0, 60, 62, 64, 0, 59, 0 }, new[] { 45, 41, 43, 40 }, 0.36f);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicFor(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicFor(scene.name);
    }

    private void PlayMusicFor(string sceneName)
    {
        AudioClip wanted = sceneName.Contains("Dungeon") ? dungeonMusic : townMusic;

        if (music.clip != wanted)
        {
            music.clip = wanted;
            music.Play();
        }
    }

    private void MakeSoundEffects()
    {
        clips["swing"] = MakeNoise(0.08f, 0.35f);
        clips["hit"] = MakeSweep(500f, 150f, 0.08f, 0.5f, true);
        clips["hurt"] = MakeSweep(300f, 80f, 0.2f, 0.5f, true);
        clips["coin"] = MakeNotes(new[] { 988f, 1319f }, 0.07f, 0.35f);
        clips["pickup"] = MakeNotes(new[] { 523f, 659f, 784f, 1047f }, 0.07f, 0.35f);
        clips["bolt"] = MakeSweep(900f, 400f, 0.15f, 0.25f, false);
        clips["blip"] = MakeNotes(new[] { 660f }, 0.03f, 0.15f);
        clips["pop"] = MakeNoise(0.25f, 0.3f);
        clips["fanfare"] = MakeNotes(new[] { 523f, 659f, 784f, 659f, 784f, 1047f }, 0.12f, 0.35f);
    }

    // ---------- tiny synthesiser ----------

    private float Square(float frequency, int sample)
    {
        float phase = frequency * sample / SampleRate;
        return (phase - Mathf.Floor(phase)) < 0.5f ? 1f : -1f;
    }

    private AudioClip ToClip(string clipName, float[] data)
    {
        AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // a burst of noise that fades out (swooshes and pops)
    private AudioClip MakeNoise(float length, float volume)
    {
        int count = (int)(SampleRate * length);
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float fade = 1f - (float)i / count;
            data[i] = Random.Range(-1f, 1f) * fade * fade * volume;
        }

        return ToClip("noise", data);
    }

    // a pitch slide from one note to another
    private AudioClip MakeSweep(float startFrequency, float endFrequency, float length, float volume, bool square)
    {
        int count = (int)(SampleRate * length);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += frequency / SampleRate;

            float wave = square ? ((phase % 1f) < 0.5f ? 1f : -1f) : Mathf.Sin(phase * Mathf.PI * 2f);
            data[i] = wave * (1f - t) * volume;
        }

        return ToClip("sweep", data);
    }

    // a few beeps one after another
    private AudioClip MakeNotes(float[] frequencies, float noteLength, float volume)
    {
        int perNote = (int)(SampleRate * noteLength);
        float[] data = new float[perNote * frequencies.Length];

        for (int n = 0; n < frequencies.Length; n++)
        {
            for (int i = 0; i < perNote; i++)
            {
                float fade = 1f - (float)i / perNote * 0.6f;
                data[n * perNote + i] = Square(frequencies[n], i) * fade * volume;
            }
        }

        return ToClip("notes", data);
    }

    // a short looping tune: melody notes (MIDI numbers, 0 = rest) over a bass line
    private AudioClip MakeTune(int[] melody, int[] bass, float noteLength)
    {
        int perNote = (int)(SampleRate * noteLength);
        int total = perNote * melody.Length;
        float[] data = new float[total];
        int bassLength = total / bass.Length;

        for (int i = 0; i < total; i++)
        {
            int note = melody[i / perNote];
            int inNote = i % perNote;
            float sample = 0f;

            if (note > 0)
            {
                float fade = 1f - (float)inNote / perNote * 0.7f;
                sample += Square(MidiToFrequency(note), i) * 0.25f * fade;
            }

            // soft triangle-ish bass so it isn't too harsh
            float bassFrequency = MidiToFrequency(bass[Mathf.Min(i / bassLength, bass.Length - 1)]);
            float bassPhase = (bassFrequency * i / SampleRate) % 1f;
            sample += (Mathf.Abs(bassPhase * 4f - 2f) - 1f) * 0.3f;

            data[i] = sample;
        }

        return ToClip("tune", data);
    }

    private float MidiToFrequency(int note)
    {
        return 440f * Mathf.Pow(2f, (note - 69) / 12f);
    }
}
