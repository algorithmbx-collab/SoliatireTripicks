using System.Collections.Generic;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        [Header("Sources")]
        [SerializeField]
        private AudioSource musicSource;

        [SerializeField]
        private AudioSource sfxSource;

        [Header("Music")]
        [SerializeField]
        private AudioClip musicClip;

        [SerializeField]
        [Range(0f, 1f)]
        private float musicVolume = 0.6f;

        [Header("Sound Effects")]
        [SerializeField]
        private AudioClip flipClip;

        [SerializeField]
        private AudioClip drawClip;

        [SerializeField]
        private AudioClip playClip;

        [SerializeField]
        private AudioClip invalidClip;

        [SerializeField]
        private AudioClip winClip;

        [SerializeField]
        private AudioClip loseClip;

        [SerializeField]
        [Range(0f, 1f)]
        private float sfxVolume = 1f;

        private readonly Dictionary<string, AudioClip> resourceCache = new();

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }

                return instance;
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
            LoadDefaultClips();
            ApplyVolumes();
            TryStartMusic();
        }

        public void PlayFlip() => PlaySfx(flipClip);

        public void PlayDraw() => PlaySfx(drawClip);

        public void PlayPlay() => PlaySfx(playClip);

        public void PlayInvalidMove() => PlaySfx(invalidClip);

        public void PlayWin() => PlaySfx(winClip);

        public void PlayLose() => PlaySfx(loseClip);

        public void SetMusicClip(AudioClip clip)
        {
            musicClip = clip;
            TryStartMusic(true);
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        private void LoadDefaultClips()
        {
            flipClip = flipClip != null ? flipClip : LoadFromResources("card_flip");
            drawClip = drawClip != null ? drawClip : LoadFromResources("card_draw");
            playClip = playClip != null ? playClip : LoadFromResources("card_play");
            invalidClip = invalidClip != null ? invalidClip : LoadFromResources("invalid");
            winClip = winClip != null ? winClip : LoadFromResources("win");
            loseClip = loseClip != null ? loseClip : LoadFromResources("lose");
            musicClip = musicClip != null ? musicClip : LoadFromResources("music_loop");
        }

        private AudioClip LoadFromResources(string key)
        {
            if (resourceCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var clip = Resources.Load<AudioClip>($"Audio/{key}");
            if (clip != null)
            {
                resourceCache[key] = clip;
            }
            else
            {
                Debug.LogWarning($"Audio clip '{key}' not found in Resources/Audio.");
            }

            return clip;
        }

        private void ApplyVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        private void TryStartMusic(bool restart = false)
        {
            if (musicClip == null || musicSource == null)
            {
                return;
            }

            if (restart || !musicSource.isPlaying)
            {
                musicSource.clip = musicClip;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }
    }
}
