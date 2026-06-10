# AutoGym

AutoGym is a BepInEx plugin for SPT 4.0+ that automatically completes the hideout gym shrinking-circle QTE after a workout starts.

It is Fika-safe and also has an optional visual cleanup setting that temporarily hides bulky gear during the workout animation so backpacks, rigs, armor, helmets, headsets, face covers, and eyewear do not clip through the bench.

## Features

- Automatically completes the hideout gym QTE.
- Configurable success-window timing bias.
- Optional extra completion delay.
- Optional workout gear hiding, restored when the workout stops.

## Install

Copy `AutoGym.dll` to:

```text
BepInEx/plugins/AutoGym.dll
```

## Build

The project expects an SPT install at `C:\SPT` by default.

```powershell
dotnet build .\AutoGym.csproj -c Release
```

To build against a different SPT path:

```powershell
dotnet build .\AutoGym.csproj -c Release /p:SptPath="D:\Games\SPT"
```

## License

MIT
