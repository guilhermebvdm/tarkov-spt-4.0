# Graph Report - modded  (2026-09-06)

## Corpus Check
- 61 files · ~4,696,983 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1830 nodes · 4009 edges · 111 communities (99 shown, 12 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 165 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5ddac638`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- manifest.json
- dependencies
- DecalInfo
- CamoEditor
- Plugin
- CamoEditor
- DecalSnapshotPacket
- Plugin
- com.unity.modules.jsonserialize
- MaterialSnapshotPacket
- Plugin
- ModulePatch
- List
- dependencies
- MaterialInfo
- ItemType
- packages-lock.json
- CamoEditor
- ITransformHandle
- com.unity.modules.audio
- CamoEditorItem
- WeaponCamoAndStickers/Plugin.cs
- com.unity.mathematics
- .GetDecal
- com.unity.test-framework
- com.unity.modules.imgui
- Transform
- RotationAxis
- DecalTextureData
- EquipmentStickers/CamoEditor.cs
- .LoadVideo
- SevenBoldPencil.WeaponCamoAndStickers
- HandleBase
- .Awake
- RuntimeHandle
- IScaleAxisHandle
- Decal
- DecalRenderer
- PositionAxis
- ScalePlane
- TypedFieldInfo
- SevenBoldPencil.Common
- .GetOrBuildItem
- PatchPrefix
- ScaleAxis
- MaterialEditor/Patches.cs
- WeaponPreview_Proxy
- PatchPostfix
- PositionPlane
- RotationHandle
- com.unity.modules.imageconversion
- com.unity.modules.ui
- Option
- .Prefix
- Patch_GClass926_GetItemIcon
- .GetHandleLocalRotation
- EquipmentStickers/Patches.cs
- PatchPostfix
- VectorExtensions
- Patch_WeaponPreview_Class3271_method_1
- MaskAngleHandle
- MaskOffsetHandle
- MaskTilingHandle
- TextureAngleHandle
- TextureOffsetHandle
- TextureTilingHandle
- .Prefix
- .CalculateScrollViewTotalAndVisibleHeight
- com.unity.settings-manager
- WeaponCamoAndStickers.sln
- .WriteAllTextAsync
- Dictionary
- PatchPrefix
- .ModifyMaterialOnItems
- ColorExtensions
- .Postfix
- com.unity.ai.navigation
- RawImageCameraProvider
- .Postfix
- .Postfix
- CamoEditorError
- MeshUtils
- com.unity.modules.physics2d
- com.unity.modules.terrain
- EquipmentStickers/Plugin.cs
- .DrawPresets
- ItemWithMaterials
- .GetOriginalMaterials
- WeaponCamoAndStickers/Patches.cs
- .Postfix
- .Postfix
- .TryGetCameraImage
- Patch_PlayerModelView_method_0
- SkinnedDecalsHost
- InteractionButtonsContainer_Proxy
- com.unity.assetbundlebrowser
- com.unity.collab-proxy
- com.unity.ide.vscode
- com.unity.scriptablebuildpipeline
- .Log
- .Postfix
- ItemInfoInteractionsAbstractClass_Proxy
- com.unity.modules.androidjni
- com.unity.modules.umbra
- com.unity.modules.wind
- Patch_GClass2304_smethod_0
- Result
- TransformExtensions.cs
- Patch_GClass928_GetItemHash
- CameraImage_Proxy
- IsExternalInit.cs

## God Nodes (most connected - your core abstractions)
1. `Plugin` - 199 edges
2. `Plugin` - 102 edges
3. `DecalInfo` - 91 edges
4. `CamoEditor` - 69 edges
5. `Decal` - 61 edges
6. `CamoEditor` - 59 edges
7. `CamoEditor` - 54 edges
8. `Plugin` - 27 edges
9. `MaterialInfo` - 27 edges
10. `Option` - 26 edges

## Surprising Connections (you probably didn't know these)
- `RawImageCameraProvider` --implements--> `ICameraProvider`  [EXTRACTED]
  mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/CamoEditor.cs → mods/WeaponCamoAndStickers/modded/Client/WeaponCamoAndStickers/RuntimeTransformHandle/RuntimeTransformHandle.cs
- `DecalStringCache` --references--> `StringCache`  [EXTRACTED]
  mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/CamoEditor.cs → mods/WeaponCamoAndStickers/modded/Client/WeaponCamoAndStickers/Common/StringCache.cs
- `CamoEditorItem` --references--> `StartDecalTransform`  [EXTRACTED]
  mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/CamoEditor.cs → mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/Plugin.cs
- `CamoEditorItem` --references--> `SkinnedDecalsHost`  [EXTRACTED]
  mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/CamoEditor.cs → mods/WeaponCamoAndStickers/modded/Client/WeaponCamoAndStickers/Plugin.cs
- `CamoEditor` --references--> `Plugin`  [EXTRACTED]
  mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/CamoEditor.cs → mods/WeaponCamoAndStickers/modded/Client/EquipmentStickers/Plugin.cs

## Import Cycles
- None detected.

## Communities (111 total, 12 thin omitted)

### Community 0 - "manifest.json"
Cohesion: 0.04
Nodes (46): com.unity.ai.navigation, com.unity.assetbundlebrowser, com.unity.cinemachine, com.unity.collab-proxy, com.unity.collections, com.unity.ide.visualstudio, com.unity.ide.vscode, com.unity.modules.androidjni (+38 more)

### Community 1 - "dependencies"
Cohesion: 0.04
Nodes (47): dependencies, com.unity.ai.navigation, com.unity.assetbundlebrowser, com.unity.burst, com.unity.cinemachine, com.unity.collab-proxy, com.unity.collections, com.unity.ide.visualstudio (+39 more)

### Community 3 - "CamoEditor"
Cohesion: 0.11
Nodes (20): AssetBundle, Action, AssetPoolObject, Camera, Color, Func, Rect, Shader (+12 more)

### Community 4 - "Plugin"
Cohesion: 0.08
Nodes (23): BaseUnityPlugin, CamoEditorItem, AssetPoolObject, Camera, CamoEditor, CamoEditorItem, Dictionary, DressItem (+15 more)

### Community 5 - "CamoEditor"
Cohesion: 0.10
Nodes (9): List, PlayerModelView, Rect, TextField, TexturesWindow, CamoEditor, Vector3, StartDecalTransform (+1 more)

### Community 6 - "DecalSnapshotPacket"
Cohesion: 0.10
Nodes (21): DecalInfo, DecalMirrorMode, DecalPaintMode, Dictionary, List, NetDataReader, NetDataWriter, Vector3 (+13 more)

### Community 7 - "Plugin"
Cohesion: 0.07
Nodes (7): LoddedSkin, ManualLogSource, Plugin, ConfigEntry, Random, VideoAudioOutputMode, WildSpawnType

### Community 8 - "com.unity.modules.jsonserialize"
Cohesion: 0.06
Nodes (35): dependencies, depth, source, version, dependencies, depth, source, version (+27 more)

### Community 9 - "MaterialSnapshotPacket"
Cohesion: 0.11
Nodes (17): Dictionary, MaterialInfo, MaterialsInfo, NetDataReader, NetDataWriter, Vector2, Vector3, Vector4 (+9 more)

### Community 10 - "Plugin"
Cohesion: 0.10
Nodes (5): HashSet, ManualLogSource, MaterialPreset, Plugin, Exception

### Community 11 - "ModulePatch"
Cohesion: 0.10
Nodes (16): MethodBase, Patch_PlayerBody_EquipmentSlotClass_method_4, Patch_PlayerBody_SetSkin, Patch_PlayerBody_SetSkin_CreateItem, Patch_PlayerModelView_method_1, Patch_PoolManagerClass_CreateItemAsync, Patch_PoolManagerClass_method_2, Patch_ScrollTrigger_OnScroll (+8 more)

### Community 12 - "List"
Cohesion: 0.15
Nodes (7): Camera, Dictionary, List, Profile, IDecalsHost, ItemsWithDecals, ItemWithDecals

### Community 13 - "dependencies"
Cohesion: 0.08
Nodes (31): dependencies, depth, source, version, dependencies, depth, source, version (+23 more)

### Community 14 - "MaterialInfo"
Cohesion: 0.16
Nodes (10): Action, Color, SystemObject, Texture, Vector4, CustomTexture, MaterialInfo, TargetMaterial (+2 more)

### Community 15 - "ItemType"
Cohesion: 0.14
Nodes (15): AssetPoolObject, CamoEditor, DressItem, Item, PreviewPivot, ResourceKey, ItemType, Armor (+7 more)

### Community 16 - "packages-lock.json"
Cohesion: 0.07
Nodes (26): com.unity.ext.nunit, com.unity.modules.subsystems, com.unity.nuget.mono-cecil, com.unity.settings-manager, com.unity.test-framework.performance, com.unity.burst, com.unity.mathematics, com.unity.modules.ai (+18 more)

### Community 17 - "CamoEditor"
Cohesion: 0.14
Nodes (5): HashSet, List, Rect, TexturesWindow, CamoEditor

### Community 18 - "ITransformHandle"
Cohesion: 0.15
Nodes (11): Camera, Ray, Transform, Vector3, DefaultCameraProvider, ICameraProvider, ITransformHandle, RuntimeTransformHandle (+3 more)

### Community 19 - "com.unity.modules.audio"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, version (+18 more)

### Community 20 - "CamoEditorItem"
Cohesion: 0.14
Nodes (9): Action, Dictionary, TextField, Vector2, Vector3, CamoEditorItem, EditedOverride, left (+1 more)

### Community 21 - "WeaponCamoAndStickers/Plugin.cs"
Cohesion: 0.13
Nodes (14): HashSet, AddTexturePararms, ClosedTexturesDirectories, DecalTextureFormat, PNG, Unknown, Video, DecalTextureType (+6 more)

### Community 22 - "com.unity.mathematics"
Cohesion: 0.09
Nodes (24): dependencies, depth, source, url, version, dependencies, depth, source (+16 more)

### Community 24 - "com.unity.test-framework"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, url, version, dependencies, depth, source (+15 more)

### Community 25 - "com.unity.modules.imgui"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, url, version, dependencies, depth, source (+15 more)

### Community 26 - "Transform"
Cohesion: 0.19
Nodes (9): center, Transform, Vector3, Vector4, WeaponPrefab, StartDecalTransform, Vector2, scale (+1 more)

### Community 27 - "RotationAxis"
Cohesion: 0.14
Nodes (12): Camera, Color, MeshCollider, MeshFilter, MeshRenderer, Quaternion, Ray, Shader (+4 more)

### Community 28 - "DecalTextureData"
Cohesion: 0.21
Nodes (7): AddTexturePararms, Texture2D, Vector2Int, DecalTextureData, extension, IEnumerator, name

### Community 29 - "EquipmentStickers/CamoEditor.cs"
Cohesion: 0.11
Nodes (16): Dictionary, DecalStringCache, DecalSettingType, Mask, HandleType, MaskAngle, MaskOffset, MaskTiling (+8 more)

### Community 30 - ".LoadVideo"
Cohesion: 0.24
Nodes (7): Texture, Action, SystemObject, DecalTextureAsset, DecalTexturePNG, DecalTextureVideo, VideoPlayer

### Community 31 - "SevenBoldPencil.WeaponCamoAndStickers"
Cohesion: 0.12
Nodes (8): Vector3, Camera, Shader, Transform, Vector3, ScaleAxisHandle_Transform, ScaleHandle, SevenBoldPencil.WeaponCamoAndStickers

### Community 32 - "HandleBase"
Cohesion: 0.17
Nodes (10): Color, Material, Ray, Shader, Transform, Vector3, HandleBase, hitPoint (+2 more)

### Community 33 - ".Awake"
Cohesion: 0.13
Nodes (8): LoddedSkin, MethodBase, Patch_AssetPoolObject_ReturnToPool, Patch_GClass3380_smethod_2, Patch_GClass928_GetItemHash, Patch_LoddedSkin_Unskin, Patch_ScrollTrigger_OnScroll, Patch_WeaponModdingScreen_Close

### Community 34 - "RuntimeHandle"
Cohesion: 0.11
Nodes (7): Ray, HandleMathUtils, Camera, Transform, Vector3, PositionAxisHandle_Tranform, RuntimeHandle

### Community 35 - "IScaleAxisHandle"
Cohesion: 0.13
Nodes (11): IScaleAxisHandle, Camera, Color, MeshCollider, MeshFilter, MeshRenderer, Ray, Shader (+3 more)

### Community 36 - "Decal"
Cohesion: 0.21
Nodes (6): Material, Shader, Transform, Vector4, Decal, LocalKeyword

### Community 37 - "DecalRenderer"
Cohesion: 0.27
Nodes (9): Camera, Dictionary, HashSet, List, Material, Matrix4x4, Mesh, DecalRenderer (+1 more)

### Community 38 - "PositionAxis"
Cohesion: 0.17
Nodes (10): Color, MeshCollider, MeshFilter, MeshRenderer, Ray, Shader, Transform, Vector3 (+2 more)

### Community 39 - "ScalePlane"
Cohesion: 0.18
Nodes (9): Color, MeshCollider, MeshFilter, MeshRenderer, Ray, Shader, Transform, Vector3 (+1 more)

### Community 40 - "TypedFieldInfo"
Cohesion: 0.14
Nodes (12): AbstractSkin, TypedFieldInfo, Weapon, LoddedSkin, WeaponPrefab, Dress_Proxy, LoddedSkin_Proxy, _lods (+4 more)

### Community 41 - "SevenBoldPencil.Common"
Cohesion: 0.17
Nodes (8): ColorStringCache, MaterialStringCache, Vector2StringCache, HashSet, CollectionsExtensions, Func, StringCache, SevenBoldPencil.Common

### Community 42 - ".GetOrBuildItem"
Cohesion: 0.25
Nodes (6): AssetPoolObject, CamoEditor, CamoEditorItem, Item, List, PlayerModelView

### Community 43 - "PatchPrefix"
Cohesion: 0.14
Nodes (8): CompoundItem, GClass2304, LoddedSkin, PatchPrefix, WeaponModdingScreen, WeaponPreview, Patch_GClass2304_smethod_0, Patch_LoddedSkin_Unskin

### Community 44 - "ScaleAxis"
Cohesion: 0.19
Nodes (10): Color, MeshCollider, MeshFilter, MeshRenderer, Ray, Shader, Transform, Vector3 (+2 more)

### Community 45 - "MaterialEditor/Patches.cs"
Cohesion: 0.16
Nodes (6): Renderer, Patch_HotObject_SetTemperatureToRenderer, Patch_RainCondensator_OnDisable, Patch_RainCondensator_OnEnable, Patch_RainCondensator_UpdateValues, SevenBoldPencil.MaterialEditor

### Community 46 - "WeaponPreview_Proxy"
Cohesion: 0.15
Nodes (10): Class3271, WeaponPreview, Patch_WeaponPreview_Class3271_method_1, Patch_WeaponPreview_Rotate, GameObject, Item, WeaponPreview, WeaponPreview_Proxy (+2 more)

### Community 47 - "PatchPostfix"
Cohesion: 0.20
Nodes (10): EBodyModelPart, KeyValuePair, PatchPostfix, Player, PoolManagerClass, PoolsCategory, ResourceKey, Skeleton (+2 more)

### Community 48 - "PositionPlane"
Cohesion: 0.17
Nodes (9): Color, MeshCollider, MeshFilter, MeshRenderer, Ray, Shader, Transform, Vector3 (+1 more)

### Community 49 - "RotationHandle"
Cohesion: 0.18
Nodes (7): Camera, Quaternion, Shader, Transform, Vector3, RotationAxisHandle_Transform, RotationHandle

### Community 50 - "com.unity.modules.imageconversion"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 51 - "com.unity.modules.ui"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, version, dependencies, depth, source, version (+7 more)

### Community 53 - ".Prefix"
Cohesion: 0.14
Nodes (10): CancellationToken, ECameraType, GameObject, GDelegate62, IPlayer, PoolManagerClass, PoolsCategory, ResourceKey (+2 more)

### Community 54 - "Patch_GClass926_GetItemIcon"
Cohesion: 0.33
Nodes (9): GameObject, Item, Texture2D, Patch_GClass926_GetItemIcon, GClass926, GClass929, Sprite, Task (+1 more)

### Community 55 - ".GetHandleLocalRotation"
Cohesion: 0.29
Nodes (6): Quaternion, Vector3, Vector4, UVTools, signX, signY

### Community 56 - "EquipmentStickers/Patches.cs"
Cohesion: 0.23
Nodes (6): MethodBase, Patch_InventoryPlayerModelWithStatsWindow_method_4, Patch_OverallScreen_Close, Patch_OverallScreen_Show, Patch_ScrollTrigger_OnScroll, SevenBoldPencil.EquipmentStickers

### Community 57 - "PatchPostfix"
Cohesion: 0.18
Nodes (7): GClass3380, IIdGenerator, Item, PatchPostfix, PlayerModelView, Patch_GClass928_smethod_1, Patch_PlayerModelView_method_0

### Community 58 - "VectorExtensions"
Cohesion: 0.23
Nodes (5): Quaternion, Vector2Int, Vector3, Vector4, VectorExtensions

### Community 59 - "Patch_WeaponPreview_Class3271_method_1"
Cohesion: 0.17
Nodes (6): AssetPoolObject, Class3271, PreviewPivot, Patch_AssetPoolObject_OnDestroy, Patch_AssetPoolObject_ReturnToPool, Patch_WeaponPreview_Class3271_method_1

### Community 60 - "MaskAngleHandle"
Cohesion: 0.18
Nodes (7): Camera, Quaternion, Shader, Transform, Vector3, MaskAngleHandle, RotationAxisHandle_MaskAngle

### Community 61 - "MaskOffsetHandle"
Cohesion: 0.21
Nodes (7): Camera, Shader, Transform, Vector3, Vector4, MaskOffsetHandle, PositionAxisHandle_MaskOffset

### Community 62 - "MaskTilingHandle"
Cohesion: 0.18
Nodes (7): Camera, Shader, Transform, Vector2, Vector4, MaskTilingHandle, ScaleAxisHandle_MaskTiling

### Community 63 - "TextureAngleHandle"
Cohesion: 0.18
Nodes (7): Camera, Quaternion, Shader, Transform, Vector3, RotationAxisHandle_TextureAngle, TextureAngleHandle

### Community 64 - "TextureOffsetHandle"
Cohesion: 0.21
Nodes (7): Camera, Shader, Transform, Vector3, Vector4, PositionAxisHandle_TextureOffset, TextureOffsetHandle

### Community 65 - "TextureTilingHandle"
Cohesion: 0.18
Nodes (7): Camera, Shader, Transform, Vector2, Vector4, ScaleAxisHandle_TextureTiling, TextureTilingHandle

### Community 66 - ".Prefix"
Cohesion: 0.17
Nodes (9): BotOwner, Action, CancellationToken, ECameraType, GDelegate62, IPlayer, Profile, Patch_BotCreatorClass_method_2 (+1 more)

### Community 68 - "com.unity.settings-manager"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, url, version, dependencies, depth, source (+4 more)

### Community 69 - "WeaponCamoAndStickers.sln"
Cohesion: 0.25
Nodes (9): EquipmentStickers, Microsoft.NET.Sdk, MaterialEditor.Fika, Microsoft.NET.Sdk, MaterialEditor, Microsoft.NET.Sdk, WeaponCamoAndStickers.Fika, Microsoft.NET.Sdk (+1 more)

### Community 71 - "Dictionary"
Cohesion: 0.42
Nodes (3): Dictionary, ItemsWithMaterials, MaterialsInfo

### Community 72 - "PatchPrefix"
Cohesion: 0.22
Nodes (6): AssetPoolObject, CompoundItem, PatchPrefix, WeaponModdingScreen, Patch_AssetPoolObject_OnDestroy, Patch_WeaponModdingScreen_method_6

### Community 74 - "ColorExtensions"
Cohesion: 0.35
Nodes (4): Color, Vector3, Vector4, ColorExtensions

### Community 75 - ".Postfix"
Cohesion: 0.29
Nodes (6): EItemInfoButton, ItemContextClass, ItemInfoInteractionsAbstractClass, ItemUiContext, Patch_ItemUiContext_GetItemContextInteractions, FailedResult

### Community 76 - "com.unity.ai.navigation"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 77 - "RawImageCameraProvider"
Cohesion: 0.24
Nodes (7): Camera, RawImage, Ray, RectTransform, Transform, Vector2, RawImageCameraProvider

### Community 78 - ".Postfix"
Cohesion: 0.20
Nodes (6): InventoryController, OverallScreen, PatchPostfix, PatchPrefix, Profile, SessionCountersClass

### Community 79 - ".Postfix"
Cohesion: 0.20
Nodes (6): InventoryController, OverallScreen, Profile, SessionCountersClass, Patch_OverallScreen_Close, Patch_OverallScreen_Show

### Community 80 - "CamoEditorError"
Cohesion: 0.33
Nodes (3): Matrix4x4, Rect, CamoEditorError

### Community 81 - "MeshUtils"
Cohesion: 0.47
Nodes (3): Mesh, Vector3, MeshUtils

### Community 82 - "com.unity.modules.physics2d"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 83 - "com.unity.modules.terrain"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 84 - "EquipmentStickers/Plugin.cs"
Cohesion: 0.22
Nodes (8): DecalMirrorMode, Disabled, Enabled, EnabledNoFlip, MODES_COUNT, DecalPaintMode, Erase, Paint

### Community 86 - "ItemWithMaterials"
Cohesion: 0.36
Nodes (4): GameObject, LoddedSkin, ResourceKey, ItemWithMaterials

### Community 87 - ".GetOriginalMaterials"
Cohesion: 0.33
Nodes (3): Material, Renderer, MaterialPropertyBlock

### Community 88 - "WeaponCamoAndStickers/Patches.cs"
Cohesion: 0.33
Nodes (6): InteractionButtonsContainer, RectTransform, SimpleContextMenuButton, Custom_DynamicInteractionClass, Patch_InteractionButtonsContainer_method_3, DynamicInteractionClass

### Community 89 - ".Postfix"
Cohesion: 0.25
Nodes (6): EBodyModelPart, KeyValuePair, Player, PlayerBody, Skeleton, Patch_PlayerBody_SetSkin

### Community 90 - ".Postfix"
Cohesion: 0.36
Nodes (5): EItemInfoButton, ItemContextClass, ItemInfoInteractionsAbstractClass, ItemUiContext, Patch_ItemUiContext_GetItemContextInteractions

### Community 91 - ".TryGetCameraImage"
Cohesion: 0.38
Nodes (5): Camera, CameraImage, PlayerModelView, RawImage, Patch_PlayerModelView_method_0

### Community 92 - "Patch_PlayerModelView_method_0"
Cohesion: 0.43
Nodes (3): Camera, PlayerModelView, Patch_PlayerModelView_method_0

### Community 93 - "SkinnedDecalsHost"
Cohesion: 0.40
Nodes (3): GameObject, Skeleton, SkinnedDecalsHost

### Community 94 - "InteractionButtonsContainer_Proxy"
Cohesion: 0.33
Nodes (6): InteractionButtonsContainer, RectTransform, SimpleContextMenuButton, InteractionButtonsContainer_Proxy, _buttonsContainer, _buttonTemplate

### Community 95 - "com.unity.assetbundlebrowser"
Cohesion: 0.33
Nodes (6): dependencies, depth, hash, source, version, com.unity.assetbundlebrowser

### Community 96 - "com.unity.collab-proxy"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 97 - "com.unity.ide.vscode"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.vscode

### Community 98 - "com.unity.scriptablebuildpipeline"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.scriptablebuildpipeline

### Community 99 - ".Log"
Cohesion: 0.40
Nodes (3): ManualLogSource, LoggerExtensions, LogLevel

### Community 100 - ".Postfix"
Cohesion: 0.40
Nodes (3): GClass3380, IIdGenerator, Patch_GClass3380_smethod_2

### Community 101 - "ItemInfoInteractionsAbstractClass_Proxy"
Cohesion: 0.40
Nodes (5): Dictionary, DynamicInteractionClass, ItemInfoInteractionsAbstractClass, ItemInfoInteractionsAbstractClass_Proxy, Dictionary_0

### Community 102 - "com.unity.modules.androidjni"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 103 - "com.unity.modules.umbra"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.umbra

### Community 104 - "com.unity.modules.wind"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

### Community 109 - "CameraImage_Proxy"
Cohesion: 0.50
Nodes (4): CameraImage, RawImage, CameraImage_Proxy, rawImage_0

## Knowledge Gaps
- **360 isolated node(s):** `Microsoft.NET.Sdk`, `Microsoft.NET.Sdk`, `Microsoft.NET.Sdk`, `Microsoft.NET.Sdk`, `Mask` (+355 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 604 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **12 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Plugin` connect `Plugin` to `DecalInfo`, `CamoEditor`, `Plugin`, `CamoEditor`, `DecalSnapshotPacket`, `ModulePatch`, `List`, `MaterialInfo`, `ItemType`, `CamoEditor`, `WeaponCamoAndStickers/Plugin.cs`, `.GetDecal`, `Transform`, `DecalTextureData`, `EquipmentStickers/CamoEditor.cs`, `.LoadVideo`, `SevenBoldPencil.WeaponCamoAndStickers`, `DecalRenderer`, `SevenBoldPencil.Common`, `RotationHandle`, `Option`, `MaskAngleHandle`, `MaskOffsetHandle`, `MaskTilingHandle`, `TextureAngleHandle`, `TextureOffsetHandle`, `TextureTilingHandle`, `.CalculateScrollViewTotalAndVisibleHeight`, `.WriteAllTextAsync`, `CamoEditorError`, `EquipmentStickers/Plugin.cs`, `SkinnedDecalsHost`?**
  _High betweenness centrality (0.217) - this node is a cross-community bridge._
- **Why does `Plugin` connect `Plugin` to `.Awake`, `CamoEditor`, `Plugin`, `Dictionary`, `MaterialSnapshotPacket`, `.GetOrBuildItem`, `.ModifyMaterialOnItems`, `MaterialInfo`, `CamoEditor`, `Option`, `.DrawPresets`, `ItemWithMaterials`, `.GetOriginalMaterials`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **Why does `DecalInfo` connect `DecalInfo` to `CamoEditor`, `CamoEditor`, `DecalSnapshotPacket`, `Plugin`, `List`, `WeaponCamoAndStickers/Plugin.cs`, `.GetDecal`, `Transform`, `EquipmentStickers/CamoEditor.cs`, `SevenBoldPencil.WeaponCamoAndStickers`, `Decal`, `DecalRenderer`, `RotationHandle`, `MaskAngleHandle`, `MaskOffsetHandle`, `MaskTilingHandle`, `TextureAngleHandle`, `TextureOffsetHandle`, `TextureTilingHandle`, `.CalculateScrollViewTotalAndVisibleHeight`, `EquipmentStickers/Plugin.cs`?**
  _High betweenness centrality (0.085) - this node is a cross-community bridge._
- **What connects `Microsoft.NET.Sdk`, `Microsoft.NET.Sdk`, `Microsoft.NET.Sdk` to the rest of the system?**
  _360 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.0425531914893617 - nodes in this community are weakly interconnected._
- **Should `dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.0425531914893617 - nodes in this community are weakly interconnected._
- **Should `DecalInfo` be split into smaller, more focused modules?**
  _Cohesion score 0.12367864693446089 - nodes in this community are weakly interconnected._