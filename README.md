# AppStructure

A small state-based framework for building Unity applications and games.

## What it is

**AppStructure** is a set of C# scripts for Unity that helps you structure an app/game around **states** (a state-based architecture) and common subsystems such as navigation, UI roots/layers, utilities, and more.

The repository includes root-level classes such as `AppBootstrap`, `AppStateRoot`, `ImmediateAppStateRoot`, `AppMainViewsRoot`, and a data transfer object `TransferInfo`.

## Why

- Keep app startup in one clear place (bootstrap).
- Describe the application lifecycle explicitly through states.
- Make transitions between states simpler and more predictable.
- Keep navigation and UI structure separated from gameplay/business logic.

## Contents

Key directories/modules (as of the current branch):

- `StateMachines` — base and helper entities for state machines.
- `BaseNavigation` — base navigation (routing, stack, transitions).
- `AdaptiveView` — adaptive/rebuildable views.
- `InputLocker` — input blocking during transitions/animations.
  
## Branch

Main branch for this repo: `string-as-key-refactor`.

## Installation

### Option 1. Copy into a project
1. Copy the repository folders/files into your Unity project `Assets/` (keep `.meta` files).
2. Do not break `.meta` GUIDs while moving files.

### Option 2. Git submodule
Add as a submodule so updates are easy, e.g. into:
- `Assets/ThirdParty/AppStructure/`

## Quick start (conceptual)

1. Create an entry point in your scene:
   - a dedicated `GameObject` (e.g. `App`)
   - add `AppBootstrap` (or your wrapper/inheritor) as a component

2. Define your set of application states:
   - menu
   - loading
   - gameplay
   - pause
   - etc.

3. Configure the state root:
   - via `AppStateRoot` / `ImmediateAppStateRoot` depending on how you want transitions to behave

