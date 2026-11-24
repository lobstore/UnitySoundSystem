using System;
using UnityEngine;

namespace LOGIYGames.Audio
{
    /// <summary>
    /// Fluent builder for configuring and playing one sound through a <see cref="SoundManager"/>.
    /// </summary>
    /// <remarks>
    /// Create builders with <see cref="SoundManager.CreateSoundBuilder"/>. A builder can be
    /// reused; position and explicit pitch overrides remain configured between calls to
    /// <see cref="Play"/>. Without an explicit override, the pitch delta from <see cref="SoundData"/>
    /// is used for each playback.
    /// </remarks>
    public class SoundBuilder
    {
        private readonly SoundManager soundManager;
        private Vector3 position = Vector3.zero;
        private bool hasPitchOverride;
        private float pitchMinDelta;
        private float pitchMaxDelta;

        /// <summary>Creates a builder bound to a sound manager.</summary>
        /// <param name="soundManager">Manager that owns the emitter pool.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="soundManager"/> is null.</exception>
        public SoundBuilder(SoundManager soundManager)
        {
            this.soundManager = soundManager != null
                ? soundManager
                : throw new ArgumentNullException(nameof(soundManager));
        }

        /// <summary>Sets the world-space position for subsequent playback.</summary>
        /// <param name="position">World-space position.</param>
        /// <returns>This builder, for fluent configuration.</returns>
        public SoundBuilder WithPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        /// <summary>Overrides the random pitch offset range from the supplied <see cref="SoundData"/>.</summary>
        /// <param name="pitchMinDelta">Minimum offset from the configured base pitch.</param>
        /// <param name="pitchMaxDelta">Maximum offset from the configured base pitch.</param>
        /// <returns>This builder, for fluent configuration.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the minimum offset is greater than the maximum offset.</exception>
        public SoundBuilder WithRandomPitch(float pitchMinDelta, float pitchMaxDelta)
        {
            if (pitchMinDelta > pitchMaxDelta)
            {
                throw new ArgumentOutOfRangeException(nameof(pitchMinDelta), "Minimum pitch offset must not exceed maximum pitch offset.");
            }

            this.pitchMinDelta = pitchMinDelta;
            this.pitchMaxDelta = pitchMaxDelta;
            hasPitchOverride = true;
            return this;
        }

        /// <summary>Gets an emitter, applies this builder's configuration, and starts playback.</summary>
        /// <param name="soundData">Sound configuration. Its <see cref="SoundData.clip"/> must be assigned.</param>
        /// <remarks>
        /// A null configuration or clip is logged and ignored. Frequent sounds may be rejected
        /// by the manager when its configured limit is zero or its active list is inconsistent.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The manager has no valid SoundEmitter prefab.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The SoundData pitch-offset range is invalid.</exception>
        public void Play(SoundData soundData)
        {
            if (soundData == null)
            {
                Debug.LogError("Cannot play a sound: SoundData is null.");
                return;
            }

            if (soundData.clip == null)
            {
                Debug.LogError("Cannot play a sound: SoundData.clip is null.");
                return;
            }

            if (!soundManager.CanPlaySound(soundData))
            {
                return;
            }

            var emitter = soundManager.Get();
            try
            {
                emitter.Initialize(soundData);
                emitter.transform.position = position;
                emitter.transform.SetParent(soundManager.transform, true);

                if (hasPitchOverride)
                {
                    emitter.WithRandomPitch(pitchMinDelta, pitchMaxDelta);
                }
                else if (soundData.minPitchDelta != 0f || soundData.maxPitchDelta != 0f)
                {
                    emitter.WithRandomPitch(soundData.minPitchDelta, soundData.maxPitchDelta);
                }

                if (soundData.frequentSound)
                {
                    emitter.Node = soundManager.FrequentSoundEmitters.AddLast(emitter);
                }

                emitter.Play();
            }
            catch
            {
                soundManager.ReturnToPool(emitter);
                throw;
            }
        }
    }
}
