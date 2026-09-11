# Graph Report - modded  (2026-09-10)

## Corpus Check
- 133 files · ~107,974 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2073 nodes · 3677 edges · 140 communities (117 shown, 15 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 135 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `06ab4491`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- BipedNaming
- PressureSensor
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
- ConfigurableJoint
- BipedRagdollCreator
- BehaviourPuppet
- RagdollCreator
- PuppetMasterProp
- JointConverter
- ParticleFloorPainter
- PropMuscle
- ShowIfAttribute
- RagdollClassPatch
- DebuggerHidden
- DismembermentPacket
- RagdollHelperClass
- MuscleCollision
- GoreObjectPool
- QuaTools
- KillPatch
- VisceralCombat.Dismemberment.Classes
- PuppetMasterSettings
- BakerHumanoidQT
- SubBehaviourCOM
- SolverManager
- .SetupPuppetMaster
- .Postfix
- Utils
- LivingDismembermentPacket
- TQ
- .AddMuscle
- .SetState
- .Log
- PuppetMasterHumanoidConfig
- HumanoidBaker
- InterpolationMode
- Vector3
- .DeathSetup
- BFX_BloodDecalLayers
- VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics
- .Initiate
- .DismemberLimb
- LayerMaskExtensions
- .GetProps
- .GetAxisVectorToDirection
- BakerMuscle
- Actuator
- .GetFalloff
- BehaviourTemplate
- ParticleCollisionHandler
- .ProcessImpulse
- ShellCasingPatch
- ProneLockPatch
- .CreateLimbJoints
- .SetCurves
- .CreateFootCollider
- HitEffectPacket
- MuscleCollisionBroadcaster
- RagdollEditor
- _003CActivation_003Ed__21
- ClipSettings
- BundleLoaderPlugin
- .HandleDeathAudio
- AnimationModifierStack
- .Postfix
- VisceralNetworkUtils
- GenericBaker
- V2Tools
- GameStartedPatch
- LimbKillPatch
- CreateBSGRagdollPatch
- VisceralCombat.Ragdolls.Patches
- AnimationModifier
- PhysicalItemsPatch
- Mode
- .Initiate
- .CopyCollider
- IEnumerator
- SubBehaviourBase
- .Awake
- .Postfix
- VisceralCombat.Ragdolls.Classes.RootMotion
- AttachWeaponPatch
- BFX_ShaderProperies
- MonoBehaviour
- VisceralHandshakePacket
- .RealignRagdoll
- TriggerEventBroadcaster
- .Log
- EffectContainer
- BipedLimbOrientations
- Mode
- BFX_RenderDepth
- WeaponDropOnDeathPatch
- LayerCollisionData
- BFX_ManualAnimationUpdate
- LazySingleton
- bundleloader.csproj
- VisceralCombat.csproj
- ConfigurationManagerAttributes
- PuppetEvent
- VolumetricBloodFX.csproj
- BasedUponY
- NormalMode
- State
- Mode
- State
- UpdateMode
- Direction
- BFX_BloodSettings
- .GetCharacterRoot
- .V3
- .ParseDismembermentJson
- .Activate
- Quaternion
- ELogType
- .TransformPointUnscaled
- Singleton
- _003CDisabledToActive_003Ed__171
- ShowIfMode
- .LayerMaskRun

## God Nodes (most connected - your core abstractions)
1. `PuppetMaster` - 190 edges
2. `Muscle` - 99 edges
3. `VisceralEntry` - 79 edges
4. `BehaviourPuppet` - 66 edges
5. `BehaviourBase` - 62 edges
6. `VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics` - 46 edges
7. `VisceralCombat.Ragdolls.Classes.RootMotion` - 35 edges
8. `KillPatch` - 31 edges
9. `Baker` - 31 edges
10. `Interp` - 31 edges

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

## Communities (140 total, 15 thin omitted)

### Community 0 - "BipedNaming"
Cohesion: 0.06
Nodes (30): AutoDetectParams, BoneSide, BoneType, Object, SkinnedMeshRenderer, Transform, BipedNaming, BoneSide (+22 more)

### Community 1 - "PressureSensor"
Cohesion: 0.05
Nodes (36): BehaviourUpdateDelegate, ForceMode, Settings, CapsuleCollider, ConfigurableJoint, Quaternion, Rigidbody, Vector3 (+28 more)

### Community 2 - "Baker"
Cohesion: 0.12
Nodes (8): ClipSettings, AnimationClip, Animator, ContextMenu, Baker, bakingProgress, clipLength, isBaking

### Community 3 - "MuscleLite"
Cohesion: 0.06
Nodes (25): Collision, CollisionEventBroadcaster, Collision, ICollisionEventListener, ConfigurableJoint, JointDrive, Quaternion, Rigidbody (+17 more)

### Community 4 - "BFX_DecalSettings"
Cohesion: 0.17
Nodes (6): MeshRenderer, BFX_DecalSettings, AnimationCurve, Renderer, Transform, Vector3

### Community 5 - "PuppetMaster"
Cohesion: 0.05
Nodes (24): AnimatorUpdateMode, Color, MuscleDelegate, StateSettings, UpdateDelegate, ContextMenu, List, PuppetMaster (+16 more)

### Community 6 - "Prop"
Cohesion: 0.06
Nodes (24): Group, Arm, Foot, Hand, Head, Hips, Leg, Prop (+16 more)

### Community 7 - "PuppetMasterLite"
Cohesion: 0.12
Nodes (9): PuppetMasterLiteDelegate, UpdateMode, Animator, IteratorStateMachine, Transform, PuppetMasterLite, UpdateMode, Fixed (+1 more)

### Community 9 - "Muscle"
Cohesion: 0.06
Nodes (25): TargetChild, ConfigurableJoint, ConfigurableJointMotion, JointDrive, InternalCollisionIgnoreSettings, Muscle, additionalRigidbody, colliders (+17 more)

### Community 10 - "BehaviourBase"
Cohesion: 0.08
Nodes (6): HitDelegate, CollisionDelegate, Quaternion, Vector3, BehaviourBase, forceActive

### Community 11 - "VisceralEntry"
Cohesion: 0.03
Nodes (58): ConfigEntry, ManualLogSource, AssetBundle, BoxCollider, CollisionDetectionMode, GameObject, GameWorld, HashSet (+50 more)

### Community 12 - "BehaviourFall"
Cohesion: 0.07
Nodes (11): ContextMenu, DebuggerHidden, IteratorStateMachine, LayerMask, PuppetEvent, _003CSmoothActivate_003Ed__23, Current, BehaviourFall (+3 more)

### Community 13 - "BakerTransform"
Cohesion: 0.22
Nodes (6): AnimationClip, AnimationCurve, Quaternion, Transform, Vector3, BakerTransform

### Community 15 - "BipedRagdollCreator"
Cohesion: 0.14
Nodes (8): BipedReferences, Options, PlayerBones, ContextMenu, BipedRagdollCreator, Animator, Transform, BipedRagdollReferences

### Community 16 - "BehaviourPuppet"
Cohesion: 0.08
Nodes (17): CollisionImpulseDelegate, CollisionResistanceMultiplier, MasterProps, MusclePropsGroup, NormalMode, CollisionDelegate, ContextMenu, LayerMask (+9 more)

### Community 17 - "RagdollCreator"
Cohesion: 0.12
Nodes (16): Direction, Animator, BoxCollider, CapsuleCollider, Collider, Joint, Rigidbody, Transform (+8 more)

### Community 18 - "PuppetMasterProp"
Cohesion: 0.10
Nodes (14): RigidbodyConstraints, RigidbodyInterpolation, Collider, CollisionDetectionMode, PhysicMaterial, Props, Rigidbody, Transform (+6 more)

### Community 19 - "JointConverter"
Cohesion: 0.20
Nodes (12): FixedJoint, HingeJoint, JointLimits, JointSpring, SoftJointLimitSpring, CharacterJoint, ConfigurableJoint, GameObject (+4 more)

### Community 20 - "ParticleFloorPainter"
Cohesion: 0.09
Nodes (22): AmmoItemClass, BallisticsCalculator, GameObject, List, ParticleCollisionEvent, ParticleSystem, ParticleFloorPainter, Collider (+14 more)

### Community 21 - "PropMuscle"
Cohesion: 0.10
Nodes (10): PropDelegate, MuscleDisconnectMode, Explode, Sever, ConfigurableJoint, Rigidbody, Vector3, PropMuscle (+2 more)

### Community 22 - "ShowIfAttribute"
Cohesion: 0.15
Nodes (10): ShowIfAttribute, indent, mode, otherPropValue, propName, propValue, ShowLargeHeaderIf, ShowRangeIfAttribute (+2 more)

### Community 23 - "RagdollClassPatch"
Cohesion: 0.24
Nodes (6): IEnumerator, MethodBase, PatchPrefix, RagdollClass, Rigidbody, RagdollClassPatch

### Community 24 - "DebuggerHidden"
Cohesion: 0.15
Nodes (10): IDisposable, DebuggerHidden, _003CActiveToDisabled_003Ed__174, Current, _003CActiveToKinematic_003Ed__175, Current, _003CAliveToDead_003Ed__226, Current (+2 more)

### Community 25 - "DismembermentPacket"
Cohesion: 0.16
Nodes (11): EBodyPart, NetDataReader, NetDataWriter, Vector3, DismembermentPacket, assetNames, bodyPartType, bone (+3 more)

### Community 26 - "RagdollHelperClass"
Cohesion: 0.16
Nodes (12): Collider, Dictionary, EBodyPart, HashSet, IEnumerator, List, Player, Rigidbody (+4 more)

### Community 27 - "MuscleCollision"
Cohesion: 0.18
Nodes (4): Collision, Collision, MuscleCollision, Collision

### Community 28 - "GoreObjectPool"
Cohesion: 0.21
Nodes (9): Dictionary, GameObject, IEnumerator, Quaternion, Queue, Transform, Vector3, GoreObjectPool (+1 more)

### Community 29 - "QuaTools"
Cohesion: 0.19
Nodes (3): Quaternion, Vector3, QuaTools

### Community 30 - "KillPatch"
Cohesion: 0.12
Nodes (15): AmmoTemplate, HeadDismemberOutcome, InventoryController, Dictionary, EBodyPart, Func, MethodBase, DismemberChances (+7 more)

### Community 31 - "VisceralCombat.Dismemberment.Classes"
Cohesion: 0.08
Nodes (11): VisceralCombat, VisceralCombat.Dismemberment.Classes.Packets, VisceralCombat.Ragdolls.Classes.Packets, VisceralCombat.Dismemberment.Classes, VisceralCombat.Combined.Classes, VisceralCombat.Dismemberment.Patches, VisceralCombat.Combat.Patches, QuickLogger (+3 more)

### Community 32 - "PuppetMasterSettings"
Cohesion: 0.15
Nodes (9): PuppetUpdateLimit, Singleton, List, PuppetMasterSettings, currentlyActivePuppets, currentlyDisabledPuppets, currentlyKinematicPuppets, puppets (+1 more)

### Community 33 - "BakerHumanoidQT"
Cohesion: 0.16
Nodes (7): AnimationCurve, Avatar, AvatarIKGoal, Quaternion, Transform, Vector3, BakerHumanoidQT

### Community 34 - "SubBehaviourCOM"
Cohesion: 0.13
Nodes (15): Quaternion, Vector3, Mode, CenterOfPressure, FeetCentroid, SubBehaviourCOM, angle, centerOfPressure (+7 more)

### Community 35 - "SolverManager"
Cohesion: 0.16
Nodes (6): Animation, Animator, Transform, SolverManager, animatePhysics, isAnimated

### Community 36 - ".SetupPuppetMaster"
Cohesion: 0.11
Nodes (13): AnimatorOverrideController, AnimationClip, GameObject, IEnumerable, IEnumerator, Player, Transform, Utils (+5 more)

### Community 37 - ".Postfix"
Cohesion: 0.20
Nodes (7): IExplosiveItem, MethodBase, ObservedLootItem, PatchPostfix, Rigidbody, Vector3, GrenadeItemsPatch

### Community 38 - "Utils"
Cohesion: 0.20
Nodes (7): Collider, GameObject, IEnumerable, List, Player, Transform, Utils

### Community 39 - "LivingDismembermentPacket"
Cohesion: 0.16
Nodes (11): EBodyPart, NetDataReader, NetDataWriter, Vector3, LivingDismembermentPacket, AssetNames, Bone, CapAssetName (+3 more)

### Community 41 - "TQ"
Cohesion: 0.22
Nodes (8): Avatar, AvatarIKGoal, HumanBodyBones, Quaternion, AvatarUtility, Quaternion, Vector3, TQ

### Community 42 - ".AddMuscle"
Cohesion: 0.12
Nodes (18): CharacterController, Cloth, Comments, Component, InternalCollisionIgnoreSettings, Props, mapPosition, Animator (+10 more)

### Community 43 - ".SetState"
Cohesion: 0.18
Nodes (3): LayerMask, Quaternion, Vector3

### Community 44 - ".Log"
Cohesion: 0.32
Nodes (4): BotOwner, EBodyPart, Player, LivingDismembermentController

### Community 45 - "PuppetMasterHumanoidConfig"
Cohesion: 0.22
Nodes (8): HumanoidMuscle, ScriptableObject, State, Animator, HumanBodyBones, Props, HumanoidMuscle, PuppetMasterHumanoidConfig

### Community 46 - "HumanoidBaker"
Cohesion: 0.16
Nodes (8): HumanPose, HumanPoseHandler, Quaternion, Animator, Quaternion, Transform, Vector3, HumanoidBaker

### Community 47 - "InterpolationMode"
Cohesion: 0.07
Nodes (28): InterpolationMode, BackInCubic, BackInQuartic, InBack, InCubic, InElastic, InElasticBig, InElasticSmall (+20 more)

### Community 48 - "Vector3"
Cohesion: 0.29
Nodes (3): Vector3, State, Default

### Community 49 - ".DeathSetup"
Cohesion: 0.17
Nodes (7): EBodyPart, NetDataReader, NetDataWriter, RagdollSyncPacket, BodyPart, PlayerID, RandomChance

### Community 50 - "BFX_BloodDecalLayers"
Cohesion: 0.13
Nodes (14): DecalLayersProperty, DepthMode, RenderTexture, BFX_BloodDecalLayers, Camera, DepthTextureMode, LayerMask, DecalLayersProperty (+6 more)

### Community 51 - "VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics"
Cohesion: 0.11
Nodes (7): VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics, VisceralCombat.Ragdolls.Classes, Nexus.BundleLoader, MuscleRemoveMode, Explode, Numb, Sever

### Community 52 - ".Initiate"
Cohesion: 0.17
Nodes (3): SolverManager, JointBreakBroadcaster, Animation

### Community 53 - ".DismemberLimb"
Cohesion: 0.24
Nodes (11): Skin, Collider, DamageInfoStruct, GameObject, Joint, ParticleSystem, PatchPostfix, Player (+3 more)

### Community 55 - ".GetProps"
Cohesion: 0.24
Nodes (3): MuscleProps, Group, MusclePropsGroup

### Community 56 - ".GetAxisVectorToDirection"
Cohesion: 0.24
Nodes (8): Axis, X, Y, Z, Quaternion, Transform, Vector3, AxisTools

### Community 57 - "BakerMuscle"
Cohesion: 0.31
Nodes (3): AnimationClip, AnimationCurve, BakerMuscle

### Community 58 - "Actuator"
Cohesion: 0.29
Nodes (6): ConfigurableJoint, JointDrive, Quaternion, Rigidbody, Transform, Actuator

### Community 59 - ".GetFalloff"
Cohesion: 0.28
Nodes (3): ConfigurableJoint, Group, Booster

### Community 60 - "BehaviourTemplate"
Cohesion: 0.11
Nodes (7): BehaviourDelegate, LayerMask, PuppetEvent, BehaviourTemplate, Vector3, MuscleHit, LayerMask

### Community 62 - "ParticleCollisionHandler"
Cohesion: 0.33
Nodes (5): LayerMask, List, ParticleCollisionEvent, ParticleSystem, ParticleCollisionHandler

### Community 63 - ".ProcessImpulse"
Cohesion: 0.13
Nodes (12): EftBulletClass, HashSet, IEnumerator, VisceralShotProcessor, Dictionary, EftBulletClass, MethodBase, ObservedLootItem (+4 more)

### Community 64 - "ShellCasingPatch"
Cohesion: 0.25
Nodes (6): AmmoPoolObject, HashSet, MethodBase, PatchPrefix, Queue, ShellCasingPatch

### Community 65 - "ProneLockPatch"
Cohesion: 0.22
Nodes (6): BotLay, BotMover, MethodBase, PatchPrefix, ProneLockPatch, ProneMoverDoPronePatch

### Community 66 - ".CreateLimbJoints"
Cohesion: 0.18
Nodes (12): ColliderType, CreateJointParams, JointType, Limits, Rigidbody, Options, Default, CharacterJoint (+4 more)

### Community 67 - ".SetCurves"
Cohesion: 0.17
Nodes (6): Keyframe, AnimationClip, Animator, AnimationCurve, BakerUtilities, AnimationClip

### Community 68 - ".CreateFootCollider"
Cohesion: 0.51
Nodes (3): Collider, Transform, Vector3

### Community 69 - "HitEffectPacket"
Cohesion: 0.20
Nodes (10): INetSerializable, NetDataReader, NetDataWriter, Vector3, HitEffectPacket, colliderName, Direction, HitPoint (+2 more)

### Community 70 - "MuscleCollisionBroadcaster"
Cohesion: 0.23
Nodes (6): Collider, Collision, Vector3, MuscleCollisionBroadcaster, Collider, GameObject

### Community 71 - "RagdollEditor"
Cohesion: 0.22
Nodes (7): Collider, ContextMenu, Rigidbody, Mode, Colliders, Joints, RagdollEditor

### Community 72 - "_003CActivation_003Ed__21"
Cohesion: 0.22
Nodes (5): DebuggerHidden, _003CActivation_003Ed__21, Current, _003CDeactivation_003Ed__23, Current

### Community 73 - "ClipSettings"
Cohesion: 0.20
Nodes (10): BasedUponRotation, BasedUponXZ, BasedUponY, BasedUponRotation, BodyOrientation, Original, BasedUponXZ, CenterOfMass (+2 more)

### Community 74 - "BundleLoaderPlugin"
Cohesion: 0.17
Nodes (10): AssetBundleCreateRequest, BaseUnityPlugin, AssetBundle, Dictionary, Task, BundleLoaderPlugin, Instance, CancellationToken (+2 more)

### Community 75 - ".HandleDeathAudio"
Cohesion: 0.48
Nodes (4): EPhraseTrigger, EBodyPart, Player, DeathAudioController

### Community 76 - "AnimationModifierStack"
Cohesion: 0.28
Nodes (4): BakerDelegate, AnimationClip, Animator, AnimationModifierStack

### Community 77 - ".Postfix"
Cohesion: 0.20
Nodes (7): EBodyPartColliderType, DamageInfoStruct, EBodyPart, MethodBase, PatchPostfix, Player, ShootOffHelmetPatch

### Community 78 - "VisceralNetworkUtils"
Cohesion: 0.54
Nodes (4): EBodyPart, Player, Vector3, VisceralNetworkUtils

### Community 80 - "GenericBaker"
Cohesion: 0.29
Nodes (3): AnimationClip, Transform, GenericBaker

### Community 81 - "V2Tools"
Cohesion: 0.33
Nodes (3): Vector2, Vector3, V2Tools

### Community 82 - "GameStartedPatch"
Cohesion: 0.25
Nodes (5): GameWorld, LayerMask, MethodBase, PatchPostfix, GameStartedPatch

### Community 83 - "LimbKillPatch"
Cohesion: 0.18
Nodes (7): BodyPartCollider, EftBulletClass, HashSet, MethodBase, PatchPostfix, Player, LimbKillPatch

### Community 84 - "CreateBSGRagdollPatch"
Cohesion: 0.20
Nodes (6): Corpse, PlayerBody, FieldInfo, MethodBase, PatchPrefix, CreateBSGRagdollPatch

### Community 85 - "VisceralCombat.Ragdolls.Patches"
Cohesion: 0.11
Nodes (12): VisceralCombat.Ragdolls.Patches, MovementContext, HashSet, MethodBase, Type, CreateCorpsePatch, MethodBase, GrenadeDeadBodiesPatch (+4 more)

### Community 86 - "AnimationModifier"
Cohesion: 0.33
Nodes (3): AnimationClip, Animator, AnimationModifier

### Community 87 - "PhysicalItemsPatch"
Cohesion: 0.25
Nodes (5): LootItem, MethodBase, PatchPrefix, Type, PhysicalItemsPatch

### Community 88 - "Mode"
Cohesion: 0.25
Nodes (6): Mode, AnimationCurve, Mode, Curve, Float, Weight

### Community 89 - ".Initiate"
Cohesion: 0.22
Nodes (5): Collider, Renderer, Rigidbody, Transform, TargetChild

### Community 90 - ".CopyCollider"
Cohesion: 0.36
Nodes (4): BoxCollider, CapsuleCollider, GameObject, SphereCollider

### Community 93 - "SubBehaviourBase"
Cohesion: 0.39
Nodes (3): Vector2, Vector3, SubBehaviourBase

### Community 94 - ".Awake"
Cohesion: 0.08
Nodes (16): VisceralCombat.Combined.Patches, ModulePatch, MethodBase, PatchPrefix, Player, DefaultPlayPatch, MethodBase, PatchPrefix (+8 more)

### Community 95 - ".Postfix"
Cohesion: 0.40
Nodes (4): IExplosiveItem, PatchPostfix, Rigidbody, Vector3

### Community 96 - "VisceralCombat.Ragdolls.Classes.RootMotion"
Cohesion: 0.17
Nodes (4): VisceralCombat.Ragdolls.Classes.RootMotion, PropertyAttribute, InspectorComment, LargeHeader

### Community 97 - "AttachWeaponPatch"
Cohesion: 0.22
Nodes (6): MethodBase, PatchPostfix, RagdollClass, Rigidbody, SpringJoint, AttachWeaponPatch

### Community 98 - "BFX_ShaderProperies"
Cohesion: 0.36
Nodes (4): BFX_ShaderProperies, AnimationCurve, MaterialPropertyBlock, Renderer

### Community 99 - "MonoBehaviour"
Cohesion: 0.25
Nodes (5): MonoBehaviour, Comments, ContextMenu, Transform, FixFootColliders

### Community 100 - "VisceralHandshakePacket"
Cohesion: 0.14
Nodes (7): FikaNetworkManagerCreatedEvent, NetDataReader, NetDataWriter, VisceralHandshakePacket, IsRequest, ResponderNetId, IEnumerator

### Community 101 - ".RealignRagdoll"
Cohesion: 0.24
Nodes (6): BoxCollider, CapsuleCollider, Rigidbody, SphereCollider, Vector3, PuppetMasterTools

### Community 102 - "TriggerEventBroadcaster"
Cohesion: 0.38
Nodes (3): Collider, GameObject, TriggerEventBroadcaster

### Community 103 - ".Log"
Cohesion: 0.40
Nodes (3): Logger, Transform, Warning

### Community 104 - "EffectContainer"
Cohesion: 0.33
Nodes (5): GameObject, List, EffectContainer, GameWorld, PatchPostfix

### Community 105 - "BipedLimbOrientations"
Cohesion: 0.29
Nodes (6): LimbOrientation, Vector3, BipedLimbOrientations, MaxBiped, UMA, LimbOrientation

### Community 106 - "Mode"
Cohesion: 0.40
Nodes (5): Mode, AnimationClips, AnimationStates, PlayableDirector, Realtime

### Community 107 - "BFX_RenderDepth"
Cohesion: 0.40
Nodes (3): BFX_RenderDepth, Camera, DepthTextureMode

### Community 108 - "WeaponDropOnDeathPatch"
Cohesion: 0.25
Nodes (5): Item, MethodBase, PatchPrefix, Player, WeaponDropOnDeathPatch

### Community 110 - "BFX_ManualAnimationUpdate"
Cohesion: 0.39
Nodes (4): BFX_ManualAnimationUpdate, AnimationCurve, MaterialPropertyBlock, Renderer

### Community 111 - "LazySingleton"
Cohesion: 0.40
Nodes (3): LazySingleton, hasInstance, instance

### Community 114 - "ConfigurationManagerAttributes"
Cohesion: 0.33
Nodes (5): Action, ConfigEntryBase, CustomHotkeyDrawerFunc, ConfigurationManagerAttributes, Func

### Community 115 - "PuppetEvent"
Cohesion: 0.33
Nodes (4): AnimatorEvent, UnityEvent, PuppetEvent, switchBehaviour

### Community 117 - "BasedUponY"
Cohesion: 0.50
Nodes (4): BasedUponY, CenterOfMass, Feet, Original

### Community 121 - "NormalMode"
Cohesion: 0.50
Nodes (4): NormalMode, Active, Kinematic, Unmapped

### Community 122 - "State"
Cohesion: 0.50
Nodes (4): State, GetUp, Puppet, Unpinned

### Community 123 - "Mode"
Cohesion: 0.50
Nodes (4): Mode, Active, Disabled, Kinematic

### Community 124 - "State"
Cohesion: 0.50
Nodes (4): State, Alive, Dead, Frozen

### Community 125 - "UpdateMode"
Cohesion: 0.50
Nodes (4): UpdateMode, AnimatePhysics, FixedUpdate, Normal

### Community 126 - "Direction"
Cohesion: 0.50
Nodes (4): Direction, X, Y, Z

### Community 127 - "BFX_BloodSettings"
Cohesion: 0.33
Nodes (5): _DecalRenderinMode, BFX_BloodSettings, _DecalRenderinMode, AverageRayBetwenForwardAndFloor, Floor_XZ

### Community 130 - ".ParseDismembermentJson"
Cohesion: 0.40
Nodes (3): DismemberChances, Dictionary, List

### Community 131 - ".Activate"
Cohesion: 0.33
Nodes (3): Animation, Animator, AnimatorEvent

### Community 134 - "ELogType"
Cohesion: 0.40
Nodes (4): ELogType, Error, Log, Warn

### Community 138 - "ShowIfMode"
Cohesion: 0.50
Nodes (3): ShowIfMode, Disabled, Hidden

## Knowledge Gaps
- **299 isolated node(s):** `Log`, `Warn`, `Error`, `DismemberChances`, `None` (+294 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 741 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PuppetMaster` connect `PuppetMaster` to `PressureSensor`, `.UpdateHierarchies`, `Prop`, `PuppetMasterLite`, `_003CDisabledToActive_003Ed__171`, `BehaviourBase`, `Muscle`, `ConfigurableJoint`, `PuppetMasterProp`, `PropMuscle`, `DebuggerHidden`, `RagdollHelperClass`, `PuppetMasterSettings`, `.SetupPuppetMaster`, `.FixedUpdate`, `.AddMuscle`, `PuppetMasterHumanoidConfig`, `.DeathSetup`, `VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics`, `.Initiate`, `.DismemberLimb`, `.ReconnectMuscle`, `MuscleCollisionBroadcaster`, `Mode`, `.UpdateInternalCollisions`, `IEnumerator`, `MonoBehaviour`, `.RealignRagdoll`, `PuppetEvent`, `Mode`, `State`, `UpdateMode`?**
  _High betweenness centrality (0.370) - this node is a cross-community bridge._
- **Why does `VisceralCombat.Ragdolls.Classes.RootMotion` connect `VisceralCombat.Ragdolls.Classes.RootMotion` to `BipedNaming`, `Baker`, `Singleton`, `ShowIfMode`, `BakerTransform`, `ShowIfAttribute`, `QuaTools`, `BakerHumanoidQT`, `SolverManager`, `TQ`, `HumanoidBaker`, `.GetAxisVectorToDirection`, `BakerMuscle`, `.SetCurves`, `AnimationModifierStack`, `V2Tools`, `AnimationModifier`, `MonoBehaviour`, `TriggerEventBroadcaster`, `.Log`, `BipedLimbOrientations`, `LazySingleton`?**
  _High betweenness centrality (0.216) - this node is a cross-community bridge._
- **Why does `VisceralEntry` connect `VisceralEntry` to `.ParseDismembermentJson`, `VisceralHandshakePacket`, `LivingDismembermentPacket`, `EffectContainer`, `BundleLoaderPlugin`, `.LayerMaskRun`, `.DeathSetup`, `DismembermentPacket`, `.Awake`, `VisceralCombat.Dismemberment.Classes`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **What connects `Log`, `Warn`, `Error` to the rest of the system?**
  _299 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `BipedNaming` be split into smaller, more focused modules?**
  _Cohesion score 0.05700852189244784 - nodes in this community are weakly interconnected._
- **Should `PressureSensor` be split into smaller, more focused modules?**
  _Cohesion score 0.05336951605608322 - nodes in this community are weakly interconnected._
- **Should `Baker` be split into smaller, more focused modules?**
  _Cohesion score 0.12105263157894737 - nodes in this community are weakly interconnected._