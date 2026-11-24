# LOGIYGames Audio — руководство

## 1. Архитектура

Пакет предоставляет пул переиспользуемых AudioSource через компонент SoundEmitter. Игровой код описывает звук через сериализуемый SoundData, затем запускает его fluent API класса SoundBuilder. Жизненным циклом emitter управляет SoundManager.

| Тип | Ответственность |
|---|---|
| SoundData | Настройки одного звука, сериализуемые в Inspector |
| SoundManager | Persistent singleton, пул emitter, лимит частых звуков |
| SoundBuilder | Fluent-настройка позиции и pitch перед запуском |
| SoundEmitter | Компонент с AudioSource и управлением playback lifecycle |
| AudioExtensions | Преобразования громкости и нормализованной fraction |

Структурированный контракт для ИИ-агентов находится в API.json. Там перечислены сигнатуры, поля, ограничения, побочные эффекты и жизненный цикл.

## 2. Установка и настройка

1. Установите пакет в Unity-проект.
2. Создайте prefab с компонентом SoundEmitter. Атрибут RequireComponent автоматически требует AudioSource.
3. Добавьте SoundManager на объект сцены. Обычно нужен один экземпляр persistent singleton.
4. Назначьте prefab в поле Sound Emitter Prefab.
5. При необходимости настройте Collection Check, Default Capacity, Max Pool Size и Max Sound Instances.
6. Создайте сериализуемое поле SoundData в компоненте и назначьте AudioClip в поле clip.

Пул инициализируется лениво при первом SoundManager.Get(), обычно внутри SoundBuilder.Play. Порядок вызова Start пользовательскими компонентами не влияет на готовность пула, но prefab должен быть назначен до первого воспроизведения.

Пакет использует UnityEngine.Audio, UnityEngine.Pool и внешние assembly references, уже перечисленные в Runtime/LOGIYGames.Audio.asmdef. Проект должен содержать соответствующие Core и Extensions assemblies; GUID-ссылки не следует менять без проверки исходных зависимостей.

## 3. Первое воспроизведение

    using LOGIYGames.Audio;
    using UnityEngine;

    public sealed class AudioExample : MonoBehaviour
    {
        [SerializeField] private SoundData clickSound;
        public void PlayClick()
        {
            SoundManager.Instance
                .CreateSoundBuilder()
                .WithPosition(transform.position)
                .Play(clickSound);
        }
    }

Создайте SoundData через сериализованное поле и назначьте AudioClip. CreateSoundBuilder возвращает builder, WithPosition задаёт world position, а Play арендует emitter и запускает звук.

Для UI и других 2D-звуков задайте spatialBlend = 0. Для позиционных 3D-эффектов используйте spatialBlend = 1 и настройте minDistance, maxDistance и rolloffMode.

## 4. Жизненный цикл и владение

1. SoundBuilder.Play проверяет SoundData, clip и ограничение частых звуков.
2. SoundManager.Get арендует emitter из UnityEngine.Pool.ObjectPool<SoundEmitter>.
3. SoundEmitter.Initialize копирует конфигурацию в AudioSource.
4. Builder задаёт позицию, parent и, при необходимости, случайное смещение pitch.
5. SoundEmitter.Play запускает клип.
6. Для non-looping клипа coroutine ожидает окончания AudioSource.isPlaying и возвращает emitter в пул.
7. Looping клип сам не заканчивается. Вызовите SoundEmitter.Stop либо SoundManager.StopAll.

Emitter возвращается в тот экземпляр SoundManager, который его арендовал. Повторный или чужой вызов ReturnToPool игнорируется.

Правила:
- Не вызывайте Destroy для emitter, полученного из пула.
- Не отключайте GameObject emitter вручную: используйте Stop.
- Не изменяйте SoundManager.FrequentSoundEmitters и SoundEmitter.Node из прикладного кода.
- При ручном SoundManager.Get вызывайте ReturnToPool или Stop.
- Builder сохраняет позицию и явный pitch override; для независимых конфигураций создавайте новый builder.

## 5. Настройки SoundData

Все настройки SoundData копируются в AudioSource при вызове Initialize.

| Поле | Назначение |
|---|---|
| clip | AudioClip для воспроизведения; обязателен |
| mixerGroup | Необязательная группа Audio Mixer |
| loop | Зацикливает звук; остановка вручную |
| playOnAwake | Значение AudioSource.playOnAwake; запуск API выполняется через Play |
| frequentSound | Включает maxSoundInstances и вытеснение старейшего frequent emitter |
| mute | Отключает звук на AudioSource |
| bypassEffects | Обход эффектов AudioSource |
| bypassListenerEffects | Обход эффектов AudioListener |
| bypassReverbZones | Обход reverb zones |
| priority | Приоритет AudioSource: 0 — самый высокий, 256 — самый низкий |
| volume | Громкость, Inspector range 0..1 |
| pitch | Базовая высота тона, Inspector range -3..3 |
| minPitchDelta, maxPitchDelta | Диапазон случайного смещения относительно pitch |
| panStereo | Стереопанорама -1..1 |
| spatialBlend | 0 — 2D, 1 — 3D |
| reverbZoneMix | Смешивание с reverb zone |
| dopplerLevel | Интенсивность эффекта Доплера |
| spread | Угол spread в градусах |
| minDistance, maxDistance | Дистанции пространственной аттенюации |
| ignoreListenerVolume | Игнорировать AudioListener.volume |
| ignoreListenerPause | Игнорировать AudioListener.pause |
| rolloffMode | Режим затухания AudioSource |

Inspector Range и Min не заменяют runtime-валидацию данных, заданных из кода. Для pitch-диапазона должно выполняться minPitchDelta <= maxPitchDelta.

## 6. Fluent API

### Позиция

    SoundManager.Instance
        .CreateSoundBuilder()
        .WithPosition(enemy.transform.position)
        .Play(explosionSound);

Позиция задаётся в мировых координатах. После конфигурации emitter становится дочерним объектом SoundManager с сохранением мирового положения.

### Случайный pitch

    SoundManager.Instance
        .CreateSoundBuilder()
        .WithPosition(transform.position)
        .WithRandomPitch(-0.12f, 0.12f)
        .Play(footstepSound);

Случайное смещение прибавляется к SoundData.pitch; итоговое значение ограничивается диапазоном AudioSource [-3, 3]. Явный WithRandomPitch перекрывает minPitchDelta и maxPitchDelta из SoundData. Без override используются настройки SoundData. Чтобы отключить вариацию, установите обе границы в 0.

### Остановка всех звуков

    SoundManager.Instance.StopAll();

Метод останавливает и возвращает все активные emitter этого менеджера, включая зацикленные звуки.

## 7. Лимит frequent sounds и пул

maxSoundInstances применяется только к SoundData.frequentSound = true. При достижении лимита менеджер останавливает самый старый frequent emitter, чтобы освободить место.

- maxSoundInstances = 30: одновременно до 30 frequent sounds.
- maxSoundInstances = 1: новый frequent sound вытесняет предыдущий.
- maxSoundInstances = 0: frequent sounds не запускаются; обычные звуки этим параметром не ограничиваются.

maxPoolSize задаёт максимум неиспользуемых emitter, которые пул сохраняет после возврата. Это не лимит одновременного воспроизведения. defaultCapacity задаёт начальную ёмкость внутреннего пула и не создаёт объекты заранее.

## 8. Низкоуровневое использование

В обычном случае используйте CreateSoundBuilder. Публичные Get и ReturnToPool доступны для специализированных сценариев:

    SoundEmitter emitter = SoundManager.Instance.Get();
    try
    {
        emitter.Initialize(soundData);
        emitter.transform.position = transform.position;
        emitter.Play();
    }
    catch
    {
        SoundManager.Instance.ReturnToPool(emitter);
        throw;
    }

Не вызывайте ReturnToPool после автоматического возврата того же emitter. Для loop-звуков вызовите emitter.Stop(). SoundEmitter — инфраструктурный MonoBehaviour; обычно его не нужно добавлять вручную в игровой код.

## 9. AudioExtensions

### Громкость для Audio Mixer

    float decibels = slider.value.ToLogarithmicVolume();
    audioMixer.SetFloat("MasterVolume", decibels);

Вход ограничивается диапазоном 0..1. Значение 0 преобразуется в -80 dB, значение 1 — в 0 dB. Это позволяет избежать отрицательной бесконечности.

### Логарифмическая fraction

    float shapedFade = linearFade.ToLogarithmicFraction();

Метод ограничивает fraction диапазоном 0..1 и возвращает логарифмически сформированное значение в том же диапазоне.

## 10. Диагностика

| Симптом | Проверка |
|---|---|
| InvalidOperationException: prefab missing | Назначьте prefab в Sound Emitter Prefab у SoundManager |
| На prefab нет SoundEmitter | Добавьте компонент SoundEmitter; AudioSource требуется автоматически |
| SoundData.clip is null | Назначьте AudioClip в поле clip |
| Frequent sounds не запускаются | Проверьте frequentSound и maxSoundInstances; при 0 они отключены |
| Loop-звук продолжает играть | Вызовите SoundEmitter.Stop или SoundManager.StopAll |
| Emitter остался активным | Не отключайте его вручную; используйте Stop/ReturnToPool |
| Не найден PersistentSingleton | Подключите Core assembly из исходного asmdef |
| Не найдено расширение GetOrAddComponent | Проверьте подключение Extensions assembly из исходного asmdef |

## 11. Совместимость

Текущий package manifest указывает Unity 6000.6. Runtime использует UnityEngine.Pool.ObjectPool<T> и внешние Core/Extensions assemblies проекта. Публичные имена исходного API сохранены. Файлы .meta и ссылки GUID в asmdef не меняются.

Компиляцию в Unity Editor/Player необходимо проверить в целевом проекте с подключёнными зависимостями. Перед выпуском выполните импорт пакета, проверьте Console и проиграйте как минимум 2D, 3D, loop и frequent sound.
