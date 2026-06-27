// =============================================================================
// AudioManager.cs
// =============================================================================
// PURPOSE:
//   Music + SFX mixer. Pools AudioSources, fades, and adapts to time of day.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShadowEmpire.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioMixer mixer;
        public AudioMixerGroup sfxGroup, musicGroup, ambientGroup;

        private readonly Dictionary<string, AudioClip> _sfx = new();
        private AudioSource _musicSrc;
        private AudioSource _ambientSrc;
        private readonly Queue<AudioSource> _sfxPool = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSrc   = gameObject.AddComponent<AudioSource>();
            _ambientSrc = gameObject.AddComponent<AudioSource>();
            _musicSrc.outputAudioMixerGroup = musicGroup;
            _ambientSrc.outputAudioMixerGroup = ambientGroup;
            _musicSrc.loop = _ambientSrc.loop = true;
        }

        public void RegisterSfx(string key, AudioClip clip) => _sfx[key] = clip;

        public void PlaySfx(string key, Vector3 pos, float volume = 1f)
        {
            if (!_sfx.TryGetValue(key, out var clip)) return;
            var src = GetPooledSource();
            src.transform.position = pos;
            src.clip = clip;
            src.volume = volume;
            src.outputAudioMixerGroup = sfxGroup;
            src.Play();
        }

        public void PlayMusic(AudioClip clip, float fadeDuration = 1.5f)
        {
            if (clip == null) return;
            StopAllCoroutines();
            StartCoroutine(FadeSwap(clip, fadeDuration));
        }

        private System.Collections.IEnumerator FadeSwap(AudioClip newClip, float dur)
        {
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                _musicSrc.volume = Mathf.Lerp(1, 0, t / dur);
                yield return null;
            }
            _musicSrc.clip = newClip;
            _musicSrc.Play();
            t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                _musicSrc.volume = Mathf.Lerp(0, 1, t / dur);
                yield return null;
            }
        }

        private AudioSource GetPooledSource()
        {
            foreach (var s in _sfxPool) if (!s.isPlaying) return s;
            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f; // 3D
            _sfxPool.Enqueue(src);
            return src;
        }

        public void SetMasterVolume(float v) => mixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);
        public void SetMusicVolume(float v) => mixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);
        public void SetSfxVolume(float v)   => mixer.SetFloat("SfxVol",   Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20);
    }
}