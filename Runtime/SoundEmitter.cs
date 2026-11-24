using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LOGIYGames.Audio
{
    /// <summary>
    /// Runtime playback component backed by a Unity AudioSource and managed by a pool.
    /// </summary>
    /// <remarks>
    /// Emitters are created and rented by <see cref="SoundManager"/>. Prefer the
    /// <see cref="SoundBuilder"/> API instead of creating or reparenting emitters manually.
    /// </remarks>
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        private AudioSource audioSource;
        private Coroutine playbackCoroutine;
        private SoundManager owner;

        /// <summary>The configuration currently assigned to this emitter; null while returned to the pool.</summary>
        public SoundData Data { get; private set; }

        /// <summary>
        /// Linked-list node used internally by the manager for oldest-first frequent-sound limiting.
        /// Do not modify it directly in application code.
        /// </summary>
        public LinkedListNode<SoundEmitter> Node { get; set; }

        private void Awake()
        {
            EnsureAudioSource();
        }

        /// <summary>Copies a sound configuration to the underlying AudioSource.</summary>
        /// <param name="data">Non-null sound configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public void Initialize(SoundData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            StopPlayback();
            Data = data;
            var source = EnsureAudioSource();
            source.clip = data.clip;
            source.outputAudioMixerGroup = data.mixerGroup;
            source.loop = data.loop;
            source.playOnAwake = data.playOnAwake;

            source.mute = data.mute;
            source.bypassEffects = data.bypassEffects;
            source.bypassListenerEffects = data.bypassListenerEffects;
            source.bypassReverbZones = data.bypassReverbZones;

            source.priority = data.priority;
            source.volume = data.volume;
            source.pitch = data.pitch;
            source.panStereo = data.panStereo;
            source.spatialBlend = data.spatialBlend;
            source.reverbZoneMix = data.reverbZoneMix;
            source.dopplerLevel = data.dopplerLevel;
            source.spread = data.spread;

            source.maxDistance = data.maxDistance;
            source.minDistance = data.minDistance;

            source.ignoreListenerVolume = data.ignoreListenerVolume;
            source.ignoreListenerPause = data.ignoreListenerPause;
            source.rolloffMode = data.rolloffMode;
        }

        /// <summary>Starts playback and returns this emitter to its owner when a non-looping clip ends.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the emitter has not been initialized.</exception>
        public void Play()
        {
            if (Data == null)
            {
                throw new InvalidOperationException("SoundEmitter must be initialized before Play is called.");
            }

            StopPlayback();
            var source = EnsureAudioSource();
            source.Play();

            if (source.isPlaying)
            {
                playbackCoroutine = StartCoroutine(WaitForSoundToEnd());
            }
            else
            {
                Stop();
            }
        }

        private IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => audioSource != null && audioSource.isPlaying);
            playbackCoroutine = null;
            Stop();
        }

        /// <summary>Stops playback and returns the emitter to the manager that rented it.</summary>
        public void Stop()
        {
            StopPlayback();
            if (owner != null)
            {
                owner.ReturnToPool(this);
            }
        }

        /// <summary>Applies a random pitch offset relative to the configured base pitch.</summary>
        /// <param name="min">Minimum pitch offset.</param>
        /// <param name="max">Maximum pitch offset.</param>
        /// <exception cref="InvalidOperationException">Thrown when the emitter has not been initialized.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            if (Data == null)
            {
                throw new InvalidOperationException("SoundEmitter must be initialized before changing pitch.");
            }

            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(min), "Minimum pitch offset must not exceed maximum pitch offset.");
            }

            var randomizedPitch = Data.pitch + Random.Range(min, max);
            EnsureAudioSource().pitch = Mathf.Clamp(randomizedPitch, -3f, 3f);
        }

        internal void SetOwner(SoundManager soundManager)
        {
            owner = soundManager;
        }

        internal void ResetForPool()
        {
            StopPlayback();
            if (audioSource != null)
            {
                audioSource.clip = null;
            }

            Data = null;
            Node = null;
            owner = null;
        }

        internal void StopPlayback()
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }

            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private AudioSource EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            return audioSource;
        }
    }
}
