# Graph Report - modded  (2026-08-14)

## Corpus Check
- 18 files · ~28,266 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 322 nodes · 546 edges · 16 communities (15 shown, 1 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 28 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `6ee02479`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- VoipController
- SftNetwork
- MenuVoipHUD
- TRL_SpeakFromTarkov.Audio
- VoiceCalibrationHUD
- AudioFilter
- VoipProcessor
- GameSessionPatcher
- MicrophoneCapturer
- VoIPPlugin
- SftAudioPacketV2
- InRaidVoipHUD
- VoipHUD
- TRL-SpeakFromTarkov.csproj
- SpeakFromTarkov

## God Nodes (most connected - your core abstractions)
1. `VoipController` - 34 edges
2. `MenuVoipHUD` - 33 edges
3. `SftNetwork` - 30 edges
4. `VoiceCalibrationHUD` - 26 edges
5. `AudioFilter` - 25 edges
6. `MicrophoneCapturer` - 23 edges
7. `VoipProcessor` - 21 edges
8. `RemoteSpeaker` - 18 edges
9. `InRaidVoipHUD` - 17 edges
10. `VoIPPlugin` - 16 edges

## Surprising Connections (you probably didn't know these)
- `VoIPPlugin` --references--> `HudVisibilityMode`  [EXTRACTED]
  mods/TRL-SpeakFromTarkov/modded/VOIPPlugin.cs → mods/TRL-SpeakFromTarkov/modded/UI/InRaidVoipHUD.cs
- `MicrophoneCapturer` --references--> `AudioFilter`  [EXTRACTED]
  mods/TRL-SpeakFromTarkov/modded/Audio/MicrophoneCapturer.cs → mods/TRL-SpeakFromTarkov/modded/Audio/AudioFilter.cs
- `VoipProcessor` --references--> `AudioFilter`  [EXTRACTED]
  mods/TRL-SpeakFromTarkov/modded/Audio/VoipProcessor.cs → mods/TRL-SpeakFromTarkov/modded/Audio/AudioFilter.cs
- `VoipController` --references--> `BotVoiceBridge`  [EXTRACTED]
  mods/TRL-SpeakFromTarkov/modded/Core/VoipController.cs → mods/TRL-SpeakFromTarkov/modded/Audio/BotVoiceBridge.cs
- `VoipController` --references--> `MicrophoneCapturer`  [EXTRACTED]
  mods/TRL-SpeakFromTarkov/modded/Core/VoipController.cs → mods/TRL-SpeakFromTarkov/modded/Audio/MicrophoneCapturer.cs

## Import Cycles
- None detected.

## Communities (16 total, 1 thin omitted)

### Community 0 - "VoipController"
Cohesion: 0.07
Nodes (13): AudioSource, bool, ConcurrentQueue, float, int, RemoteSpeaker, bool, ConcurrentQueue (+5 more)

### Community 1 - "SftNetwork"
Cohesion: 0.09
Nodes (17): Exception, IFikaNetworkManager, byte, NetDataReader, NetDataWriter, string, SftChannelAnnouncementPacket, byte (+9 more)

### Community 2 - "MenuVoipHUD"
Cohesion: 0.14
Nodes (11): ConcurrentDictionary, PendingActionType, bool, byte, Color, float, GUIStyle, Texture2D (+3 more)

### Community 3 - "TRL_SpeakFromTarkov.Audio"
Cohesion: 0.09
Nodes (16): TRL_SpeakFromTarkov.UI, TRL_SpeakFromTarkov.Core, TRL_SpeakFromTarkov.Network, TRL_SpeakFromTarkov.Audio, DateTime, long, int, string (+8 more)

### Community 4 - "VoiceCalibrationHUD"
Cohesion: 0.15
Nodes (10): Behaviour, CalibrationStep, List, bool, Color, float, GUIStyle, Texture2D (+2 more)

### Community 5 - "AudioFilter"
Cohesion: 0.15
Nodes (8): bool, DllImport, float, int, IntPtr, string, AudioFilter, IDisposable

### Community 6 - "VoipProcessor"
Cohesion: 0.12
Nodes (15): bool, float, Player, BotVoiceBridge, Action, byte, float, int (+7 more)

### Community 7 - "GameSessionPatcher"
Cohesion: 0.13
Nodes (12): ManualLogSource, Player, FikaVoipReceivePatch, FikaVoipSendPatch, GameSessionPatcher, GameWorldDisposePatch, PlayerInitPatch, PlayerOnDeadPatch (+4 more)

### Community 8 - "MicrophoneCapturer"
Cohesion: 0.14
Nodes (10): Action, AudioSource, bool, float, int, string, MicrophoneCapturer, AudioClip (+2 more)

### Community 9 - "VoIPPlugin"
Cohesion: 0.12
Nodes (11): BaseUnityPlugin, ConfigEntry, LoadSceneMode, Scene, Dictionary, DllImport, IntPtr, KeyboardShortcut (+3 more)

### Community 10 - "SftAudioPacketV2"
Cohesion: 0.17
Nodes (10): TRL_SpeakFromTarkov, INetSerializable, byte, float, int, NetDataReader, NetDataWriter, string (+2 more)

### Community 11 - "InRaidVoipHUD"
Cohesion: 0.20
Nodes (8): CanvasGroup, Component, bool, Color, float, Texture2D, Vector2, InRaidVoipHUD

### Community 12 - "VoipHUD"
Cohesion: 0.27
Nodes (4): Color, float, Texture2D, VoipHUD

### Community 13 - "TRL-SpeakFromTarkov.csproj"
Cohesion: 0.50
Nodes (3): netstandard2.1, Concentus (1.1.7), Microsoft.NET.Sdk

## Knowledge Gaps
- **7 isolated node(s):** `VoipMode`, `netstandard2.1`, `Concentus (1.1.7)`, `Microsoft.NET.Sdk`, `PendingActionType` (+2 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `VoipController` connect `VoipController` to `SftNetwork`, `MenuVoipHUD`, `TRL_SpeakFromTarkov.Audio`, `VoiceCalibrationHUD`, `VoipProcessor`, `MicrophoneCapturer`, `InRaidVoipHUD`, `VoipHUD`?**
  _High betweenness centrality (0.480) - this node is a cross-community bridge._
- **Why does `MenuVoipHUD` connect `MenuVoipHUD` to `VoipController`, `TRL_SpeakFromTarkov.Audio`, `VoipProcessor`?**
  _High betweenness centrality (0.232) - this node is a cross-community bridge._
- **Why does `SftNetwork` connect `SftNetwork` to `VoipController`, `MenuVoipHUD`, `TRL_SpeakFromTarkov.Audio`, `VoipProcessor`?**
  _High betweenness centrality (0.195) - this node is a cross-community bridge._
- **What connects `VoipMode`, `netstandard2.1`, `Concentus (1.1.7)` to the rest of the system?**
  _7 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `VoipController` be split into smaller, more focused modules?**
  _Cohesion score 0.07419712070874862 - nodes in this community are weakly interconnected._
- **Should `SftNetwork` be split into smaller, more focused modules?**
  _Cohesion score 0.08901515151515152 - nodes in this community are weakly interconnected._
- **Should `MenuVoipHUD` be split into smaller, more focused modules?**
  _Cohesion score 0.1350806451612903 - nodes in this community are weakly interconnected._