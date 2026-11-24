using UnityEngine;

namespace LOGIYGames.Audio
{
    /// <summary>
    /// Audio-related conversion helpers for UI controls and normalized values.
    /// </summary>
    public static class AudioExtensions
    {
        /// <summary>
        /// Converts a normalized slider value to a decibel value suitable for an exposed
        /// Unity Audio Mixer volume parameter.
        /// </summary>
        /// <param name="sliderValue">Input value. Values are clamped to the inclusive range [0, 1].</param>
        /// <returns>A decibel value in the range [-80, 0].</returns>
        /// <remarks>
        /// Zero is represented by -80 dB rather than negative infinity, avoiding invalid
        /// values when the result is passed to an Audio Mixer parameter.
        /// </remarks>
        public static float ToLogarithmicVolume(this float sliderValue)
        {
            var normalized = Mathf.Clamp01(sliderValue);
            return Mathf.Log10(Mathf.Max(normalized, 0.0001f)) * 20f;
        }

        /// <summary>
        /// Maps a normalized linear fraction to a normalized logarithmic fraction.
        /// </summary>
        /// <param name="fraction">Input fraction. Values are clamped to [0, 1].</param>
        /// <returns>A logarithmically shaped value in the range [0, 1].</returns>
        /// <remarks>
        /// This is useful when a fade or UI control should progress logarithmically while
        /// still starting at zero and ending at one.
        /// </remarks>
        public static float ToLogarithmicFraction(this float fraction)
        {
            var normalized = Mathf.Clamp01(fraction);
            return Mathf.Log10(1f + 9f * normalized);
        }
    }
}
