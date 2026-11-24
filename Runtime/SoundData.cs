using System;
using UnityEngine;
using UnityEngine.Audio;

namespace LOGIYGames.Audio
{
    /// <summary>
    /// Serializable configuration for a sound playback.
    /// </summary>
    /// <remarks>
    /// Store this type in a serialized field to configure sounds in the Inspector.
    /// The runtime system copies these values to a pooled <see cref="AudioSource"/> each
    /// time a sound is played. Random pitch offsets are applied relative to <see cref="pitch"/>.
    /// </remarks>
    [Serializable]
    public class SoundData
    {
        /// <summary>The audio clip to play. A non-null clip is required.</summary>
        public AudioClip clip;

        /// <summary>Optional Audio Mixer group that receives this sound.</summary>
        public AudioMixerGroup mixerGroup;

        /// <summary>Whether playback loops until explicitly stopped.</summary>
        public bool loop;

        /// <summary>AudioSource play-on-awake setting. Playback through this package is started by <see cref="SoundEmitter.Play"/>.</summary>
        public bool playOnAwake;

        /// <summary>Whether this sound participates in the manager's capped, oldest-first sound limit.</summary>
        public bool frequentSound;

        /// <summary>Mute state copied to AudioSource.</summary>
        public bool mute;

        /// <summary>Whether source effects are bypassed.</summary>
        public bool bypassEffects;

        /// <summary>Whether listener effects are bypassed.</summary>
        public bool bypassListenerEffects;

        /// <summary>Whether reverb zones are bypassed.</summary>
        public bool bypassReverbZones;

        /// <summary>AudioSource priority. Unity uses values from 0 (highest) to 256 (lowest).</summary>
        public int priority = 128;

        /// <summary>Playback volume in the range [0, 1].</summary>
        [Range(0, 1)]
        public float volume = 1f;

        /// <summary>Base playback pitch. Random offsets, when enabled, are added to this value.</summary>
        [Range(-3, 3)]
        public float pitch = 1f;

        /// <summary>Default minimum random pitch offset, applied relative to <see cref="pitch"/>.</summary>
        public float minPitchDelta = -0.05f;

        /// <summary>Default maximum random pitch offset, applied relative to <see cref="pitch"/>.</summary>
        public float maxPitchDelta = 0.05f;

        /// <summary>Stereo pan in the range [-1, 1].</summary>
        [Range(-1, 1)]
        public float panStereo;

        /// <summary>Spatial blend: 0 is 2D and 1 is fully 3D.</summary>
        [Range(0, 1)]
        public float spatialBlend;

        /// <summary>Reverb zone mix.</summary>
        [Range(0, 1.1f)]
        public float reverbZoneMix = 1f;

        /// <summary>Doppler effect level.</summary>
        [Range(0, 5)]
        public float dopplerLevel = 1f;

        /// <summary>Spread angle in degrees.</summary>
        [Range(0, 360)]
        public float spread;

        /// <summary>Minimum distance for 3D attenuation.</summary>
        [Min(0)]
        public float minDistance = 1f;

        /// <summary>Maximum distance for 3D attenuation.</summary>
        [Min(0)]
        public float maxDistance = 500f;

        /// <summary>Whether the sound ignores AudioListener.volume.</summary>
        public bool ignoreListenerVolume;

        /// <summary>Whether the sound ignores AudioListener.pause.</summary>
        public bool ignoreListenerPause;

        /// <summary>Distance attenuation curve used by the AudioSource.</summary>
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}
