using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[HelpURL("http://root-motion.com/puppetmasterdox/html/page6.html")]
[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Prop Root")]
public class PropRoot : MonoBehaviour
{
	[Tooltip("Reference to the PuppetMaster component.")]
	public PuppetMaster puppetMaster;

	[Tooltip("If a prop is connected, what will its joint be connected to?")]
	public Rigidbody connectTo;

	[Tooltip("Is there a Prop connected to this PropRoot? Simply assign this value to connect, replace or drop props.")]
	public Prop currentProp;

	private Prop lastProp;

	private bool fixedUpdateCalled;

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page6.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_prop_root.html");
	}

	public void DropImmediate()
	{
		if (!((Object)(object)lastProp == (Object)null))
		{
			puppetMaster.RemoveMuscleRecursive(lastProp.muscle, attachTarget: true);
			lastProp.Drop();
			currentProp = null;
			lastProp = null;
		}
	}

	private void Awake()
	{
		if ((Object)(object)currentProp != (Object)null)
		{
			currentProp.StartPickedUp(this);
		}
	}

	private void Update()
	{
		if (fixedUpdateCalled && (Object)(object)currentProp != (Object)null && (Object)(object)lastProp == (Object)(object)currentProp && (Object)(object)((Joint)currentProp.muscle).connectedBody == (Object)null)
		{
			currentProp.Drop();
			currentProp = null;
			lastProp = null;
		}
	}

	private void FixedUpdate()
	{
		fixedUpdateCalled = true;
		if (!((Object)(object)currentProp == (Object)(object)lastProp) && (!((Object)(object)currentProp != (Object)null) || currentProp.initiated))
		{
			if ((Object)(object)currentProp == (Object)null)
			{
				puppetMaster.RemoveMuscleRecursive(lastProp.muscle, attachTarget: true);
				lastProp.Drop();
			}
			if ((Object)(object)lastProp == (Object)null)
			{
				AttachProp(currentProp);
			}
			if ((Object)(object)lastProp != (Object)null && (Object)(object)currentProp != (Object)null)
			{
				puppetMaster.RemoveMuscleRecursive(lastProp.muscle, attachTarget: true);
				AttachProp(currentProp);
			}
			lastProp = currentProp;
		}
	}

	private void AttachProp(Prop prop)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((Component)prop).transform.position = ((Component)this).transform.position;
		((Component)prop).transform.rotation = ((Component)this).transform.rotation;
		prop.PickUp(this);
		puppetMaster.AddMuscle(prop.muscle, ((Component)prop).transform, connectTo, ((Component)this).transform, prop.muscleProps, forceTreeHierarchy: false, prop.forceLayers);
		if ((Object)(object)prop.additionalPin != (Object)null && (Object)(object)prop.additionalPinTarget != (Object)null)
		{
			puppetMaster.AddMuscle(prop.additionalPin, prop.additionalPinTarget, ((Component)prop.muscle).GetComponent<Rigidbody>(), ((Component)prop).transform, new Muscle.Props(prop.additionalPinWeight, 0f, 0f, 0f, Muscle.Group.Prop), forceTreeHierarchy: true, prop.forceLayers);
		}
	}
}
