using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class PuppetControllerLite : MonoBehaviour, ICollisionEventListener
{
	[Serializable]
	public class Group
	{
		public string name;

		[Tooltip("The muscle groups to apply this pinWeightMlp and muscleWeightMlp to.")]
		public int[] indices = new int[0];

		[Range(0f, 1f)]
		public float pinWeightMlp = 0.5f;

		[Range(0f, 1f)]
		public float muscleWeightMlp = 0.5f;

		[Tooltip("When the puppet is touched, sets muscle Rigidbody drag to this value to reduce the rubber chicken effect.")]
		public float drag = 2f;

		[Tooltip("The time of blending in this script's effects when the puppet is touched.")]
		public float blendInTime = 0.05f;

		[Tooltip("The time of blending out this script's effects when the puppet is not touched any more.")]
		public float blendOutTime = 1f;

		private float dam = 0f;

		private float damTime = -100f;

		private float damV;

		private float map;

		private float mapV;

		public bool enabled { get; private set; }

		public float mappingWeight { get; private set; }

		public void TryDamage(Collision collision, CollisionEventBroadcaster broadcaster)
		{
			bool flag = false;
			for (int i = 0; i < indices.Length; i++)
			{
				if (broadcaster.muscle.index == indices[i])
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				damTime = Time.time;
				enabled = true;
			}
		}

		public void Update(PuppetMasterLite puppetMaster)
		{
			if (enabled)
			{
				bool flag = puppetMaster.pinWeight <= 0f;
				float num = ((Time.time > damTime + 0.2f) ? 0f : 1f);
				if (flag)
				{
					num = 1f;
				}
				float num2 = num;
				if (flag)
				{
					num2 = 1f;
				}
				float num3 = ((num > dam) ? blendInTime : blendOutTime);
				dam = Mathf.SmoothDamp(dam, num, ref damV, num3);
				if (num < dam && dam < 0.001f)
				{
					dam = 0f;
				}
				float num4 = ((num2 > map) ? blendInTime : blendOutTime);
				map = Mathf.SmoothDamp(map, num2, ref mapV, num4);
				if (num2 < map && map < 0.001f)
				{
					map = 0f;
				}
				if (flag)
				{
					dam = Mathf.Min(dam, map);
				}
				float num5 = (flag ? 0f : (drag * map));
				float angularDrag = (flag ? 0.05f : (drag * map));
				mappingWeight = Mathf.Lerp(0f, 1f, map);
				for (int i = 0; i < indices.Length; i++)
				{
					int num6 = indices[i];
					puppetMaster.muscles[num6].pinWeightMlp = Mathf.Lerp(1f, pinWeightMlp, dam);
					puppetMaster.muscles[num6].muscleWeightMlp = Mathf.Lerp(1f, muscleWeightMlp, dam);
					puppetMaster.muscles[num6].rigidbody.drag = num5;
					puppetMaster.muscles[num6].rigidbody.angularDrag = angularDrag;
				}
				if (dam <= 0f && map < 0f)
				{
					enabled = false;
				}
			}
		}
	}

	public PuppetMasterLite puppetMaster;

	public LayerMask collisionLayers;

	[Tooltip("When the puppet is touched, sets pin weight and muscle weight values for these groups.")]
	public Group[] groups = new Group[0];

	private void Start()
	{
		MuscleLite[] muscles = puppetMaster.muscles;
		foreach (MuscleLite muscleLite in muscles)
		{
			CollisionEventBroadcaster collisionEventBroadcaster = ((Component)muscleLite.joint).gameObject.AddComponent<CollisionEventBroadcaster>();
			collisionEventBroadcaster.listener = this;
			collisionEventBroadcaster.muscle = muscleLite;
		}
	}

	private bool NeedToUpdate()
	{
		Group[] array = groups;
		foreach (Group group in array)
		{
			if (group.enabled)
			{
				return true;
			}
		}
		return false;
	}

	private void FixedUpdate()
	{
		if (NeedToUpdate())
		{
			float num = 0f;
			Group[] array = groups;
			foreach (Group group in array)
			{
				group.Update(puppetMaster);
				num = Mathf.Max(num, group.mappingWeight);
			}
			MuscleLite[] muscles = puppetMaster.muscles;
			foreach (MuscleLite muscleLite in muscles)
			{
				muscleLite.mappingWeightMlp = num;
			}
		}
	}

	public void OnCollisionEnterEvent(Collision collision, CollisionEventBroadcaster broadcaster)
	{
		ProcessCollisionEvent(collision, broadcaster);
	}

	public void OnCollisionStayEvent(Collision collision, CollisionEventBroadcaster broadcaster)
	{
		ProcessCollisionEvent(collision, broadcaster);
	}

	private void ProcessCollisionEvent(Collision collision, CollisionEventBroadcaster broadcaster)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Component)collision.collider).transform.root == (Object)(object)((Component)this).transform) && LayerMaskExtensions.Contains(collisionLayers, ((Component)collision.collider).gameObject.layer))
		{
			Group[] array = groups;
			foreach (Group group in array)
			{
				group.TryDamage(collision, broadcaster);
			}
		}
	}

	public void OnCollisionExitEvent(Collision collision, CollisionEventBroadcaster broadcaster)
	{
	}
}
