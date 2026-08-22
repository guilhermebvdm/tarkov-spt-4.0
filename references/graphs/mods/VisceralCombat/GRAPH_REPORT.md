# Graph Report - mods\VisceralCombat\modded  (2026-08-12)

## Corpus Check
- 130 files · ~103,805 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1798 nodes · 3154 edges · 121 communities (108 shown, 13 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 99 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `336d07fb`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- BipedNaming
- PhysXTools
- Baker
- MuscleLite
- BFX_DecalSettings
- PuppetMaster
- Prop
- PuppetMasterLite
- Interp
- Muscle
- BehaviourBase
- VisceralEntry
- BehaviourFall
- BakerTransform
- .AddMuscle
- BipedRagdollCreator
- BehaviourPuppet
- RagdollCreator
- PuppetMasterProp
- JointConverter
- BleedPatch
- PropMuscle
- VisceralCombat.Ragdolls.Classes.RootMotion
- _003CRagdollSleepHandler_003Ed__2
- _003CAliveToDead_003Ed__226
- DismembermentPacket
- RagdollHelperClass
- MuscleCollision
- QuaTools
- KillPatch
- VisceralCombat.Ragdolls.Patches
- PuppetMasterSettings
- BakerHumanoidQT
- SubBehaviourCOM
- SolverManager
- .Postfix
- ModulePatch
- Utils
- PuppetControllerLite
- TQ
- Transform
- .SetState
- LivingDismembermentController
- PuppetMasterHumanoidConfig
- HumanoidBaker
- GoreObjectPool
- Vector3
- AnimatorEvent
- BFX_BloodDecalLayers
- VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics
- MonoBehaviour
- .SetIKKeyframes
- LayerMaskExtensions
- .GetProps
- .GetAxisVectorToDirection
- BakerMuscle
- Actuator
- .GetFalloff
- BehaviourTemplate
- ParticleCollisionHandler
- BodiesImpulsePatch
- ShellCasingPatch
- VisceralCombat.Dismemberment.Classes
- .CreateLimbJoints
- BakerUtilities
- .CreateFootCollider
- CollisionEventBroadcaster
- MuscleCollisionBroadcaster
- RagdollEditor
- RigidbodyController
- ConfigurationManagerAttributes
- BundleLoaderPlugin
- .HandleDeathAudio
- IEnumerator
- .Postfix
- ParticleFloorPainter
- .Postfix
- .Initiate
- V2Tools
- GameStartedPatch
- LimbKillPatch
- CreateBSGRagdollPatch
- MovementContextPatch
- Props
- PhysicalItemsPatch
- Weight
- float
- .CopyCollider
- .Postfix
- MuscleHit
- SubBehaviourBase
- AttachWeaponPatch
- GrenadeDeadBodiesPatch
- PropertyAttribute
- DefaultPlayPatch
- PlaySoundBankPatch
- PlayStepSoundPatch
- VisceralHandshakePacket
- PuppetMasterTools
- TriggerEventBroadcaster
- .Log
- GameStartedPatch
- BipedLimbOrientations
- FixFootColliders
- BFX_RenderDepth
- BaseUnityPlugin
- LayerCollisionData
- EffectContainer
- Singleton
- bundleloader.csproj
- VisceralCombat.csproj
- .OnTeleport
- LazySingleton
- VolumetricBloodFX.csproj
- .FromBipedReferences

## God Nodes (most connected - your core abstractions)
1. `PuppetMaster` - 173 edges
2. `Muscle` - 83 edges
3. `BehaviourPuppet` - 67 edges
4. `BehaviourBase` - 62 edges
5. `VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics` - 46 edges
6. `VisceralCombat.Ragdolls.Classes.RootMotion` - 35 edges
7. `BipedRagdollCreator` - 31 edges
8. `Baker` - 31 edges
9. `Interp` - 31 edges
10. `VisceralEntry` - 30 edges

## Surprising Connections (you probably didn't know these)
- `VisceralEntry` --references--> `EffectContainer`  [EXTRACTED]
  mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs → mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Classes/EffectContainer.cs
- `BehaviourBase` --references--> `PuppetMaster`  [EXTRACTED]
  mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourBase.cs → mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/PuppetMaster.cs
- `BehaviourFall` --inherits--> `BehaviourBase`  [EXTRACTED]
  mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourFall.cs → mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourBase.cs
- `BehaviourPuppet` --inherits--> `BehaviourBase`  [EXTRACTED]
  mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourPuppet.cs → mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourBase.cs
- `BehaviourTemplate` --inherits--> `BehaviourBase`  [EXTRACTED]
  mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourTemplate.cs → mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics/BehaviourBase.cs

## Import Cycles
- None detected.

## Communities (121 total, 13 thin omitted)

### Community 0 - "BipedNaming"
Cohesion: 0.07
Nodes (18): AutoDetectParams, BoneSide, BoneType, string, Transform, BipedNaming, BoneSide, BoneType (+10 more)

### Community 1 - "PhysXTools"
Cohesion: 0.08
Nodes (23): ForceMode, Settings, CapsuleCollider, ConfigurableJoint, Quaternion, Rigidbody, Vector3, PhysXTools (+15 more)

### Community 2 - "Baker"
Cohesion: 0.05
Nodes (26): BakerDelegate, BasedUponRotation, BasedUponXZ, BasedUponY, ClipSettings, PlayableDirector, AnimationClip, Animator (+18 more)

### Community 3 - "MuscleLite"
Cohesion: 0.07
Nodes (14): bool, ConfigurableJoint, float, JointDrive, Quaternion, Rigidbody, string, Transform (+6 more)

### Community 4 - "BFX_DecalSettings"
Cohesion: 0.05
Nodes (23): _DecalRenderinMode, BFX_BloodSettings, bool, float, _DecalRenderinMode, BFX_DecalSettings, AnimationCurve, bool (+15 more)

### Community 5 - "PuppetMaster"
Cohesion: 0.07
Nodes (14): AnimatorUpdateMode, MuscleDelegate, StateSettings, UpdateDelegate, Animation, bool, ContextMenu, float (+6 more)

### Community 6 - "Prop"
Cohesion: 0.08
Nodes (15): bool, Collider, ConfigurableJoint, ConfigurableJointMotion, float, int, PhysicMaterial, Props (+7 more)

### Community 7 - "PuppetMasterLite"
Cohesion: 0.09
Nodes (14): PuppetMasterLiteDelegate, UpdateMode, Animator, bool, DebuggerHidden, float, int, IteratorStateMachine (+6 more)

### Community 8 - "Interp"
Cohesion: 0.11
Nodes (3): Vector3, Interp, InterpolationMode

### Community 9 - "Muscle"
Cohesion: 0.09
Nodes (8): TargetChild, ConfigurableJointMotion, int, JointDrive, string, Group, Muscle, VisceralEntry

### Community 10 - "BehaviourBase"
Cohesion: 0.08
Nodes (5): BehaviourDelegate, BehaviourUpdateDelegate, HitDelegate, CollisionDelegate, BehaviourBase

### Community 11 - "VisceralEntry"
Cohesion: 0.08
Nodes (20): ConfigEntry, FikaNetworkManagerCreatedEvent, ManualLogSource, ELogType, AssetBundle, BoxCollider, CollisionDetectionMode, Dictionary (+12 more)

### Community 12 - "BehaviourFall"
Cohesion: 0.08
Nodes (12): bool, ContextMenu, DebuggerHidden, float, int, IteratorStateMachine, LayerMask, object (+4 more)

### Community 13 - "BakerTransform"
Cohesion: 0.09
Nodes (13): AnimationClip, AnimationCurve, bool, Quaternion, string, Transform, Vector3, BakerTransform (+5 more)

### Community 14 - ".AddMuscle"
Cohesion: 0.14
Nodes (4): ConfigurableJoint, HumanBodyBones, Props, Rigidbody

### Community 15 - "BipedRagdollCreator"
Cohesion: 0.16
Nodes (7): Options, PlayerBones, ContextMenu, BipedRagdollCreator, Animator, Transform, BipedRagdollReferences

### Community 16 - "BehaviourPuppet"
Cohesion: 0.10
Nodes (11): CollisionImpulseDelegate, CollisionResistanceMultiplier, MasterProps, MusclePropsGroup, CollisionDelegate, ContextMenu, int, PuppetEvent (+3 more)

### Community 17 - "RagdollCreator"
Cohesion: 0.14
Nodes (11): CreateJointParams, Direction, Collider, Rigidbody, SoftJointLimit, Transform, Vector3, ColliderType (+3 more)

### Community 18 - "PuppetMasterProp"
Cohesion: 0.10
Nodes (13): RigidbodyConstraints, RigidbodyInterpolation, bool, Collider, CollisionDetectionMode, float, int, PhysicMaterial (+5 more)

### Community 19 - "JointConverter"
Cohesion: 0.17
Nodes (12): CharacterJoint, FixedJoint, HingeJoint, JointLimits, JointSpring, SoftJointLimitSpring, SpringJoint, ConfigurableJoint (+4 more)

### Community 20 - "BleedPatch"
Cohesion: 0.14
Nodes (14): AmmoItemClass, BallisticsCalculator, Item, Collider, Dictionary, EftBulletClass, IEnumerator, List (+6 more)

### Community 21 - "PropMuscle"
Cohesion: 0.12
Nodes (6): PropDelegate, MuscleDisconnectMode, Vector3, PropMuscle, Quaternion, Vector3

### Community 22 - "VisceralCombat.Ragdolls.Classes.RootMotion"
Cohesion: 0.12
Nodes (8): VisceralCombat.Ragdolls.Classes.RootMotion, string, Comments, ShowIfAttribute, ShowIfMode, string, ShowLargeHeaderIf, ShowRangeIfAttribute

### Community 23 - "_003CRagdollSleepHandler_003Ed__2"
Cohesion: 0.13
Nodes (13): Enumerator, MethodInfo, bool, DebuggerHidden, float, int, IteratorStateMachine, MethodBase (+5 more)

### Community 24 - "_003CAliveToDead_003Ed__226"
Cohesion: 0.19
Nodes (9): IDisposable, DebuggerHidden, int, object, _003CActiveToDisabled_003Ed__174, _003CActiveToKinematic_003Ed__175, _003CAliveToDead_003Ed__226, _003CDisabledToActive_003Ed__171 (+1 more)

### Community 25 - "DismembermentPacket"
Cohesion: 0.10
Nodes (14): INetSerializable, EBodyPart, NetDataReader, NetDataWriter, Vector3, DismembermentPacket, NetDataReader, NetDataWriter (+6 more)

### Community 26 - "RagdollHelperClass"
Cohesion: 0.16
Nodes (10): Dictionary, EBodyPart, float, IEnumerator, List, Player, Transform, Vector3 (+2 more)

### Community 27 - "MuscleCollision"
Cohesion: 0.15
Nodes (6): Collision, bool, Collision, int, MuscleCollision, Collision

### Community 29 - "QuaTools"
Cohesion: 0.21
Nodes (3): Quaternion, Vector3, QuaTools

### Community 30 - "KillPatch"
Cohesion: 0.20
Nodes (11): AmmoTemplate, DamageInfoStruct, Dictionary, EBodyPart, Func, MethodBase, PatchPostfix, Player (+3 more)

### Community 31 - "VisceralCombat.Ragdolls.Patches"
Cohesion: 0.15
Nodes (7): VisceralCombat, VisceralCombat.Dismemberment.Classes.Packets, VisceralCombat.Ragdolls.Classes.Packets, VisceralCombat.Ragdolls.Classes, VisceralCombat.Combined.Patches, VisceralCombat.Ragdolls.Patches, QuickLogger

### Community 32 - "PuppetMasterSettings"
Cohesion: 0.16
Nodes (8): PuppetUpdateLimit, Singleton, bool, float, int, List, PuppetMasterSettings, PuppetUpdateLimit

### Community 33 - "BakerHumanoidQT"
Cohesion: 0.15
Nodes (8): AnimationClip, AnimationCurve, Animator, AvatarIKGoal, bool, string, BakerHumanoidQT, AnimationClip

### Community 34 - "SubBehaviourCOM"
Cohesion: 0.20
Nodes (7): bool, float, LayerMask, Quaternion, Vector3, Mode, SubBehaviourCOM

### Community 35 - "SolverManager"
Cohesion: 0.16
Nodes (5): Animation, Animator, bool, Transform, SolverManager

### Community 36 - ".Postfix"
Cohesion: 0.12
Nodes (11): AnimatorOverrideController, AnimationClip, IEnumerable, Player, Transform, Utils, PatchPostfix, Player (+3 more)

### Community 37 - "ModulePatch"
Cohesion: 0.12
Nodes (12): ModulePatch, HashSet, MethodBase, string, CreateCorpsePatch, IExplosiveItem, MethodBase, PatchPostfix (+4 more)

### Community 38 - "Utils"
Cohesion: 0.20
Nodes (7): Collider, GameObject, IEnumerable, List, Player, Transform, Utils

### Community 39 - "PuppetControllerLite"
Cohesion: 0.17
Nodes (7): Collision, float, int, LayerMask, string, Group, PuppetControllerLite

### Community 41 - "TQ"
Cohesion: 0.22
Nodes (8): Avatar, AvatarIKGoal, HumanBodyBones, Quaternion, AvatarUtility, Quaternion, Vector3, TQ

### Community 42 - "Transform"
Cohesion: 0.18
Nodes (6): Cloth, Color, Animator, Collider, Group, Transform

### Community 43 - ".SetState"
Cohesion: 0.16
Nodes (4): State, LayerMask, Quaternion, Vector3

### Community 44 - "LivingDismembermentController"
Cohesion: 0.26
Nodes (6): BotOwner, bool, EBodyPart, float, Player, LivingDismembermentController

### Community 45 - "PuppetMasterHumanoidConfig"
Cohesion: 0.16
Nodes (11): HumanoidMuscle, ScriptableObject, Animator, bool, float, HumanBodyBones, int, Props (+3 more)

### Community 46 - "HumanoidBaker"
Cohesion: 0.14
Nodes (9): HumanPose, HumanPoseHandler, bool, float, int, Quaternion, Transform, Vector3 (+1 more)

### Community 47 - "GoreObjectPool"
Cohesion: 0.21
Nodes (7): Dictionary, GameObject, IEnumerator, Quaternion, Transform, Vector3, GoreObjectPool

### Community 48 - "Vector3"
Cohesion: 0.16
Nodes (4): Quaternion, Rigidbody, Vector3, TargetChild

### Community 49 - "AnimatorEvent"
Cohesion: 0.15
Nodes (10): AnimatorEvent, UnityEvent, Animation, Animator, bool, float, int, string (+2 more)

### Community 50 - "BFX_BloodDecalLayers"
Cohesion: 0.17
Nodes (9): Camera, DecalLayersProperty, DepthMode, RenderTexture, BFX_BloodDecalLayers, DepthTextureMode, LayerMask, DecalLayersProperty (+1 more)

### Community 52 - "MonoBehaviour"
Cohesion: 0.15
Nodes (5): MonoBehaviour, DismemberedLimbScaler, AnimationBlocker, int, JointBreakBroadcaster

### Community 53 - ".SetIKKeyframes"
Cohesion: 0.21
Nodes (5): Avatar, Quaternion, Transform, Vector3, Quaternion

### Community 55 - ".GetProps"
Cohesion: 0.22
Nodes (4): MuscleProps, Group, string, MusclePropsGroup

### Community 56 - ".GetAxisVectorToDirection"
Cohesion: 0.44
Nodes (5): Axis, Quaternion, Transform, Vector3, AxisTools

### Community 57 - "BakerMuscle"
Cohesion: 0.22
Nodes (5): AnimationClip, AnimationCurve, int, string, BakerMuscle

### Community 58 - "Actuator"
Cohesion: 0.22
Nodes (7): ConfigurableJoint, float, JointDrive, Quaternion, Rigidbody, Transform, Actuator

### Community 59 - ".GetFalloff"
Cohesion: 0.22
Nodes (5): bool, ConfigurableJoint, float, Group, Booster

### Community 60 - "BehaviourTemplate"
Cohesion: 0.18
Nodes (5): float, LayerMask, PuppetEvent, string, BehaviourTemplate

### Community 62 - "ParticleCollisionHandler"
Cohesion: 0.18
Nodes (7): Vector3, float, GameObject, LayerMask, List, ParticleSystem, ParticleCollisionHandler

### Community 63 - "BodiesImpulsePatch"
Cohesion: 0.25
Nodes (6): Dictionary, EftBulletClass, IEnumerator, MethodBase, PatchPostfix, BodiesImpulsePatch

### Community 64 - "ShellCasingPatch"
Cohesion: 0.20
Nodes (7): AmmoPoolObject, VisceralCombat.Combat.Patches, Queue, int, MethodBase, PatchPrefix, ShellCasingPatch

### Community 65 - "VisceralCombat.Dismemberment.Classes"
Cohesion: 0.27
Nodes (3): VisceralCombat.Dismemberment.Classes, VisceralCombat.Dismemberment.Patches, Nexus.BundleLoader

### Community 66 - ".CreateLimbJoints"
Cohesion: 0.22
Nodes (9): ColliderType, JointType, Limits, bool, float, Options, float, CreateJointParams (+1 more)

### Community 67 - "BakerUtilities"
Cohesion: 0.29
Nodes (3): Keyframe, AnimationCurve, BakerUtilities

### Community 68 - ".CreateFootCollider"
Cohesion: 0.51
Nodes (3): Collider, Transform, Vector3

### Community 69 - "CollisionEventBroadcaster"
Cohesion: 0.42
Nodes (4): Collision, CollisionEventBroadcaster, Collision, ICollisionEventListener

### Community 70 - "MuscleCollisionBroadcaster"
Cohesion: 0.33
Nodes (5): Collider, Collision, int, string, MuscleCollisionBroadcaster

### Community 71 - "RagdollEditor"
Cohesion: 0.24
Nodes (6): bool, Collider, ContextMenu, Rigidbody, Mode, RagdollEditor

### Community 72 - "RigidbodyController"
Cohesion: 0.22
Nodes (7): bool, float, Quaternion, Rigidbody, Transform, Vector3, RigidbodyController

### Community 73 - "ConfigurationManagerAttributes"
Cohesion: 0.22
Nodes (8): Action, CustomHotkeyDrawerFunc, ConfigurationManagerAttributes, bool, Func, int, object, string

### Community 74 - "BundleLoaderPlugin"
Cohesion: 0.31
Nodes (5): AssetBundle, Dictionary, Task, BundleLoaderPlugin, CancellationToken

### Community 75 - ".HandleDeathAudio"
Cohesion: 0.33
Nodes (5): VisceralCombat.Combined.Classes, EPhraseTrigger, EBodyPart, Player, DeathAudioController

### Community 77 - ".Postfix"
Cohesion: 0.22
Nodes (6): MaterialType, EBodyPart, MethodBase, PatchPostfix, Player, ShootOffHelmetPatch

### Community 78 - "ParticleFloorPainter"
Cohesion: 0.22
Nodes (6): float, GameObject, int, List, ParticleSystem, ParticleFloorPainter

### Community 79 - ".Postfix"
Cohesion: 0.25
Nodes (6): GameObject, IExplosiveItem, MethodBase, PatchPostfix, Vector3, PlayerDetonationPatch

### Community 81 - "V2Tools"
Cohesion: 0.33
Nodes (3): Vector2, Vector3, V2Tools

### Community 82 - "GameStartedPatch"
Cohesion: 0.25
Nodes (5): GameWorld, LayerMask, MethodBase, PatchPostfix, GameStartedPatch

### Community 83 - "LimbKillPatch"
Cohesion: 0.33
Nodes (5): EftBulletClass, IEnumerator, MethodBase, PatchPostfix, LimbKillPatch

### Community 84 - "CreateBSGRagdollPatch"
Cohesion: 0.25
Nodes (4): Corpse, MethodBase, PatchPrefix, CreateBSGRagdollPatch

### Community 85 - "MovementContextPatch"
Cohesion: 0.25
Nodes (5): FieldInfo, MovementContext, MethodBase, PatchPrefix, MovementContextPatch

### Community 86 - "Props"
Cohesion: 0.25
Nodes (7): InternalCollisionIgnoreSettings, bool, ConfigurableJoint, float, InternalCollisionIgnoreSettings, Props, State

### Community 87 - "PhysicalItemsPatch"
Cohesion: 0.25
Nodes (5): LootItem, int, MethodBase, PatchPrefix, PhysicalItemsPatch

### Community 88 - "Weight"
Cohesion: 0.25
Nodes (6): Mode, AnimationCurve, float, string, Mode, Weight

### Community 89 - "float"
Cohesion: 0.29
Nodes (8): NormalMode, bool, float, LayerMask, PhysicMaterial, CollisionResistanceMultiplier, MasterProps, MuscleProps

### Community 90 - ".CopyCollider"
Cohesion: 0.36
Nodes (4): SphereCollider, BoxCollider, CapsuleCollider, GameObject

### Community 91 - ".Postfix"
Cohesion: 0.25
Nodes (6): DamageInfoStruct, EBodyPart, MethodBase, PatchPostfix, Player, KillClientPatch

### Community 92 - "MuscleHit"
Cohesion: 0.29
Nodes (4): float, int, Vector3, MuscleHit

### Community 93 - "SubBehaviourBase"
Cohesion: 0.39
Nodes (3): Vector2, Vector3, SubBehaviourBase

### Community 94 - "AttachWeaponPatch"
Cohesion: 0.25
Nodes (5): MethodBase, PatchPostfix, RagdollClass, Rigidbody, AttachWeaponPatch

### Community 95 - "GrenadeDeadBodiesPatch"
Cohesion: 0.25
Nodes (5): IExplosiveItem, MethodBase, PatchPostfix, Vector3, GrenadeDeadBodiesPatch

### Community 96 - "PropertyAttribute"
Cohesion: 0.29
Nodes (5): PropertyAttribute, string, InspectorComment, string, LargeHeader

### Community 97 - "DefaultPlayPatch"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPrefix, Player, DefaultPlayPatch

### Community 98 - "PlaySoundBankPatch"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPrefix, Player, PlaySoundBankPatch

### Community 99 - "PlayStepSoundPatch"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPrefix, Player, PlayStepSoundPatch

### Community 100 - "VisceralHandshakePacket"
Cohesion: 0.29
Nodes (3): NetDataReader, NetDataWriter, VisceralHandshakePacket

### Community 102 - "TriggerEventBroadcaster"
Cohesion: 0.38
Nodes (3): Collider, GameObject, TriggerEventBroadcaster

### Community 103 - ".Log"
Cohesion: 0.33
Nodes (4): Logger, bool, Transform, Warning

### Community 104 - "GameStartedPatch"
Cohesion: 0.33
Nodes (4): GameWorld, MethodBase, PatchPostfix, GameStartedPatch

### Community 105 - "BipedLimbOrientations"
Cohesion: 0.40
Nodes (4): LimbOrientation, Vector3, BipedLimbOrientations, LimbOrientation

### Community 106 - "FixFootColliders"
Cohesion: 0.40
Nodes (3): ContextMenu, Transform, FixFootColliders

### Community 108 - "BaseUnityPlugin"
Cohesion: 0.50
Nodes (3): BaseUnityPlugin, VolumetricBloodFX, Entry

### Community 109 - "LayerCollisionData"
Cohesion: 0.50
Nodes (3): List, string, LayerCollisionData

### Community 110 - "EffectContainer"
Cohesion: 0.50
Nodes (3): GameObject, List, EffectContainer

## Knowledge Gaps
- **30 isolated node(s):** `VisceralCombat.Combined.Classes`, `State`, `NormalMode`, `Group`, `Mode` (+25 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PuppetMaster` connect `PuppetMaster` to `PhysXTools`, `Prop`, `PuppetMasterLite`, `Muscle`, `BehaviourBase`, `BehaviourFall`, `.AddMuscle`, `PuppetMasterProp`, `PropMuscle`, `_003CAliveToDead_003Ed__226`, `RagdollHelperClass`, `.FixedUpdate`, `PuppetMasterSettings`, `.Initiate`, `Transform`, `.SetState`, `PuppetMasterHumanoidConfig`, `VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics`, `MonoBehaviour`, `.ReconnectMuscle`, `MuscleCollisionBroadcaster`, `IEnumerator`, `Weight`, `PuppetMasterTools`?**
  _High betweenness centrality (0.290) - this node is a cross-community bridge._
- **Why does `VisceralCombat.Ragdolls.Classes.RootMotion` connect `VisceralCombat.Ragdolls.Classes.RootMotion` to `BipedNaming`, `BakerHumanoidQT`, `Baker`, `BakerUtilities`, `PropertyAttribute`, `SolverManager`, `TriggerEventBroadcaster`, `.Log`, `Interp`, `TQ`, `BipedLimbOrientations`, `BakerTransform`, `HumanoidBaker`, `Singleton`, `V2Tools`, `LazySingleton`, `LayerMaskExtensions`, `BakerMuscle`?**
  _High betweenness centrality (0.187) - this node is a cross-community bridge._
- **Why does `Baker` connect `Baker` to `Weight`, `MonoBehaviour`, `BakerTransform`, `HumanoidBaker`?**
  _High betweenness centrality (0.152) - this node is a cross-community bridge._
- **What connects `VisceralCombat.Combined.Classes`, `State`, `NormalMode` to the rest of the system?**
  _30 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `BipedNaming` be split into smaller, more focused modules?**
  _Cohesion score 0.07120500782472614 - nodes in this community are weakly interconnected._
- **Should `PhysXTools` be split into smaller, more focused modules?**
  _Cohesion score 0.07764705882352942 - nodes in this community are weakly interconnected._
- **Should `Baker` be split into smaller, more focused modules?**
  _Cohesion score 0.05053191489361702 - nodes in this community are weakly interconnected._