# PinionCore NetSync Suite

PinionCore NetSync is a Unity-focused networking toolkit for synchronizing gameplay state across multiple transport layers. This repository bundles the shipping runtime package, a Unity demo project, and the shared transport layer submodule that powers TCP and WebSocket connectivity.

## Repository layout

| Path | Description |
| --- | --- |
| `PinionCore.NetSync.Develop/` | Unity project used for feature development, validation, and sample scenes (`Assets/PinionCore`). |
| `PinionCore.NetSync.Package/` | Unity Package Manager (UPM) distribution of the NetSync runtime, editor tooling, analyzers, samples, and NUnit tests. |
| `PinionCore.Remote/` | Git submodule that implements the underlying transport layer (TCP, WebSocket, pooling utilities). NetSync consumes the same version of this library. |
| `Publishs/` | Generated packages ready for distribution or CI verification. |
| `notes/` | Scratch documents and internal research materials. |

## Getting started

1. Clone the repository and initialize submodules:

   ```bash
   git submodule update --init --recursive
   ```

2. Use Unity 2022.2 or newer. Open `PinionCore.NetSync.Develop/PinionCore.NetSync.Develop.sln` from Unity Hub or launch the project folder directly.
3. The solution contains a dedicated development assembly definition (`PinionCore.NetSync.Develop`) that references the runtime package for rapid iteration.

## Exploring the demos

- **Sample 1 (`Assets/PinionCore/Sample1`)** demonstrates how to wire the `Server`, `Client`, and transport-specific connectors for Standalone, TCP, and WebSocket configurations. Use the `Main` scene to switch protocols, or open `Client`/`Server` scenes individually to focus on one side.
- **Sample 2 – Chat (`Assets/PinionCore/Sample2-Chat`)** showcases a gameplay-oriented loop with protocol switching and UI feedback layered on the NetSync runtime.

Each sample scene includes prefabs configured with the package components. Adjust inspector properties to match your network endpoints before entering Play Mode.

A hosted build of Sample 2 is available at <https://proxy.pinioncore.dpdns.org/sample2>, allowing you to exercise the connection flow without cloning the project.

## Working on the package

The `PinionCore.NetSync.Package` folder mirrors the published UPM package:

- `Runtime/Scripts/Links` encapsulates transport adapters:
  - `Standalone/` provides an in-editor loopback connector.
  - `Tcp/` and `Web/` bridge to TCP sockets and WebSocket streams via `PinionCore.Remote`.
  - `Client`, `Server`, `Linstener`, and `ProtocolCreator` manage the protocol lifecycle and binder registration.
- `Runtime/Scripts/Syncs` implements the state replication pipeline:
  - `Souls/` author authoritative state producers and binder lifecycle.
  - `Ghosts/` consume replicated data and expose notifier utilities.
  - `Protocols/Trackers` provide interpolation, compression, and zip encoding for transform data.
- `Editor/Scripts` adds inspectors and tooling that surface runtime metrics.
- `Tests/` contains NUnit coverage for tracker math and networking primitives.
- `Analyzers/` stores Roslyn analyzers enforced by CI.

## Building and testing

Use Unity batch mode for repeatable builds:

```powershell
"<UnityEditorPath>\Unity.exe" -projectPath PinionCore.NetSync.Develop -quit -batchmode -logFile Logs/ci.log
```

Run edit-mode tests from the editor or via CLI:

```powershell
"<UnityEditorPath>\Unity.exe" -projectPath PinionCore.NetSync.Develop -quit -batchmode -runTests -testPlatform EditMode -testResults Logs/editmode.xml
```

Add `-testPlatform PlayMode` when required. Package-specific NUnit tests live under `PinionCore.NetSync.Package/Tests` and execute when the package is imported into the development project.

## Release workflow

1. Update `PinionCore.Remote` with `git submodule update --remote --merge` and confirm the resulting SHA.
2. Ensure `Packages/com.pinioncore.netsync/package.json` still references the correct version.
3. Rebuild distributables into `Publishs/` using Unity's package export tools.
4. Record user-facing changes in `PinionCore.NetSync.Package/CHANGELOG.md`.

## Coding standards and contributions

- Target C# 9 with four-space indentation and explicit types unless an existing scope uses `var`.
- Keep assembly definition dependencies minimal and run the analyzers (`PinionCore.NetSync.Package/Analyzers`) before committing.
- Follow Conventional Commits (for example, `feat: add web reconnect tracker`).
- Submit Unity assets with supporting screenshots or recordings when visuals change.

## License

PinionCore NetSync ships under the terms of the included `LICENSE`.
