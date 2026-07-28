using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class PropMuscle : MonoBehaviour
{
	public delegate void PropDelegate(PuppetMasterProp prop);

	[HideInInspector]
	public PuppetMaster puppetMaster;

	[Tooltip("The PuppetMasterProp currently held by this Prop Muscle. To pick up a prop, just assign it as propMuscle.currentProp. To drop, set propMuscle.currentProp to null. Replacing this value with another prop drops any previously held props.")]
	public PuppetMasterProp currentProp;

	[LargeHeader("Additional Pin")]
	[Tooltip("Offset of the additional pin from this Prop Muscle in local space.")]
	public Vector3 additionalPinOffset = Vector3.forward;

	public PropDelegate OnPickUpProp;

	public PropDelegate OnDropProp;

	private Muscle _muscle;

	private PuppetMasterProp lastProp;

	private Vector3 lastAdditionalPinOffset;

	public Muscle muscle
	{
		get
		{
			if (_muscle == null)
			{
				_muscle = puppetMaster.GetMuscle(((Component)this).GetComponent<ConfigurableJoint>());
			}
			return _muscle;
		}
	}

	public PuppetMasterProp activeProp { get; private set; }

	public bool AddAdditionalPin()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (!puppetMaster.initiated)
		{
			return false;
		}
		if ((Object)(object)muscle.additionalPin != (Object)null)
		{
			return false;
		}
		GameObject val = new GameObject("Additional Pin");
		val.gameObject.layer = ((Component)this).gameObject.layer;
		val.transform.parent = ((Component)this).transform;
		val.transform.localPosition = additionalPinOffset;
		val.transform.localRotation = Quaternion.identity;
		lastAdditionalPinOffset = additionalPinOffset;
		val.AddComponent<Rigidbody>();
		ConfigurableJoint val2 = val.AddComponent<ConfigurableJoint>();
		((Joint)val2).connectedBody = ((Component)muscle.joint).GetComponent<Rigidbody>();
		((Joint)val2).autoConfigureConnectedAnchor = false;
		((Joint)val2).connectedAnchor = additionalPinOffset;
		val2.xMotion = (ConfigurableJointMotion)0;
		val2.yMotion = (ConfigurableJointMotion)0;
		val2.zMotion = (ConfigurableJointMotion)0;
		val2.angularXMotion = (ConfigurableJointMotion)0;
		val2.angularYMotion = (ConfigurableJointMotion)0;
		val2.angularZMotion = (ConfigurableJointMotion)0;
		GameObject val3 = new GameObject("Additional Pin Target");
		val3.layer = ((Component)muscle.target).gameObject.layer;
		val3.transform.parent = muscle.target;
		val3.transform.position = val.transform.position;
		val3.transform.rotation = val.transform.rotation;
		muscle.additionalPin = val2;
		muscle.additionalPinTarget = val3.transform;
		muscle.InitiateAdditionalPin();
		return true;
	}

	public bool RemoveAdditionalPin()
	{
		if (!puppetMaster.initiated)
		{
			return false;
		}
		if ((Object)(object)muscle.additionalPin == (Object)null)
		{
			return false;
		}
		Object.Destroy((Object)(object)((Component)muscle.additionalPin).gameObject);
		Object.Destroy((Object)(object)((Component)muscle.additionalPinTarget).gameObject);
		muscle.additionalPin = null;
		muscle.additionalPinTarget = null;
		return true;
	}

	public void OnInitiate()
	{
		muscle.isPropMuscle = true;
		if ((Object)(object)currentProp == (Object)null && (Object)(object)activeProp == (Object)null)
		{
			puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, deactivate: true);
		}
	}

	public void TakeOver()
	{
		currentProp = null;
		lastProp = null;
		activeProp = null;
		puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, deactivate: true);
	}

	public void OnUpdate()
	{
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentProp != (Object)(object)lastProp && !puppetMaster.IsDisconnecting(muscle.index) && !puppetMaster.IsReconnecting(muscle.index))
		{
			if ((Object)(object)lastProp != (Object)null)
			{
				lastProp.Drop(puppetMaster, muscle.index);
				activeProp = null;
				if (OnDropProp != null)
				{
					OnDropProp(lastProp);
				}
			}
			if ((Object)(object)currentProp != (Object)null)
			{
				PropMuscle[] propMuscles = puppetMaster.propMuscles;
				foreach (PropMuscle propMuscle in propMuscles)
				{
					if ((Object)(object)propMuscle != (Object)(object)this && (Object)(object)propMuscle.currentProp == (Object)(object)currentProp)
					{
						propMuscle.TakeOver();
					}
				}
				if (muscle.state.isDisconnected)
				{
					puppetMaster.ReconnectMuscleRecursive(muscle.index);
				}
				currentProp.PickUp(puppetMaster, muscle.index);
				muscle.rigidbody.centerOfMass = currentProp.localCenterOfMass;
				if (currentProp.inertiaTensor != Vector3.zero)
				{
					muscle.rigidbody.inertiaTensor = currentProp.inertiaTensor;
				}
				activeProp = currentProp;
				if (OnPickUpProp != null)
				{
					OnPickUpProp(currentProp);
				}
			}
			else
			{
				puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, deactivate: true);
			}
			lastProp = currentProp;
		}
		if (!((Object)(object)currentProp != (Object)null))
		{
			return;
		}
		muscle.rigidbody.mass = currentProp.mass;
		if ((Object)(object)muscle.additionalPin != (Object)null)
		{
			muscle.additionalPinWeight = currentProp.additionalPinWeight;
			muscle.additionalRigidbody.mass = currentProp.additionalPinMass;
			muscle.additionalRigidbody.drag = muscle.rigidbody.drag;
			muscle.additionalRigidbody.angularDrag = muscle.rigidbody.angularDrag;
			muscle.additionalRigidbody.useGravity = muscle.rigidbody.useGravity;
			muscle.additionalRigidbody.inertiaTensor = Vector3.one * 1E-05f;
			Vector3 val = additionalPinOffset + currentProp.additionalPinOffsetAdd;
			if (lastAdditionalPinOffset != val)
			{
				muscle.additionalPinTarget.localPosition = val;
				((Joint)muscle.additionalPin).connectedAnchor = val;
				lastAdditionalPinOffset = val;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying && !((Object)(object)muscle.target == (Object)null))
		{
			muscle.target.position = ((Component)this).transform.position;
			muscle.target.rotation = ((Component)this).transform.rotation;
			if ((Object)(object)muscle.additionalPin != (Object)null && (Object)(object)muscle.additionalPinTarget != (Object)null)
			{
				((Component)muscle.additionalPin).transform.localPosition = additionalPinOffset;
				((Component)muscle.additionalPin).transform.localRotation = Quaternion.identity;
				muscle.additionalPinTarget.position = ((Component)muscle.additionalPin).transform.position;
				muscle.additionalPinTarget.rotation = ((Component)muscle.additionalPin).transform.rotation;
			}
		}
	}
}
