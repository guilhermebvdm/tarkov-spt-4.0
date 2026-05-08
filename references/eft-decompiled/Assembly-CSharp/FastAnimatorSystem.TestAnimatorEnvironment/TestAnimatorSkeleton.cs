using AnimationSystem.RootMotionTable;
using UnityEngine;

namespace FastAnimatorSystem.TestAnimatorEnvironment;

public class TestAnimatorSkeleton : MonoBehaviour
{
	public Animator animator;

	public RuntimeAnimatorController originalAnimatorController;

	public TextAsset fastAnimatorJson;

	public RootMotionBlendTable blendTable;

	public CharacterClipsKeeper clipsKeeper;

	public Camera cam;

	public PlayableAnimator playableAnimator;

	public bool useFastAnimator;

	public bool usePlayableWithFast;

	public float speedMult = 1f;

	public Vector3 rotateAroundVec;

	public bool applyDeltaPosition = true;

	public bool isVisible = true;

	private Vector3 vector3_0 = Vector3.zero;

	private IAnimator ianimator_0;

	private int int_0 = -1;

	public IAnimator Animator => ianimator_0;

	public Vector3 LastDelta
	{
		get
		{
			return vector3_0;
		}
		set
		{
			vector3_0 = value;
		}
	}

	public void Init()
	{
		if (playableAnimator != null)
		{
			playableAnimator.enabled = useFastAnimator && usePlayableWithFast;
		}
		if (useFastAnimator)
		{
			blendTable.LoadNodes();
			FastAnimatorControllerClass fastAnimatorController = GClass1346.Deserialize(fastAnimatorJson.bytes);
			ianimator_0 = GClass1445.CreateAnimator(fastAnimatorController, blendTable._loadedNodes, base.transform, playableAnimator);
			if (usePlayableWithFast)
			{
				FastAnimatorProcessorClass fastAnimatorProcessorClass = ianimator_0 as FastAnimatorProcessorClass;
				playableAnimator.Init(ianimator_0, fastAnimatorProcessorClass.GetParametersCache(), blendTable, clipsKeeper, manualUpdate: true);
				playableAnimator.SetCuller(new GClass1340(playableAnimator));
				playableAnimator.Play();
				for (int i = 0; i < playableAnimator.initialLayerInfo.Length; i++)
				{
					fastAnimatorProcessorClass.SetLayerWeight(i, playableAnimator.initialLayerInfo[i].weight);
				}
			}
		}
		else
		{
			ianimator_0 = GClass1445.CreateAnimator(animator);
			ianimator_0.runtimeAnimatorController = originalAnimatorController;
		}
	}

	public void Process(float dt)
	{
		if (useFastAnimator)
		{
			ianimator_0.Update(dt);
			if (usePlayableWithFast)
			{
				playableAnimator.Process(isVisible, dt);
			}
			if (applyDeltaPosition)
			{
				Vector3 deltaPosition = ianimator_0.deltaPosition;
				Vector3 vector = base.transform.rotation * deltaPosition;
				base.transform.position += vector;
				vector3_0 = vector;
			}
		}
		else if (applyDeltaPosition)
		{
			Vector3 deltaPosition2 = ianimator_0.deltaPosition;
			base.transform.position += deltaPosition2;
			vector3_0 = deltaPosition2;
		}
	}

	public void method_0(string val)
	{
	}

	public AnimatorStateInfoWrapper GetCurrentState()
	{
		return ianimator_0.GetCurrentAnimatorStateInfo(0);
	}

	public AnimatorStateInfoWrapper GetNextState()
	{
		return ianimator_0.GetNextAnimatorStateInfo(0);
	}

	public Vector3 GetDeltaPosition()
	{
		return vector3_0;
	}
}
