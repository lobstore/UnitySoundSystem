# LOGIYGames Audio — Unity Sound System

A small pooled audio playback system for Unity. Configure a SoundEmitter prefab, add a SoundManager to a scene, describe clips with SoundData, and play them through the fluent SoundBuilder API.

## Quick start

1. Add a SoundManager to the scene.
2. Create a prefab containing SoundEmitter (it requires an AudioSource) and assign it to Sound Emitter Prefab.
3. Add a serialized SoundData field to a component and assign its clip.
4. Play the sound:

    using LOGIYGames.Audio;
    using UnityEngine;

    public sealed class AudioExample : MonoBehaviour
    {
        [SerializeField] private SoundData clickSound;
        public void PlayClick()
        {
            SoundManager.Instance.CreateSoundBuilder()
                .WithPosition(transform.position)
                .Play(clickSound);
        }
    }

The emitter pool is initialized lazily on the first playback request. Non-looping sounds return to the pool automatically. Looping sounds must be stopped explicitly with SoundEmitter.Stop or SoundManager.StopAll.

## Documentation

- Documentation/API.json — structured API contract for AI agents and tooling.
- Documentation/Manual.md — setup, configuration, lifecycle, API reference, and troubleshooting.
- Documentation/QuickStart.pdf — printable usage example.

## Compatibility

The package manifest targets Unity 6000.6. The runtime assembly is LOGIYGames.Audio; the public namespace is LOGIYGames.Audio. The runtime assembly references the project's existing Core and Extensions assemblies.
