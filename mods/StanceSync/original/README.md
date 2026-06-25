# StanceSync

> A simple mod to sync leaning and shoulder swapping.

**Author:** hazelify  
**Version:** 1.0.0  
**SPT Version:** 4.0.13  
**License:** MIT

---

## What This Mod Does

Inspired by [HollywoodCam](https://forge.sp-tarkov.com/mod/2201/hollywoodcam) which is created by the almighty [Janky](https://forge.sp-tarkov.com/user/72916/jankytheclown), I created this little offshoot mod as a standalone for those who, like me, only used HollywoodCam for the leaning. The configuration is more or less the same, with some variation; lean-shoulder-swapping works while you're aiming with this mod. I have not noticed any problems with leaning during ADS, so I left it in.

This template includes BepInEx Configuration examples. All settings are editable in-game via the **F12** config manager or by editing the config file at `BepInEx/config/hazelify.StanceSync.cfg`.

### Included Config Entries

| Setting | Type | Default | Description |
|---|---|---|---|
| Enable syncing | `bool` | `true` | Enable lean and shoulder swap sync |
| Enable reset sync  | `bool` | `true` | Enable reset sync for leaning |

---

## Requirements

- [SPT](https://www.sp-tarkov.com/) installed (BepInEx is included)
- .NET SDK (`netstandard2.1`-compatible) for building from source

---

## Building

```sh
cd StanceSync
dotnet build -c Release
```

The PostBuild target automatically copies the compiled DLL to your SPT installation's `BepInEx\plugins\` folder.
The project also packages the mod into a distributable `StanceSync.zip` ready to be uploaded.

---

## Installation

1. Build the project (see above) **or** download the latest release DLL.
2. Copy `StanceSync.dll` to `<SPT install>\BepInEx\plugins\` **or** Drag and drop the `BepInEx` folder to your source / root SPT install, where `SPT.Server` is.
3. Launch SPT as usual.
4. Press **F12** in-game to open the config manager and adjust settings.

---

## Project Structure

```
StanceSync/
├── Patches/
    ├── OnLeanPatchPostfix.cs                  ← PatchPostfix for leaning
    └── PlayerCameraControllerPatchPrefix.cs   ← PatchPrefix for camera controller
├── Camera.cs           ← Unused file, too lazy to delete currently
├── StanceSync.csproj   ← project file with DLL references
├── Plugin.cs           ← BepInEx plugin with config entries
├── README.md           ← README file for reading
└── .gitignore          ← Wwhich files Git should ignore
```

---

## Learning Resources

| Resource | URL |
|---|---|
| BepInEx Configuration Docs | https://github.com/BepInEx/BepInEx.ConfigurationManager/blob/master/README.md|
| SPT Client Mod Examples | https://github.com/Jehree/SPTClientModExamples |
| SPT Wiki Modding Resources | https://wiki.sp-tarkov.com/modding/Modding_Resources |

---
