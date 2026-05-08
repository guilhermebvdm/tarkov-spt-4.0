using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cutscene;
using EFT;
using EFT.CameraControl;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class GClass1048
{
	[NonSerialized]
	public static GClass1048 Gclass1048_0;

	public Dictionary<int, BaseCutsceneTrigger> cutscenesTriggers = new Dictionary<int, BaseCutsceneTrigger>();

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	public PlayerCameraController PlayerCameraController_0;

	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public PlayableDirector PlayableDirector_0;

	[NonSerialized]
	public BaseCutsceneTrigger BaseCutsceneTrigger_0;

	[NonSerialized]
	public Scene Scene_0;

	public static GClass1048 Instance
	{
		get
		{
			if (Gclass1048_0 == null)
			{
				smethod_0();
			}
			return Gclass1048_0;
		}
	}

	public GClass1048()
	{
		GameWorld.OnDispose -= method_0;
		GameWorld.OnDispose += method_0;
	}

	public static void smethod_0()
	{
		Gclass1048_0 = new GClass1048();
		PlayerCameraController.OnPlayerCameraControllerCreated += Gclass1048_0.method_1;
		GameObject gameObject = new GameObject("PlayerCutscenesDirector");
		Gclass1048_0.PlayableDirector_0 = gameObject.AddComponent<PlayableDirector>();
		Gclass1048_0.PlayableDirector_0.playOnAwake = false;
		Gclass1048_0.PlayableDirector_0.stopped += Gclass1048_0.method_4;
	}

	public void method_0()
	{
		Gclass1048_0 = null;
		if (CancellationTokenSource_0 != null)
		{
			CancellationTokenSource_0.Cancel();
		}
		GameWorld.OnDispose -= method_0;
		PlayerCameraController.OnPlayerCameraControllerCreated -= method_1;
	}

	public void method_1(PlayerCameraController playerCameraController, Camera camera)
	{
		PlayerCameraController.OnPlayerCameraControllerCreated -= method_1;
		PlayerCameraController_0 = playerCameraController;
	}

	public void AddActiveCutsceneTrigger(BaseCutsceneTrigger trigger)
	{
		trigger.OnPlayerCausesCutscene += method_2;
		cutscenesTriggers[trigger.CutsceneID] = trigger;
	}

	public void method_2(BaseCutsceneTrigger causedTrigger, Player player)
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		method_3(causedTrigger, player, CancellationTokenSource_0.Token).HandleExceptions();
	}

	public bool IsPlaying()
	{
		if (PlayableDirector_0 == null)
		{
			return false;
		}
		return PlayableDirector_0.state == PlayState.Playing;
	}

	public async Task method_3(BaseCutsceneTrigger causedTrigger, Player player, CancellationToken cToken)
	{
		if (!player.IsYourPlayer)
		{
			return;
		}
		Scene_0 = PlayerCameraController_0.Camera.gameObject.scene;
		GlobalEventHandlerClass.Instance.CreateCommonEvent<GClass3548>().Invoke();
		GamePlayerOwner.SetIgnoreInputWithKeepResetLook(ignore: true);
		player.MovementContext.PlayerAnimatorEnableInert(enabled: false);
		player.ResetLookDirection();
		player.Look(0f, 0f);
		bool num = player.MovementContext.IsInPronePose != causedTrigger.NeedToProneAtStart || (!player.MovementContext.IsInPronePose && player.MovementContext.StraightenTimer + 2f > Time.time);
		bool flag = player.MovementContext.PoseLevel != causedTrigger.StartPlayerPosLevel;
		player.MovementContext.IsInPronePose = causedTrigger.NeedToProneAtStart;
		if (num)
		{
			await Task.Delay(500, cToken);
		}
		if (cToken.IsCancellationRequested)
		{
			return;
		}
		player.MovementContext.SetPoseLevel(causedTrigger.StartPlayerPosLevel);
		if (flag && !player.MovementContext.IsInPronePose)
		{
			await Task.Delay(500, cToken);
		}
		if (cToken.IsCancellationRequested)
		{
			return;
		}
		WorldInteractiveObject.GStruct436 interactionParameters = new WorldInteractiveObject.GStruct436
		{
			InteractionPosition = causedTrigger.StartPosition,
			Grip = null,
			AnimationId = 0,
			ViewTarget = causedTrigger.StartViewDirection,
			Snap = true,
			RotationMode = WorldInteractiveObject.ERotationInterpolationMode.ViewTarget,
			BlockChangePosLevel = true
		};
		player.MovementContext.InteractionParameters = interactionParameters;
		ApproachStateClass approachStateClass = null;
		IPlayerStateContainerBehaviour[] getInitedMovementState = player.MovementContext.GetInitedMovementState;
		for (int i = 0; i < getInitedMovementState.Length; i++)
		{
			if (getInitedMovementState[i].EncapsulatedState is ApproachStateClass approachStateClass2)
			{
				approachStateClass = approachStateClass2;
				player.MovementContext.OverrideState(approachStateClass);
				break;
			}
		}
		if (approachStateClass == null)
		{
			return;
		}
		while (!approachStateClass.ApproachDone)
		{
			await Task.Yield();
		}
		GamePlayerOwner.SetIgnoreInput(ignore: true);
		await Task.Delay(200, cToken);
		if (cToken.IsCancellationRequested)
		{
			return;
		}
		while (player.HandsController.IsInInteraction())
		{
			await Task.Yield();
		}
		await Task.Delay(500, cToken);
		if (cToken.IsCancellationRequested)
		{
			return;
		}
		player.MovementContext.ExitOverridenState();
		GamePlayerOwner.SetIgnoreInputWithKeepResetLook(ignore: false);
		GamePlayerOwner.SetIgnoreInput(ignore: true);
		PlayerCameraController_0.Camera.transform.SetParent(player.gameObject.transform);
		player.HandsController.ControllerGameObject.transform.SetParent(player.gameObject.transform);
		player.MovementContext.ToggleBlockInputPlayerRotation(block: true);
		Player_0 = player;
		BaseCutsceneTrigger_0 = causedTrigger;
		Animator animator = player.gameObject.GetComponent<Animator>();
		if (animator == null)
		{
			animator = player.gameObject.AddComponent<Animator>();
		}
		PlayableDirector_0.playableAsset = causedTrigger.TimeLineAsset;
		foreach (TrackAsset outputTrack in (PlayableDirector_0.playableAsset as TimelineAsset).GetOutputTracks())
		{
			PlayableDirector_0.SetGenericBinding(outputTrack, animator);
		}
		PlayableDirector_0.Play();
		if (BaseCutsceneTrigger_0.fakePlayerSteps != null)
		{
			BaseCutsceneTrigger_0.fakePlayerSteps.ToggleFakeStepsSound(player, toggle: true);
		}
	}

	public void method_4(PlayableDirector director)
	{
		if (!(Player_0 == null))
		{
			PlayerCameraController_0.Camera.transform.SetParent(null);
			SceneManager.MoveGameObjectToScene(PlayerCameraController_0.Camera.gameObject, Scene_0);
			Player_0.HandsController.ControllerGameObject.transform.SetParent(null);
			Player_0.MovementContext.ToggleBlockInputPlayerRotation(block: false);
			GamePlayerOwner.SetIgnoreInput(ignore: false);
			Vector2 vector = new Vector2(Player_0.CameraContainer.transform.rotation.eulerAngles.y, Player_0.CameraContainer.transform.rotation.eulerAngles.x);
			Player_0.MovementContext.SetDirectlyLookRotations(vector, vector);
			BaseCutsceneTrigger_0.CallEndCutscene();
			if (BaseCutsceneTrigger_0.fakePlayerSteps != null)
			{
				BaseCutsceneTrigger_0.fakePlayerSteps.ToggleFakeStepsSound(null, toggle: false);
			}
			BaseCutsceneTrigger_0 = null;
			Player_0 = null;
		}
	}

	public static CutsceneTriggerStartInfoSO.StartPlayerValues CalculateMyPlayerInfoOnStartCutscene(Player player)
	{
		Vector3 startViewDirection = player.HandsController.WeaponRoot.position + player.HandsController.WeaponRoot.up * -2f;
		return new CutsceneTriggerStartInfoSO.StartPlayerValues
		{
			startPosition = player.MovementContext.TransformPosition,
			startViewDirection = startViewDirection,
			startPlayerPosLevel = player.PoseLevel,
			needToProneAtStart = player.IsInPronePose
		};
	}
}
