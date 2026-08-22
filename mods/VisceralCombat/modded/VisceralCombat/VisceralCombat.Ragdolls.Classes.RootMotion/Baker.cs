using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine.Playables;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[HelpURL("http://www.root-motion.com/finalikdox/html/page3.html")]
[AddComponentMenu("Scripts/RootMotion/Baker")]
public abstract class Baker : MonoBehaviour
{
	[Serializable]
	public enum Mode
	{
		AnimationClips,
		AnimationStates,
		PlayableDirector,
		Realtime
	}

	public delegate void BakerDelegate(AnimationClip clip, float time);

	[Serializable]
	public class ClipSettings
	{
		[Serializable]
		public enum BasedUponRotation
		{
			Original,
			BodyOrientation
		}

		[Serializable]
		public enum BasedUponY
		{
			Original,
			CenterOfMass,
			Feet
		}

		[Serializable]
		public enum BasedUponXZ
		{
			Original,
			CenterOfMass
		}

		public bool loopTime;

		public bool loopBlend;

		public float cycleOffset;

		public bool loopBlendOrientation;

		public BasedUponRotation basedUponRotation;

		public float orientationOffsetY;

		public bool loopBlendPositionY;

		public BasedUponY basedUponY;

		public float level;

		public bool loopBlendPositionXZ;

		public BasedUponXZ basedUponXZ;

		public bool mirror;
	}

	[Tooltip("In AnimationClips, AnimationStates or PlayableDirector mode - the frame rate at which the animation clip will be sampled. In Realtime mode - the frame rate at which the pose will be sampled. With the latter, the frame rate is not guaranteed if the player is not able to reach it.")]
	[Range(1f, 90f)]
	public int frameRate = 30;

	[Tooltip("Maximum allowed error for keyframe reduction.")]
	[Range(0f, 0.1f)]
	public float keyReductionError = 0.01f;

	[Tooltip("AnimationClips mode can be used to bake a batch of AnimationClips directly without the need of setting up an AnimatorController. AnimationStates mode is useful for when you need to set up a more complex rig with layers and AvatarMasks in Mecanim. PlayableDirector mode bakes a Timeline. Realtime mode is for continuous baking of gameplay, ragdoll phsysics or PuppetMaster dynamics.")]
	public Mode mode;

	[Tooltip("AnimationClips to bake.")]
	public AnimationClip[] animationClips = (AnimationClip[])(object)new AnimationClip[0];

	[Tooltip("The name of the AnimationStates to bake (must be on the base layer) in the Animator above (Right-click on this component header and select 'Find Animation States' to have Baker fill those in automatically, required that state names match with the names of the clips used in them).")]
	public string[] animationStates = new string[0];

	[Tooltip("The folder to save the baked AnimationClips to.")]
	public string saveToFolder = "Assets";

	[Tooltip("String that will be added to each clip or animation state name for the saved clip. For example if your animation state/clip names were 'Idle' and 'Walk', then with '_Baked' as Append Name, the Baker will create 'Idle_Baked' and 'Walk_Baked' animation clips.")]
	public string appendName = "_Baked";

	[Tooltip("Name of the created AnimationClip file.")]
	public string saveName = "Baked Clip";

	[HideInInspector]
	public Animator animator;

	[HideInInspector]
	public PlayableDirector director;

	public BakerDelegate OnStartClip;

	public BakerDelegate OnUpdateClip;

	[Tooltip("If enabled, baked clips will have the same AnimationClipSettings as the clips used for baking. If disabled, clip settings from below will be applied to all the baked clips.")]
	public bool inheritClipSettings;

	[Tooltip("AnimationClipSettings applied to the baked animation clip.")]
	public ClipSettings clipSettings;

	protected bool addLoopFrame;

	public bool isBaking { get; private set; }

	public float bakingProgress { get; private set; }

	protected float clipLength { get; private set; }

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page3.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_baker.html");
	}

	[ContextMenu("Support Group")]
	private void SupportGroup()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	private void ASThread()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	protected abstract Transform GetCharacterRoot();

	protected abstract void OnStartBaking();

	protected abstract void OnSetLoopFrame(float time);

	protected abstract void OnSetCurves(ref AnimationClip clip);

	protected abstract void OnSetKeyframes(float time, bool lastFrame);

	public void BakeClip()
	{
	}

	public void StartBaking()
	{
	}

	public void StopBaking()
	{
	}
}
