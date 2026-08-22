using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public abstract class Prop : MonoBehaviour
{
	[Tooltip("This has no other purpose but helping you distinguish props by PropRoot.currentProp.propType.")]
	public int propType;

	[LargeHeader("Muscle")]
	[Tooltip("The Muscle of this prop.")]
	public ConfigurableJoint muscle;

	[Tooltip("The muscle properties that will be applied to the Muscle on pickup.")]
	public Muscle.Props muscleProps = new Muscle.Props();

	[Tooltip("If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.")]
	public bool forceLayers = true;

	[LargeHeader("Additional Pin")]
	[Tooltip("Optinal additional pin, useful for long melee weapons that would otherwise require a lot of muscle force and solver iterations to be swinged quickly. Should normally be without any colliders attached. The position of the pin, its mass and the pin weight will effect how the prop will handle.")]
	public ConfigurableJoint additionalPin;

	[Tooltip("Target Transform for the additional pin.")]
	public Transform additionalPinTarget;

	[Tooltip("The pin weight of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.")]
	[Range(0f, 1f)]
	public float additionalPinWeight = 1f;

	[LargeHeader("Materials")]
	[Tooltip("If assigned, sets prop colliders to this PhysicMaterial when picked up.")]
	public PhysicMaterial pickedUpMaterial;

	[Tooltip("If assigned, sets prop colliders to this PhysicMaterial when dropped.")]
	public PhysicMaterial droppedMaterial;

	private ConfigurableJointMotion xMotion;

	private ConfigurableJointMotion yMotion;

	private ConfigurableJointMotion zMotion;

	private ConfigurableJointMotion angularXMotion;

	private ConfigurableJointMotion angularYMotion;

	private ConfigurableJointMotion angularZMotion;

	private Collider[] colliders = (Collider[])(object)new Collider[0];

	public bool isPickedUp => (Object)(object)propRoot != (Object)null;

	public PropRoot propRoot { get; private set; }

	public bool initiated { get; private set; }

	public void PickUp(PropRoot propRoot)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		muscle.xMotion = xMotion;
		muscle.yMotion = yMotion;
		muscle.zMotion = zMotion;
		muscle.angularXMotion = angularXMotion;
		muscle.angularYMotion = angularYMotion;
		muscle.angularZMotion = angularZMotion;
		this.propRoot = propRoot;
		((Component)muscle).gameObject.layer = ((Component)propRoot.puppetMaster).gameObject.layer;
		Collider[] array = colliders;
		foreach (Collider val in array)
		{
			if (!val.isTrigger)
			{
				((Component)val).gameObject.layer = ((Component)muscle).gameObject.layer;
			}
		}
		SetMaterial(pickedUpMaterial);
		OnPickUp(propRoot);
	}

	public void Drop()
	{
		propRoot = null;
		SetMaterial(droppedMaterial);
		OnDrop();
	}

	public void StartPickedUp(PropRoot propRoot)
	{
		this.propRoot = propRoot;
	}

	protected virtual void OnPickUp(PropRoot propRoot)
	{
	}

	protected virtual void OnDrop()
	{
	}

	protected virtual void OnStart()
	{
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.position != ((Component)muscle).transform.position)
		{
		}
		xMotion = muscle.xMotion;
		yMotion = muscle.yMotion;
		zMotion = muscle.zMotion;
		angularXMotion = muscle.angularXMotion;
		angularYMotion = muscle.angularYMotion;
		angularZMotion = muscle.angularZMotion;
		colliders = ((Component)muscle).GetComponentsInChildren<Collider>();
		if (!isPickedUp)
		{
			ReleaseJoint();
		}
		OnStart();
		initiated = true;
	}

	private void ReleaseJoint()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		((Joint)muscle).connectedBody = null;
		muscle.targetRotation = Quaternion.identity;
		JointDrive slerpDrive = default(JointDrive);
		slerpDrive.positionSpring = 0f;
		muscle.slerpDrive = slerpDrive;
		muscle.xMotion = (ConfigurableJointMotion)2;
		muscle.yMotion = (ConfigurableJointMotion)2;
		muscle.zMotion = (ConfigurableJointMotion)2;
		muscle.angularXMotion = (ConfigurableJointMotion)2;
		muscle.angularYMotion = (ConfigurableJointMotion)2;
		muscle.angularZMotion = (ConfigurableJointMotion)2;
	}

	private void SetMaterial(PhysicMaterial material)
	{
		if (!((Object)(object)material == (Object)null))
		{
			Collider[] array = colliders;
			foreach (Collider val in array)
			{
				val.sharedMaterial = material;
			}
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)muscle == (Object)null) && !Application.isPlaying)
		{
			((Component)this).transform.position = ((Component)muscle).transform.position;
			((Component)this).transform.rotation = ((Component)muscle).transform.rotation;
			if ((Object)(object)additionalPinTarget != (Object)null && (Object)(object)additionalPin != (Object)null)
			{
				additionalPinTarget.position = ((Component)additionalPin).transform.position;
			}
			muscleProps.group = Muscle.Group.Prop;
		}
	}
}
