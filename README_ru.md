# AppStructure

`AppStructure` - это state-driven слой приложения для Unity. Он задает явный bootstrap-процесс, предсказуемую модель переходов между экранами, переиспользуемые view root'ы и набор инфраструктурных компонентов, которые отделяют UI-flow, навигацию и композицию приложения от gameplay-логики и доменных систем.

Репозиторий предназначен как foundation-уровень для более высокого composition-слоя. На практике он решает вопрос: как приложение стартует, как переключает состояния, как привязывает view к модели и как не превращает код экранов в набор случайных `SetActive`, подписок и ручных переходов.

## Какую проблему решает

Во многих Unity-проектах рост UI и application flow быстро приводит к одинаковым проблемам:

- запуск размазан по десяткам `Awake()` и `Start()`;
- переходы между экранами выражены через несогласованные `SetActive`;
- логика экрана, подписки на события и анимации смешаны в одном `MonoBehaviour`;
- popup-окна, back-flow, блокировка ввода и focus-навигация дублируются от фичи к фиче.

`AppStructure` заменяет это небольшим, но стабильным набором контрактов:

- одна bootstrap-точка входа;
- единый формат данных для перехода между состояниями;
- единый lifecycle для view root'ов и их элементов;
- отдельные state machine для полноэкранного flow и для stacked flow;
- встроенные точки расширения для focus, escape, orientation и input lock.

## Место в архитектурном стеке

```text
Project-specific code
    -> игровые состояния, модели, ECS/network setup, feature-логика

Higher-level composition layer
    -> composition root, регистрация моделей и view model,
       app/popup controllers, анимируемые state root'ы

AppStructure
    -> bootstrap, state root'ы, transfer pipeline,
       state machines, navigation helpers, adaptive view,
       input lock, generic lifecycle элементов
```

`AppStructure` находится между "сырой" Unity-оркестрацией и конкретной прикладной логикой проекта. Это не gameplay-framework и не DI-контейнер, а именно слой управления состояниями и жизненным циклом приложения.

## Технический обзор

### Основные зоны ответственности

| Зона | Ключевые типы | Ответственность |
| --- | --- | --- |
| Bootstrap | `AppBootstrap` | Запускает приложение в явно разделенных фазах. |
| Lifecycle root'ов | `IAppStructurePart<TAppModel>`, `AppPartRoot<TAppModel>` | Определяет общий lifecycle: `PreInitialize`, `InitializeAsync`, `BindAsync`, `PostInitializeAsync`. |
| Оркестрация состояний | `AppMainViewsRoot<TState, TAppModel>` | Хранит словарь state root'ов и применяет переходы между ними. |
| Root отдельного состояния | `AppStateRoot<TState, TAppModel>` | Управляет state-specific и static элементами одного экрана. |
| Базовая немедленная реализация экрана | `ImmediateAppStateRoot<TState, TAppModel>` | Дает готовую модель "включить сразу / выключить сразу" с управлением `Canvas`. |
| Leaf-элементы view | `StateViewElement<TState, TAppModel>`, `StaticStateViewElement<TState, TAppModel>`, `StaticViewElement<TAppModel>` | Разделяют поведение на state-bound, transition-aware и always-on части. |
| Payload перехода | `TransferInfo<TState>` | Передает `From`, `To`, `IsFromBack` и `Parameters`. |
| Навигационные машины состояний | `GoBackSupportStateMachine<TState>`, `OpenCloseStateMachine<TState>` | Поддерживают линейный flow и stacked open/close flow для popup'ов. |
| Блокировка ввода | `AppInputLocker<TLockMessage>` | Централизует блокировку взаимодействия через bit-mask флаги. |
| Adaptive view | `ScreenOrientationManage<TState, TAppModel>`, `RotatableAppStateImmediateStateRoot<TState, TAppModel>`, `RectPositionByState` | Управляет ориентацией экрана и layout state'ами. |
| Утилиты | `Extensions`, `RootByGenericTypes<TValue>`, `LINQExtensions` | Дают безопасную обработку коллекций и typed registry по generic-типам. |

### High-level runtime flow

```text
AppBootstrap
    -> AwakePrepare()
    -> PrepareBootstrapProcess()
    -> StartBootstrapProcess()
    -> FinalizeBootstrapProcess()
    -> EndSuccessfullyBootstrap() / EndErrorBootstrapWithErrors()

App controller / composition root
    -> PreInitialize view root'ы
    -> Register configs и external dependencies
    -> Build app model
    -> Initialize view root'ы
    -> Bind view root'ы к app model
    -> PostInitialize view root'ы
    -> Переводит приложение в первое рабочее состояние
```

Сам `AppStructure` дает контракты и orchestration points. Конкретный проект сверху решает, что именно является app model, как регистрируются зависимости и какой state считается стартовым.

## Ключевые строительные блоки

### 1. `AppBootstrap`

`AppBootstrap` - главный контракт запуска приложения. Он разбивает bootstrap на три coroutine-фазы и один `Awake`-этап:

- `AwakePrepare()`
- `PrepareBootstrapProcess(Action<bool> callback)`
- `StartBootstrapProcess(Action<bool> callback)`
- `FinalizeBootstrapProcess(Action<bool> callback)`

Каждая фаза завершает работу через callback со статусом успеха. Если все три этапа прошли успешно, вызывается `EndSuccessfullyBootstrap()`. Если хотя бы один этап провален, вызывается `EndErrorBootstrapWithErrors(...)`.

Преимущества такого подхода:

- порядок запуска не спрятан по сцене;
- тяжелую инициализацию можно раскладывать по фазам;
- граница между preparation, start и finalization становится явной;
- ошибки видны на уровне приложения, а не теряются внутри отдельных `MonoBehaviour`.

### 2. `IAppStructurePart<TAppModel>` и `AppPartRoot<TAppModel>`

Это базовый lifecycle-контракт для любой части приложения, которую нужно включить в общую композицию:

- `PreInitialize()`
- `InitializeAsync()`
- `BindAsync(TAppModel appModel)`
- `PostInitializeAsync()`

Разделение по фазам здесь принципиально:

- `PreInitialize()` подходит для локальной подготовки без зависимости от модели;
- `InitializeAsync()` подходит для асинхронной подготовки root'а или view;
- `BindAsync(...)` используется, когда app model уже собрана и ее нужно передать в view-слой;
- `PostInitializeAsync()` нужен для логики, которой требуется и готовый root, и уже привязанная модель.

Это дает проекту предсказуемый порядок связывания scene-иерархии с runtime-данными.

### 3. `AppMainViewsRoot<TState, TAppModel>`

`AppMainViewsRoot` - верхнеуровневый координатор view root'ов. Он содержит:

- serialized dictionary `state -> AppStateRoot`;
- список `StaticStateViewElement`, которые реагируют на переходы глобально;
- список `StaticViewElement`, которые живут вне state machine.

Ключевой метод - `ApplyTransferAsync(TransferInfo<TState>)`. Он:

1. отключает предыдущий state root, если есть `transferInfo.From`;
2. включает новый state root, если есть `transferInfo.To`;
3. уведомляет global static-элементы через `StaticTransfer(...)`.

Важно и то, что обработка элементов обернута в error handling. Один проблемный view element не должен silently сломать весь transition pipeline.

### 4. `AppStateRoot<TState, TAppModel>`

`AppStateRoot` - runtime-контейнер одного состояния приложения. Он управляет:

- `_stateElements` - элементами, которые активны только пока активен сам state;
- `_staticElements` - state-aware элементами, живущими внутри root'а, но обрабатываемыми отдельно.

Его задачи:

- прогонять child-элементы через общий lifecycle;
- отмечать, активен ли текущий root через `IsActive`;
- вызывать start/enable/disable hooks в правильном порядке;
- сбрасывать root через `SetDefaultValues()`.

Главные transition-методы:

- `EnableOnTransferAsync(TransferInfo<TState>)`
- `DisableOnTransferAsync(TransferInfo<TState>)`

Производные классы определяют, что именно значит "включить" и "выключить" экран: мгновенно, анимированно, с `CanvasGroup`, с tween и так далее.

### 5. `ImmediateAppStateRoot<TState, TAppModel>`

`ImmediateAppStateRoot` - базовая concrete-реализация для состояния, которое должно включаться и выключаться без промежуточной анимационной логики. Он:

- включает локальный `Canvas` перед активацией;
- выключает `Canvas` после полного отключения;
- деактивирует весь `GameObject` при reset;
- поддерживает `IsActive` в консистентном состоянии.

Это хороший default choice для экранов меню, HUD-панелей, полноэкранных экранов загрузки и фиксированных overlay.

### 6. Типы view-элементов

`AppStructure` специально делит view-поведение на три роли.

#### `StateViewElement<TState, TAppModel>`

Используется для логики, которая должна жить только во время активности конкретного state. Доступные hooks:

- `PreInitialize()`
- `InitializeAsync()`
- `BindAsync(...)`
- `PostInitializeAsync()`
- `OnStartStateEnable(...)`
- `EnableElementAsync(...)`
- `OnStartScreenDisable(...)`
- `DisableElementAsync(...)`
- `OnCompletelyDisable(...)`

Особенно важен паттерн подписок:

- `SubscribeOnly()`
- `UnsubscribeOnly()`

За счет этого подписки и отписки становятся частью архитектуры перехода, а не случайным кодом внутри `OnEnable()` и `OnDisable()`. Для Unity-проектов это большой плюс: меньше "висячих" event handler'ов, меньше дублирующихся подписок и меньше трудноуловимых lifecycle-багов.

#### `StaticStateViewElement<TState, TAppModel>`

Подходит для элементов, которые должны жить сквозь несколько состояний, но при этом реагировать на переходы. Поддерживает:

- `Enable(...)`
- `Disable(...)`
- `StaticTransfer(...)`

Типовые кейсы:

- orientation manager;
- persistent navigation layer;
- глобальный overlay;
- always-mounted UI, которому нужно знать текущий state.

#### `StaticViewElement<TAppModel>`

Используется для always-on логики, которой не важен сам факт перехода между состояниями, но нужен доступ к app model.

### 7. `TransferInfo<TState>`

`TransferInfo<TState>` - канонический payload любого перехода. Он содержит:

- `From`
- `To`
- `IsFromBack`
- `Parameters`

Также предоставляет:

- `SwapStates()`
- `SwapStates(bool isFromBack)`
- `None`
- `ValidBack`

Почему это важно:

- у любого перехода единая форма данных;
- fullscreen flow и popup flow используют один и тот же транспортный объект;
- параметры можно передавать вместе с переходом, не связывая напрямую два state root'а между собой.

### 8. State machine

В репозитории есть две базовые модели навигации.

#### `GoBackSupportStateMachine<TState>`

Используется для основного flow приложения. Хранит:

- `CurrentState`
- `LastNotNoneState`
- историю переходов `_transferHistory`

Поддерживает:

- `GoToState(...)`
- `GoBack()`
- `IsValidBack(...)`

Базовая реализация оставляет точку расширения: проект может переопределить `IsValidBack(...)` и сам определить, где back-navigation разрешена, а где нет.

#### `OpenCloseStateMachine<TState>`

Используется для stacked-состояний: popup, modal, temporary overlay. Хранит:

- `LastOpenedState`
- упорядоченный набор открытых состояний

Поддерживает:

- `OpenState(...)`
- `CloseLastState()`
- `CloseState(...)`

Это не "тяжелый" навигационный стек, а практичная lightweight-модель для открытия и закрытия поверх основного экрана.

### 9. Navigation helpers

#### `EscapeManager`

`EscapeManager` - минимальный глобальный dispatcher:

- `EscapePressed`
- `Escape(source, order)`

Он полезен, когда нескольким UI-слоям нужен общий back/escape signal, но прямые зависимости между ними нежелательны.

#### `FocusManager` и `DefaultFocusElement`

`DefaultFocusElement` - optional helper для keyboard/controller navigation. Он регистрирует focus target и приоритет слоя, чтобы активный UI сохранял корректный selected object в `EventSystem`.

Это особенно полезно для:

- меню;
- modal dialog;
- popup navigation на геймпаде;
- UI, где важно не терять текущий focus при переключении экранов.

Важно: `DefaultFocusElement` не полностью изолирован. Он зависит от `DingoProjectAppStructure.Core.AppRootCore` и `DingoUnityExtensions`, то есть относится скорее к интеграционному слою экосистемы, чем к абсолютно автономному generic-core.

### 10. `AppInputLocker<TLockMessage>`

`AppInputLocker` реализует компактную блокировку взаимодействия через bit-mask флаги.

Ключевые свойства:

- несколько причин блокировки могут существовать одновременно;
- только первый активный lock вызывает `OnLockEnable(...)`;
- только снятие последнего lock вызывает `OnLockDisable()`.

Это хорошо подходит для:

- transition animation;
- async loading;
- modal blocker;
- защиты от double-click / double-submit;
- блокировки input во время критических операций.

### 11. Подсистема `AdaptiveView`

Папка `AdaptiveView` добавляет orientation-aware поведение поверх state-based архитектуры.

Ключевые типы:

- `ScreenOrientationManage<TState, TAppModel>`
- `RotatableAppStateImmediateStateRoot<TState, TAppModel>`
- `RectPositionByState`
- `AdaptByStateElement`

Что она делает:

- переключает screen orientation в portrait-only, landscape-only или auto-rotation режим;
- переиспользует заранее "запеченные" layout state'ы;
- позволяет добавлять custom adaptation logic через `AdaptByStateElement`.

Ценность этой подсистемы в том, что адаптивность остается рядом с жизненным циклом экрана, а не размазывается по отдельным UI-компонентам.

### 12. `SerializedCollections`

`AppMainViewsRoot` опирается на serialized dictionary для Inspector-friendly конфигурации состояний. Поэтому в репозиторий включены runtime/editor-исходники `AYellowpaper.SerializedCollections` в папке `SerializedCollections/`.

Практическая польза:

- mapping `state -> root` настраивается в Inspector;
- baked layout state'ы можно хранить в сериализуемом виде;
- ключевые editor-данные остаются читаемыми и редактируемыми без вручную написанных switch/case.

## Lifecycle перехода по состояниям

Типовой fullscreen transfer выглядит так:

```text
State machine создает TransferInfo<TState>
    -> AppMainViewsRoot.ApplyTransferAsync(transferInfo)
        -> previous AppStateRoot.DisableOnTransferAsync(...)
            -> StartDisable(...)
            -> StateViewElement.OnStartScreenDisable(...)
            -> StateViewElement.DisableElementAsync(...)
            -> DisableCompletely(...)
            -> StateViewElement.OnCompletelyDisable(...)
        -> next AppStateRoot.EnableOnTransferAsync(...)
            -> StaticStateViewElement.Enable(...)
            -> StartEnable(...)
            -> StateViewElement.OnStartStateEnable(...)
            -> StateViewElement.EnableElementAsync(...)
        -> StaticStateViewElement.StaticTransfer(...)
```

Эта последовательность важна, потому что именно она дает стабильные точки для:

- подписки и отписки от событий;
- запуска анимаций в производных root'ах;
- передачи параметров в новый экран;
- обновления глобального UI, зависящего от текущего state.

## Структура репозитория

```text
AppStructure/
    AppBootstrap.cs
    AppMainViewsRoot.cs
    AppStateRoot.cs
    ImmediateAppStateRoot.cs
    TransferInfo.cs
    AdaptiveView/
    BaseElements/
    BaseNavigation/
    InputLocker/
    SerializedCollections/
    StateMachines/
    Utils/
```

### Ответственность папок

- `AdaptiveView/` - orientation-aware helpers и layout baking.
- `BaseElements/` - базовые lifecycle-контракты для root'ов и view element'ов.
- `BaseNavigation/` - escape/focus helpers.
- `InputLocker/` - lock/unlock abstraction с flag-based координацией.
- `SerializedCollections/` - встроенная поддержка serialized dictionary для runtime и editor.
- `StateMachines/` - модели истории переходов и open/close stack.
- `Utils/` - безопасная обработка коллекций и typed registry.

## Типовой сценарий интеграции

Обычно `AppStructure` подключается так:

1. Создается project-specific bootstrap на базе `AppBootstrap`.
2. Создается project-specific model root и слой регистрации зависимостей.
3. Создается главный view root через наследование от `AppMainViewsRoot<TState, TAppModel>`.
4. Экраны реализуются через `ImmediateAppStateRoot<TState, TAppModel>` или через собственный анимируемый root.
5. Локальная логика экранов раскладывается по `StateViewElement`, глобальная - по static-элементам.
6. Переходы запускаются через одну из встроенных state machine.

### Типичный higher-level слой поверх AppStructure

Поверх `AppStructure` обычно появляется дополнительный composition-уровень, который добавляет:

- специализированные root'ы вроде `AppStateElementsRoot`;
- app/controller-слой вроде `AppStateController` и `AppPopupStateController`;
- регистрацию моделей, view model и внешних зависимостей;
- project facade или singleton-точку входа наподобие `G`.

Хороший пример такого слоя - `DingoProjectAppStructure`: он не является частью core `AppStructure`, но хорошо показывает тип расширения, для которого текущий репозиторий и задуман.

## Преимущества решения

### 1. Предсказуемый запуск приложения

Bootstrap разбит на четкие фазы. Команде не нужно восстанавливать startup order по десяткам `Awake()` и `Start()` по всей сцене.

### 2. Явное владение экраном

У каждого экрана появляется собственный root с понятным lifecycle. Ответственность читается и в коде, и в иерархии сцены.

### 3. Разделение ответственности

Репозиторий разделяет:

- композицию приложения;
- transition logic;
- per-screen behavior;
- global always-on UI;
- navigation helpers;
- optional adaptation systems.

Это снижает типичную для Unity проблему, когда один `MonoBehaviour` внезапно начинает контролировать половину приложения.

### 4. Безопаснее работа с подписками

`StateViewElement` и `StaticStateViewElement` встраивают `SubscribeOnly()` / `UnsubscribeOnly()` прямо в lifecycle. Это уменьшает число ghost-listener'ов, двойных подписок и трудноотлавливаемых side effect'ов.

### 5. Единый формат перехода

`TransferInfo<TState>` делает transition flow data-driven. Один и тот же подход работает для экранов, popup'ов, back navigation и передачи параметров.

### 6. Расширяемость без переписывания core

Базовые типы generic и по ключу состояния, и по типу app model. Это позволяет:

- использовать `string`, enum или свои state identifiers;
- делать собственные animated root'ы;
- добавлять model/config/DI-слой сверху;
- не ломать при этом сам transition contract.

### 7. Inspector-friendly настройка

За счет serialized dictionary связка `state -> root` настраивается прямо в Unity Inspector, а не прячется в больших switch-блоках.

### 8. Хорошо масштабируется на сложный UI-flow

Разделение между fullscreen flow и popup open/close flow помогает архитектуре оставаться понятной даже при росте количества меню, overlay и modal window.

### 9. Production-oriented детали уже учтены

Input lock, focus control, escape routing и orientation adaptation встроены в архитектурный слой, а не добавляются хаотично постфактум.

### 10. Удобная база для более высокого app-слоя

`AppStructure` хорошо работает как foundation для следующего уровня архитектуры. Project-specific controllers и model layer можно строить поверх стабильного state/lifecycle-core, а не придумывать новую схему orchestration под каждую игру.

## Зависимости

### Репозитории AppSDK

Ниже перечислены зависимости, которые видны по коду и отражены в текущей конфигурации сабмодулей.

| Репозиторий | Зачем нужен | URL | Ветка в `.gitmodules` |
| --- | --- | --- | --- |
| [`DingoUnityExtensions`](https://github.com/DingoBite/DingoUnityExtensions) | Используется в `AdaptiveView` и focus-management helper'ах. | `https://github.com/DingoBite/DingoUnityExtensions` | `dev` |
| [`DingoProjectAppStructure`](https://github.com/DingoBite/DingoProjectAppStructure.git) | Используется как higher-level composition layer; `DefaultFocusElement` прямо зависит от `DingoProjectAppStructure.Core.AppRootCore`. | `https://github.com/DingoBite/DingoProjectAppStructure.git` | не указана в `.gitmodules` |

Примечания:

- отсутствие ветки в `.gitmodules` означает, что сабмодуль не закреплен за конкретной веткой на уровне конфигурации и обычно используется через pinned commit;
- core-абстракции `AppStructure` можно переиспользовать шире, чем эти интеграционные зависимости.

### Встроенные и package-level зависимости

| Зависимость | Формат | Назначение |
| --- | --- | --- |
| `AYellowpaper.SerializedCollections` | vendored source внутри `SerializedCollections/` | Inspector-friendly serialized dictionary и связанные editor/runtime utility. |
| `TMPro` | Unity package dependency | Нужен для baking layout-состояний текстовых элементов. |
| `NaughtyAttributes` | optional editor dependency | Используется для debug/editor-кнопок в adaptive-компонентах. |

## Установка

### Git submodule

Рекомендуемый путь:

- `Assets/AppSDK/AppStructure`

Общее правило:

- сохраняйте Unity `.meta` файлы;
- не ломайте GUID при переносе папки;
- переносите весь каталог целиком, включая `SerializedCollections/`.

### Копирование в существующий Unity-проект

Если submodule не используется, папку можно скопировать в `Assets/`, сохранив структуру каталогов без изменений.

## Когда стоит использовать AppStructure

Используйте его, если проекту нужны:

- несколько экранов или состояний, которые будут расти;
- повторяемый и контролируемый bootstrap order;
- отдельный fullscreen flow и popup flow;
- view-слой, который должен аккуратно bind'иться к model layer;
- screen-specific подписки, не живущие вечно;
- foundation-слой для более высокого app-архитектурного уровня.

Если проект очень маленький и состоит из пары статичных экранов, такой слой может быть избыточен. Основная ценность `AppStructure` появляется там, где начинают иметь значение startup order, state flow и долговечность UI-архитектуры.

## Связанная документация

- Английская версия: `README.md`

## Итог

`AppStructure` - это не просто набор utility-скриптов. Это архитектурный шов, который превращает UI/application flow в управляемую систему с явным lifecycle, явными переходами и понятными точками расширения. Главная ценность решения в том, что сложность не расползается вместе с ростом проекта: экраны остаются изолированными, инициализация остается предсказуемой, а более высокий игровой код строится поверх стабильного app-framework, а не поверх набора разрозненных scene-script'ов.
