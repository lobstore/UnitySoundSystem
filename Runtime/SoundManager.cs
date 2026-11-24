using System;
using System.Collections.Generic;
using LOGIYGames.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace LOGIYGames.Audio
{
    /// <summary>
    /// Persistent owner of the pooled sound emitters and the main entry point for sound playback.
    /// </summary>
    /// <remarks>
    /// Add one instance to the scene, assign an emitter prefab that contains a
    /// <see cref="SoundEmitter"/>, and use <see cref="CreateSoundBuilder"/> to play sounds.
    /// The pool is initialized lazily on the first request, so callers do not depend on Unity's
    /// <c>Start</c> execution order.
    /// </remarks>
    public class SoundManager : PersistentSingleton<SoundManager>
    {
        private IObjectPool<SoundEmitter> soundEmitterPool;
        private readonly HashSet<SoundEmitter> activeSoundEmitters = new HashSet<SoundEmitter>();

        /// <summary>
        /// Active frequent sounds in playback order. The first node is the oldest instance.
        /// This collection is exposed for compatibility; application code should not modify it.
        /// </summary>
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new LinkedList<SoundEmitter>();

        [SerializeField] private GameObject soundEmitterPrefab;
        [SerializeField] private bool collectionCheck = true;
        [SerializeField, Min(0)] private int defaultCapacity = 10;
        [SerializeField, Min(1)] private int maxPoolSize = 100;
        [SerializeField, Min(0)] private int maxSoundInstances = 30;

        /// <summary>Creates a new fluent builder associated with this manager.</summary>
        /// <returns>A builder that can position, vary pitch, and play sounds.</returns>
        public SoundBuilder CreateSoundBuilder()
        {
            return new SoundBuilder(this);
        }

        /// <summary>Determines whether a sound can be played under the frequent-sound limit.</summary>
        /// <param name="data">Sound configuration to validate.</param>
        /// <returns>False for null data, a zero frequent-sound limit, or an inconsistent active list; otherwise true.</returns>
        /// <remarks>
        /// Non-frequent sounds are not affected by <c>maxSoundInstances</c>. When the frequent
        /// limit is reached, the oldest frequent emitter is stopped until one slot is available.
        /// </remarks>
        public bool CanPlaySound(SoundData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot check playback capacity: SoundData is null.");
                return false;
            }

            if (!data.frequentSound)
            {
                return true;
            }

            var limit = Mathf.Max(0, maxSoundInstances);
            if (limit == 0)
            {
                return false;
            }

            while (FrequentSoundEmitters.Count >= limit)
            {
                var oldestNode = FrequentSoundEmitters.First;
                if (oldestNode == null || oldestNode.Value == null)
                {
                    if (oldestNode == null)
                    {
                        return false;
                    }

                    FrequentSoundEmitters.Remove(oldestNode);
                    continue;
                }

                var oldestEmitter = oldestNode.Value;
                oldestEmitter.Stop();

                if (ReferenceEquals(FrequentSoundEmitters.First, oldestNode))
                {
                    Debug.LogWarning("Frequent sound list is out of sync with the emitter pool; playback was rejected.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>Rents an emitter from the pool.</summary>
        /// <returns>An active emitter owned by this manager.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no valid emitter prefab is assigned.</exception>
        public SoundEmitter Get()
        {
            EnsurePoolInitialized();
            return soundEmitterPool.Get();
        }

        /// <summary>Returns an emitter rented from this manager to its pool.</summary>
        /// <param name="soundEmitter">Emitter to release. Null or already-returned emitters are ignored.</param>
        public void ReturnToPool(SoundEmitter soundEmitter)
        {
            if (soundEmitter == null || !activeSoundEmitters.Contains(soundEmitter) || soundEmitterPool == null)
            {
                return;
            }

            soundEmitter.StopPlayback();
            soundEmitterPool.Release(soundEmitter);
        }

        /// <summary>Stops and releases all emitters currently rented from this manager.</summary>
        public void StopAll()
        {
            if (activeSoundEmitters.Count == 0)
            {
                FrequentSoundEmitters.Clear();
                return;
            }

            var activeSnapshot = new List<SoundEmitter>(activeSoundEmitters);
            foreach (var soundEmitter in activeSnapshot)
            {
                if (soundEmitter != null)
                {
                    soundEmitter.Stop();
                }
            }

            FrequentSoundEmitters.Clear();
        }

        private void EnsurePoolInitialized()
        {
            if (soundEmitterPool != null)
            {
                return;
            }

            if (soundEmitterPrefab == null)
            {
                throw new InvalidOperationException("SoundManager requires a SoundEmitter prefab. Assign it in the Inspector before playing sounds.");
            }

            var safeMaxPoolSize = Mathf.Max(1, maxPoolSize);
            var safeDefaultCapacity = Mathf.Clamp(defaultCapacity, 0, safeMaxPoolSize);
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                safeDefaultCapacity,
                safeMaxPoolSize);
        }

        private SoundEmitter CreateSoundEmitter()
        {
            var emitterObject = Instantiate(soundEmitterPrefab, transform);
            var soundEmitter = emitterObject.GetComponent<SoundEmitter>();
            if (soundEmitter == null)
            {
                Destroy(emitterObject);
                throw new InvalidOperationException("The SoundEmitter prefab must contain a SoundEmitter component.");
            }

            emitterObject.SetActive(false);
            return soundEmitter;
        }

        private void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.SetOwner(this);
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }

        private void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            var node = soundEmitter.Node;
            if (node != null && node.List == FrequentSoundEmitters)
            {
                FrequentSoundEmitters.Remove(node);
            }

            activeSoundEmitters.Remove(soundEmitter);
            soundEmitter.ResetForPool();
            soundEmitter.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            if (soundEmitter != null && soundEmitter.gameObject != null)
            {
                Destroy(soundEmitter.gameObject);
            }
        }
    }
}
