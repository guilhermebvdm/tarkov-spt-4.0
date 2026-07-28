using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class PuppetMasterProp : MonoBehaviour
{
	[Tooltip("Mesh Root will be parented to Prop Muscle's target when this prop is picked up. To make sure the mesh and the colliders match up, Mesh Root's localPosition/Rotation must be zero.")]
	public Transform meshRoot;

	[Tooltip("The muscle properties that will be applied to the Prop Muscle when this prop is picked up.")]
	public Muscle.Props muscleProps;

	[Tooltip("If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.")]
	public bool forceLayers = true;

	[Tooltip("Mass of the prop while picked up. When dropped, mass of the original Rigidbody will be used.")]
	public float mass = 1f;

	[Tooltip("This has no other purpose but helping you distinguish props by PropMuscle.currentProp.propType.")]
	public int propType;

	[LargeHeader("Materials")]
	[Tooltip("If assigned, sets prop colliders to this PhysicMaterial when picked up. If no material assigned, will maintain the original PhysicMaterial (unless otherwise controlled by BehaviourPuppet's Group Overrides).")]
	public PhysicMaterial pickedUpMaterial;

	[LargeHeader("Additional Pin")]
	[Tooltip("Adds this to Prop Muscle's 'Additional Pin Offset' when this prop is picked up.")]
	public Vector3 additionalPinOffsetAdd;

	[Tooltip("The pin weight multiplier of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.")]
	[Range(0f, 1f)]
	public float additionalPinWeight = 1f;

	[Tooltip("Multiplies the mass of the additional pin by this value when this prop is picked up. The Rigidbody on this prop will be destroyed on pick-up and reattached on drop, so its mass is not used while picked up.")]
	public float additionalPinMass = 1f;

	private int defaultLayer;

	private Transform defaultParent;

	private Collider[] colliders = (Collider[])(object)new Collider[0];

	private PhysicMaterial[] droppedMaterials = (PhysicMaterial[])(object)new PhysicMaterial[0];

	private Rigidbody r;

	private float _mass;

	private float _drag;

	private float _angularDrag;

	private bool _useGravity;

	private bool _isKinematic;

	private RigidbodyInterpolation _interpolation;

	private CollisionDetectionMode _collisionDetectionMode;

	private RigidbodyConstraints _constraints;

	private Collider[] emptyColliders = (Collider[])(object)new Collider[0];

	public bool isPickedUp { get; private set; }

	public Vector3 inertiaTensor { get; private set; }

	public Vector3 localCenterOfMass { get; private set; }

	protected Muscle propMuscle { get; private set; }

	public Rigidbody GetRigidbody()
	{
		if ((Object)(object)r != (Object)null)
		{
			return r;
		}
		if (isPickedUp)
		{
			return propMuscle.rigidbody;
		}
		return null;
	}

	protected virtual void OnPickUp(PuppetMaster puppetMaster, int propMuscleIndex)
	{
	}

	protected virtual void OnDrop(PuppetMaster puppetMaster, int propMuscleIndex)
	{
	}

	public void PickUp(PuppetMaster puppetMaster, int propMuscleIndex)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		RemoveRigidbody();
		((Component)this).transform.parent = puppetMaster.muscles[propMuscleIndex].transform;
		((Component)this).transform.position = puppetMaster.muscles[propMuscleIndex].transform.position;
		((Component)this).transform.rotation = puppetMaster.muscles[propMuscleIndex].transform.rotation;
		meshRoot.parent = puppetMaster.muscles[propMuscleIndex].target;
		meshRoot.localPosition = Vector3.zero;
		meshRoot.localRotation = Quaternion.identity;
		puppetMaster.muscles[propMuscleIndex].props = muscleProps;
		if ((Object)(object)pickedUpMaterial != (Object)null)
		{
			Collider[] array = colliders;
			foreach (Collider val in array)
			{
				val.sharedMaterial = pickedUpMaterial;
			}
		}
		if (forceLayers)
		{
			Collider[] array2 = colliders;
			foreach (Collider val2 in array2)
			{
				if (!val2.isTrigger)
				{
					((Component)val2).gameObject.layer = ((Component)puppetMaster.muscles[propMuscleIndex].joint).gameObject.layer;
				}
			}
		}
		puppetMaster.muscles[propMuscleIndex].colliders = colliders;
		puppetMaster.UpdateInternalCollisions(puppetMaster.muscles[propMuscleIndex]);
		isPickedUp = true;
		propMuscle = puppetMaster.muscles[propMuscleIndex];
		OnPickUp(puppetMaster, propMuscleIndex);
	}

	public void Drop(PuppetMaster puppetMaster, int propMuscleIndex)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)puppetMaster.muscles[propMuscleIndex].joint).gameObject.activeInHierarchy)
		{
			((Component)this).transform.position = puppetMaster.muscles[propMuscleIndex].target.position;
			((Component)this).transform.rotation = puppetMaster.muscles[propMuscleIndex].target.rotation;
		}
		ReattachRigidbody();
		if (!((Component)puppetMaster.muscles[propMuscleIndex].joint).gameObject.activeInHierarchy || puppetMaster.muscles[propMuscleIndex].rigidbody.isKinematic)
		{
			r.velocity = puppetMaster.muscles[propMuscleIndex].mappedVelocity;
			r.angularVelocity = puppetMaster.muscles[propMuscleIndex].mappedAngularVelocity;
		}
		((Component)this).transform.parent = defaultParent;
		meshRoot.parent = ((Component)this).transform;
		meshRoot.localPosition = Vector3.zero;
		meshRoot.localRotation = Quaternion.identity;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].sharedMaterial = droppedMaterials[i];
		}
		puppetMaster.ResetInternalCollisions(puppetMaster.muscles[propMuscleIndex], useInternalCollisionIgnores: false);
		puppetMaster.muscles[propMuscleIndex].colliders = emptyColliders;
		if (forceLayers)
		{
			Collider[] array = colliders;
			foreach (Collider val in array)
			{
				if (!val.isTrigger)
				{
					((Component)val).gameObject.layer = defaultLayer;
				}
			}
		}
		isPickedUp = false;
		propMuscle = null;
		OnDrop(puppetMaster, propMuscleIndex);
	}

	protected virtual void Awake()
	{
		r = ((Component)this).GetComponent<Rigidbody>();
		defaultParent = ((Component)this).transform.parent;
		colliders = ((Component)this).GetComponentsInChildren<Collider>();
		droppedMaterials = (PhysicMaterial[])(object)new PhysicMaterial[colliders.Length];
		for (int i = 0; i < colliders.Length; i++)
		{
			droppedMaterials[i] = colliders[i].sharedMaterial;
		}
	}

	protected virtual void Start()
	{
		muscleProps.group = Muscle.Group.Prop;
		if ((Object)(object)meshRoot == (Object)null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		if ((Object)(object)meshRoot == (Object)(object)((Component)this).transform)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		defaultLayer = ((Component)this).gameObject.layer;
		Collider[] array = colliders;
		foreach (Collider val in array)
		{
			if (!val.isTrigger)
			{
				defaultLayer = ((Component)val).gameObject.layer;
				break;
			}
		}
	}

	protected virtual void Update()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (isPickedUp)
		{
			((Component)this).transform.localPosition = Vector3.zero;
			((Component)this).transform.localRotation = Quaternion.identity;
		}
	}

	private void RemoveRigidbody()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)r == (Object)null))
		{
			inertiaTensor = r.inertiaTensor;
			localCenterOfMass = r.centerOfMass;
			_mass = r.mass;
			_drag = r.drag;
			_angularDrag = r.angularDrag;
			_useGravity = r.useGravity;
			_isKinematic = r.isKinematic;
			_interpolation = r.interpolation;
			_collisionDetectionMode = r.collisionDetectionMode;
			_constraints = r.constraints;
			Object.Destroy((Object)(object)r);
		}
	}

	private void ReattachRigidbody()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)r != (Object)null))
		{
			r = ((Component)this).gameObject.AddComponent<Rigidbody>();
			r.mass = _mass;
			r.drag = _drag;
			r.angularDrag = _angularDrag;
			r.useGravity = _useGravity;
			r.isKinematic = _isKinematic;
			r.interpolation = _interpolation;
			r.collisionDetectionMode = _collisionDetectionMode;
			r.constraints = _constraints;
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			if (muscleProps != null)
			{
				muscleProps.group = Muscle.Group.Prop;
			}
			if ((Object)(object)meshRoot != (Object)null && (Object)(object)meshRoot != (Object)(object)((Component)this).transform)
			{
				meshRoot.parent = ((Component)this).transform;
				meshRoot.position = ((Component)this).transform.position;
				meshRoot.rotation = ((Component)this).transform.rotation;
			}
		}
	}
}
