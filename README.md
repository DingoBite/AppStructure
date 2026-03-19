# AppStructure

`AppStructure` is a state-driven application layer for Unity. It defines an explicit bootstrap flow, a predictable screen transition model, reusable view roots, and a small set of infrastructure components that keep UI flow, navigation, and application composition separate from gameplay logic and domain systems.

The repository is designed as a foundation layer for a higher-level composition package. In practice, it solves the question of how the application starts, switches states, binds views to the model, and avoids turning screen code into a mix of random `SetActive` calls, subscriptions, and hand-written transitions.

## What Problem It Solves

In many Unity projects, growing UI and application flow lead to the same set of problems:

- startup logic is spread across dozens of `Awake()` and `Start()` methods;
- screen transitions are implemented through inconsistent `SetActive` calls;
- screen logic, event subscriptions, and animations are mixed inside a single `MonoBehaviour`;
- popups, back-flow, input locking, and focus navigation are duplicated from feature to feature.

`AppStructure` replaces that with a small but stable set of contracts:

- one bootstrap entry point;
- one transition payload format;
- one lifecycle for view roots and their elements;
- separate state machines for full-screen flow and stacked flow;
- built-in extension points for focus, escape, orientation, and input locking.

## Place in the Architecture Stack

```text
Project-specific code
    -> game states, models, ECS/network setup, feature logic

Higher-level composition layer
    -> composition root, model and view model registration,
       app/popup controllers, animated state roots

AppStructure
    -> bootstrap, state roots, transfer pipeline,
       state machines, navigation helpers, adaptive view,
       input lock, generic element lifecycle
```

`AppStructure` sits between raw Unity orchestration and project-specific application logic. It is not a gameplay framework and not a DI container. It is a lifecycle and state-management layer for the application itself.

## Technical Overview

### Core Responsibility Areas

| Area | Main types | Responsibility |
| --- | --- | --- |
| Bootstrap | `AppBootstrap` | Starts the application in explicitly separated phases. |
| Root lifecycle | `IAppStructurePart<TAppModel>`, `AppPartRoot<TAppModel>` | Defines the common lifecycle: `PreInitialize`, `InitializeAsync`, `BindAsync`, `PostInitializeAsync`. |
| State orchestration | `AppMainViewsRoot<TState, TAppModel>` | Stores the state root dictionary and applies transitions between roots. |
| Single-state root | `AppStateRoot<TState, TAppModel>` | Manages state-specific and static elements for one screen. |
| Immediate screen implementation | `ImmediateAppStateRoot<TState, TAppModel>` | Provides a ready-to-use "enable now / disable now" state root with `Canvas` control. |
| Leaf view elements | `StateViewElement<TState, TAppModel>`, `StaticStateViewElement<TState, TAppModel>`, `StaticViewElement<TAppModel>` | Splits behavior into state-bound, transition-aware, and always-on parts. |
| Transition payload | `TransferInfo<TState>` | Carries `From`, `To`, `IsFromBack`, and `Parameters`. |
| Navigation state machines | `GoBackSupportStateMachine<TState>`, `OpenCloseStateMachine<TState>` | Support linear flow and stacked open/close flow for popups. |
| Input locking | `AppInputLocker<TLockMessage>` | Centralizes interaction locking through bit-mask flags. |
| Adaptive view | `ScreenOrientationManage<TState, TAppModel>`, `RotatableAppStateImmediateStateRoot<TState, TAppModel>`, `RectPositionByState` | Controls screen orientation and layout states. |
| Utilities | `Extensions`, `RootByGenericTypes<TValue>`, `LINQExtensions` | Provide safe collection processing and typed registries. |

### High-Level Runtime Flow

```text
AppBootstrap
    -> AwakePrepare()
    -> PrepareBootstrapProcess()
    -> StartBootstrapProcess()
    -> FinalizeBootstrapProcess()
    -> EndSuccessfullyBootstrap() / EndErrorBootstrapWithErrors()

App controller / composition root
    -> PreInitialize view roots
    -> Register configs and external dependencies
    -> Build app model
    -> Initialize view roots
    -> Bind view roots to the app model
    -> PostInitialize view roots
    -> Move the application into the first working state
```

`AppStructure` itself provides contracts and orchestration points. A concrete project decides what the app model is, how dependencies are registered, and which state should be the starting one.

## Main Building Blocks

### 1. `AppBootstrap`

`AppBootstrap` is the main application startup contract. It splits bootstrap into three coroutine phases plus one `Awake` stage:

- `AwakePrepare()`
- `PrepareBootstrapProcess(Action<bool> callback)`
- `StartBootstrapProcess(Action<bool> callback)`
- `FinalizeBootstrapProcess(Action<bool> callback)`

Each phase finishes through a callback with a success flag. If all three phases succeed, `EndSuccessfullyBootstrap()` is called. If at least one phase fails, `EndErrorBootstrapWithErrors(...)` is called.

Why this matters:

- startup order is not hidden across the scene;
- heavy initialization can be distributed across phases;
- the boundary between preparation, start, and finalization becomes explicit;
- failures are visible at the application boundary instead of disappearing inside individual `MonoBehaviour`s.

### 2. `IAppStructurePart<TAppModel>` and `AppPartRoot<TAppModel>`

This is the base lifecycle contract for any application part that participates in composition:

- `PreInitialize()`
- `InitializeAsync()`
- `BindAsync(TAppModel appModel)`
- `PostInitializeAsync()`

The phase split is intentional:

- `PreInitialize()` is for local setup that does not require the model;
- `InitializeAsync()` is for asynchronous root or view preparation;
- `BindAsync(...)` is used once the app model exists and must be passed into the view layer;
- `PostInitializeAsync()` is for logic that needs both a ready root and a bound model.

This gives the project a predictable way to connect scene hierarchy with runtime data.

### 3. `AppMainViewsRoot<TState, TAppModel>`

`AppMainViewsRoot` is the top-level coordinator for view roots. It contains:

- a serialized dictionary `state -> AppStateRoot`;
- a list of `StaticStateViewElement` objects that react globally to transitions;
- a list of `StaticViewElement` objects that live outside the state machine.

Its key method is `ApplyTransferAsync(TransferInfo<TState>)`. It:

1. disables the previous state root if `transferInfo.From` exists;
2. enables the new state root if `transferInfo.To` exists;
3. notifies global static elements through `StaticTransfer(...)`.

Element processing is also wrapped with error handling. One broken view element should not silently break the entire transition pipeline.

### 4. `AppStateRoot<TState, TAppModel>`

`AppStateRoot` is the runtime container for one application state. It manages:

- `_stateElements` - elements that are active only while the state is active;
- `_staticElements` - state-aware elements that live inside the root but are handled separately.

Its responsibilities are:

- push child elements through the shared lifecycle;
- track whether the current root is active through `IsActive`;
- call start/enable/disable hooks in the correct order;
- reset the root through `SetDefaultValues()`.

Main transition methods:

- `EnableOnTransferAsync(TransferInfo<TState>)`
- `DisableOnTransferAsync(TransferInfo<TState>)`

Derived classes decide what it means to "enable" or "disable" a screen: instantly, through animation, with a `CanvasGroup`, with tweens, and so on.

### 5. `ImmediateAppStateRoot<TState, TAppModel>`

`ImmediateAppStateRoot` is the base concrete implementation for a state that should enable and disable without extra animation logic. It:

- enables the local `Canvas` before activation;
- disables the `Canvas` after full shutdown;
- deactivates the entire `GameObject` on reset;
- keeps `IsActive` consistent.

It is a good default choice for menu screens, HUD panels, full-screen loading screens, and fixed overlays.

### 6. View Element Types

`AppStructure` intentionally splits view behavior into three roles.

#### `StateViewElement<TState, TAppModel>`

Use this for logic that should live only while a specific state is active. Available hooks:

- `PreInitialize()`
- `InitializeAsync()`
- `BindAsync(...)`
- `PostInitializeAsync()`
- `OnStartStateEnable(...)`
- `EnableElementAsync(...)`
- `OnStartScreenDisable(...)`
- `DisableElementAsync(...)`
- `OnCompletelyDisable(...)`

The subscription pattern is especially important:

- `SubscribeOnly()`
- `UnsubscribeOnly()`

This makes subscribe/unsubscribe timing part of the transition architecture instead of random code hidden inside `OnEnable()` and `OnDisable()`. In Unity projects, this is a major benefit: fewer ghost event handlers, fewer duplicate subscriptions, and fewer hard-to-debug lifecycle issues.

#### `StaticStateViewElement<TState, TAppModel>`

Use this for elements that should live across multiple states but still react to transitions. It supports:

- `Enable(...)`
- `Disable(...)`
- `StaticTransfer(...)`

Typical cases:

- orientation managers;
- persistent navigation layers;
- global overlays;
- always-mounted UI that still needs to know the current state.

#### `StaticViewElement<TAppModel>`

Use this for always-on logic that does not care about state transitions but still needs access to the app model.

### 7. `TransferInfo<TState>`

`TransferInfo<TState>` is the canonical payload for any transition. It contains:

- `From`
- `To`
- `IsFromBack`
- `Parameters`

It also provides:

- `SwapStates()`
- `SwapStates(bool isFromBack)`
- `None`
- `ValidBack`

Why this matters:

- every transition has one consistent data shape;
- full-screen flow and popup flow use the same transport object;
- parameters can travel with the transition without tightly coupling two state roots.

### 8. State Machines

The repository includes two core navigation models.

#### `GoBackSupportStateMachine<TState>`

This is used for the main application flow. It stores:

- `CurrentState`
- `LastNotNoneState`
- transition history in `_transferHistory`

It supports:

- `GoToState(...)`
- `GoBack()`
- `IsValidBack(...)`

The base implementation deliberately leaves an extension point: a project can override `IsValidBack(...)` and define where back navigation is allowed or blocked.

#### `OpenCloseStateMachine<TState>`

This is used for stacked states such as popups, modals, and temporary overlays. It stores:

- `LastOpenedState`
- the ordered set of opened states

It supports:

- `OpenState(...)`
- `CloseLastState()`
- `CloseState(...)`

This is not a heavy navigation stack. It is a practical lightweight model for opening and closing layers on top of the main screen.

### 9. Navigation Helpers

#### `EscapeManager`

`EscapeManager` is a minimal global dispatcher:

- `EscapePressed`
- `Escape(source, order)`

It is useful when several UI layers need a shared back/escape signal but direct dependencies between them are undesirable.

#### `FocusManager` and `DefaultFocusElement`

`DefaultFocusElement` is an optional helper for keyboard/controller navigation. It registers a focus target and a layer priority so that active UI keeps a valid selected object in `EventSystem`.

This is especially useful for:

- menus;
- modal dialogs;
- gamepad popup navigation;
- UI where current focus must not be lost during screen changes.

Important: `DefaultFocusElement` is not fully isolated. It depends on `DingoProjectAppStructure.Core.AppRootCore` and `DingoUnityExtensions`, so it belongs more to the ecosystem integration layer than to a completely standalone generic core.

### 10. `AppInputLocker<TLockMessage>`

`AppInputLocker` implements compact interaction locking through bit-mask flags.

Key properties:

- multiple lock reasons can exist at the same time;
- only the first active lock triggers `OnLockEnable(...)`;
- only the release of the final lock triggers `OnLockDisable()`.

This works well for:

- transition animations;
- async loading;
- modal blockers;
- protection from double-click / double-submit;
- input blocking during critical operations.

### 11. `AdaptiveView` Subsystem

The `AdaptiveView` folder adds orientation-aware behavior on top of the state-based architecture.

Key types:

- `ScreenOrientationManage<TState, TAppModel>`
- `RotatableAppStateImmediateStateRoot<TState, TAppModel>`
- `RectPositionByState`
- `AdaptByStateElement`

What it does:

- switches screen orientation to portrait-only, landscape-only, or auto-rotation;
- reuses baked layout states;
- allows custom adaptation logic through `AdaptByStateElement`.

The value of this subsystem is that responsiveness stays close to screen lifecycle instead of being spread across unrelated UI components.

### 12. `SerializedCollections`

`AppMainViewsRoot` relies on serialized dictionaries for Inspector-friendly state configuration. Because of that, the repository includes the runtime/editor sources of `AYellowpaper.SerializedCollections` inside `SerializedCollections/`.

Practical value:

- `state -> root` mapping is configured in the Inspector;
- baked layout states can be stored in a serializable form;
- key editor data stays readable and editable without large switch/case blocks.

## Transition Lifecycle

A typical full-screen transfer looks like this:

```text
State machine creates TransferInfo<TState>
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

This sequence matters because it creates stable points for:

- subscribing and unsubscribing from events;
- starting animations in derived roots;
- passing parameters into the next screen;
- updating global UI that depends on the current state.

## Repository Structure

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

### Folder Responsibilities

- `AdaptiveView/` - orientation-aware helpers and layout baking.
- `BaseElements/` - base lifecycle contracts for roots and view elements.
- `BaseNavigation/` - escape/focus helpers.
- `InputLocker/` - lock/unlock abstraction with flag-based coordination.
- `SerializedCollections/` - embedded serialized dictionary support for runtime and editor.
- `StateMachines/` - transition-history and open/close stack models.
- `Utils/` - safe collection processing and typed registries.

## Typical Integration Scenario

`AppStructure` is usually integrated like this:

1. Create a project-specific bootstrap on top of `AppBootstrap`.
2. Create a project-specific model root and dependency-registration layer.
3. Create the main view root by inheriting from `AppMainViewsRoot<TState, TAppModel>`.
4. Implement screens through `ImmediateAppStateRoot<TState, TAppModel>` or a custom animated root.
5. Place local screen logic in `StateViewElement`, and global logic in static elements.
6. Trigger transitions through one of the built-in state machines.

### Typical Higher-Level Layer on Top of AppStructure

A higher-level composition package on top of `AppStructure` usually adds:

- specialized roots such as `AppStateElementsRoot`;
- controller layers such as `AppStateController` and `AppPopupStateController`;
- registration of models, view models, and external dependencies;
- a project facade or singleton-style entry point such as `G`.

`DingoProjectAppStructure` is a good example of that layer. It is not part of the core `AppStructure` package, but it shows the exact type of extension that this repository is designed to support.

## Advantages of the Solution

### 1. Predictable application startup

Bootstrap is split into clear phases. The team does not need to reconstruct startup order from dozens of `Awake()` and `Start()` methods across the scene.

### 2. Explicit screen ownership

Each screen gets its own root with a clear lifecycle. Responsibility is visible in both code and scene hierarchy.

### 3. Separation of concerns

The repository separates:

- application composition;
- transition logic;
- per-screen behavior;
- global always-on UI;
- navigation helpers;
- optional adaptation systems.

This reduces the classic Unity problem where one `MonoBehaviour` unexpectedly starts controlling half of the application.

### 4. Safer subscription handling

`StateViewElement` and `StaticStateViewElement` build `SubscribeOnly()` / `UnsubscribeOnly()` directly into the lifecycle. That reduces ghost listeners, duplicate subscriptions, and difficult side effects.

### 5. One transition format

`TransferInfo<TState>` makes transition flow data-driven. The same pattern works for screens, popups, back navigation, and parameter passing.

### 6. Extensibility without rewriting the core

The base types are generic both by state key and by app model type. That allows projects to:

- use `string`, enums, or custom state identifiers;
- implement custom animated roots;
- add model/config/DI layers on top;
- keep the transition contract intact.

### 7. Inspector-friendly setup

Because of serialized dictionaries, the `state -> root` mapping is configured directly in the Unity Inspector instead of being hidden inside large switch blocks.

### 8. Scales well for complex UI flow

The split between full-screen flow and popup open/close flow helps the architecture stay understandable as the number of menus, overlays, and modal windows grows.

### 9. Production-oriented details are already considered

Input lock, focus control, escape routing, and orientation adaptation are built into the architectural layer instead of being added chaotically afterwards.

### 10. Strong foundation for a higher-level app layer

`AppStructure` works well as the foundation for the next architecture level. Project-specific controllers and model layers can be built on top of a stable state/lifecycle core instead of inventing a new orchestration pattern for every project.

## Dependencies

### AppSDK Repositories

Below are the dependencies that are visible in the code and reflected by the current submodule configuration.

| Repository | Why it is needed | URL | Branch in `.gitmodules` |
| --- | --- | --- | --- |
| [`DingoUnityExtensions`](https://github.com/DingoBite/DingoUnityExtensions) | Used by `AdaptiveView` and focus-management helpers. | `https://github.com/DingoBite/DingoUnityExtensions` | `dev` |
| [`DingoProjectAppStructure`](https://github.com/DingoBite/DingoProjectAppStructure.git) | Used as a higher-level composition layer; `DefaultFocusElement` directly depends on `DingoProjectAppStructure.Core.AppRootCore`. | `https://github.com/DingoBite/DingoProjectAppStructure.git` | not specified in `.gitmodules` |

Notes:

- when no branch is specified in `.gitmodules`, the submodule is not pinned to a named branch at config level and is typically consumed through a pinned commit;
- the core `AppStructure` abstractions can still be reused more broadly than these integration dependencies.

### Embedded and Package-Level Dependencies

| Dependency | Form | Purpose |
| --- | --- | --- |
| `AYellowpaper.SerializedCollections` | vendored source inside `SerializedCollections/` | Inspector-friendly serialized dictionaries and related editor/runtime utilities. |
| `TMPro` | Unity package dependency | Required for baking text-element layout states. |
| `NaughtyAttributes` | optional editor dependency | Used for debug/editor buttons in adaptive components. |

## Installation

### Git submodule

Recommended path:

- `Assets/AppSDK/AppStructure`

General rules:

- keep Unity `.meta` files;
- do not break GUIDs when moving the folder;
- move the entire directory, including `SerializedCollections/`.

### Copy into an existing Unity project

If you are not using a submodule, copy the folder into `Assets/` and keep the directory structure unchanged.

## When to Use AppStructure

Use it when your project needs:

- multiple screens or states that will grow over time;
- repeatable and controlled bootstrap order;
- separate full-screen flow and popup flow;
- a view layer that should bind cleanly to a model layer;
- screen-specific subscriptions that should not live forever;
- a foundation layer for a higher-level application architecture.

If the project is very small and contains only a couple of static screens, this layer may be excessive. The main value of `AppStructure` appears once startup order, state flow, and long-term UI architecture begin to matter.

## Related Documentation

- Russian version: `README_ru.md`

## Summary

`AppStructure` is not just a set of utility scripts. It is an architectural seam that turns UI/application flow into a managed system with explicit lifecycle, explicit transitions, and clear extension points. Its main value is that complexity does not spread uncontrollably as the project grows: screens remain isolated, initialization remains predictable, and higher-level game code can build on top of a stable application framework instead of a collection of unrelated scene scripts.
