using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Interactive;
using UnityEngine;

public class ThrowGrenadePlace : MonoBehaviour, IPositionPoint
{
	[CompilerGenerated]
	public class Class295
	{
		public Vector3 pos;

		public Door method_0(Door x1, Door x2)
		{
			if ((x1.transform.position - pos).sqrMagnitude > (x2.transform.position - pos).sqrMagnitude)
			{
				return x2;
			}
			return x1;
		}
	}

	public float GrenadeMass = 0.6f;

	public float GrenadeForce = 11f;

	public Transform From;

	public Transform Target;

	public float AngleForThrow = 45f;

	public float ThrowForce = 45f;

	public Door Door;

	public bool IsOk;

	public Vector3 DoorPos;

	public bool HaveDoor;

	private readonly Vector3 vector3_0 = new Vector3(0f, 1.5f, 0f);

	public Vector3 Position => From.position;

	public AIGreanageThrowData GetData()
	{
		return new AIGreanageThrowData(AngleForThrow, ThrowForce, Target.position - From.position, From.position, Target.position, alwaysGood: true);
	}

	public void Init(IEnumerable<Door> allDoors)
	{
		if (HaveDoor)
		{
			Vector3 pos = DoorPos;
			Door door = allDoors.Aggregate((Door x1, Door x2) => ((x1.transform.position - pos).sqrMagnitude > (x2.transform.position - pos).sqrMagnitude) ? x2 : x1);
			if ((DoorPos - door.transform.position).sqrMagnitude > 1f)
			{
				GClass666.AILog("AI Place info can't find door", door);
			}
			Door = door;
		}
	}

	public bool IsValid(string id, bool withSubDoor)
	{
		if (withSubDoor)
		{
			Door = null;
			HaveDoor = false;
		}
		if (Target.position == Vector3.zero)
		{
			Debug.LogError("Target is ZERO! Id:" + id);
			return false;
		}
		if (From.position == Vector3.zero)
		{
			Debug.LogError("Position is ZERO!Id:" + id);
			return false;
		}
		if (!GClass855.IsOnNavMesh(From.position, 0.1f))
		{
			if (!Physics.Raycast(From.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 2f, LayerMaskClass.TerrainLowPoly))
			{
				string text = "no parent";
				if (base.transform.parent != null)
				{
					text = base.transform.parent.name;
				}
				Debug.LogError("AHTUNG! Position not on NAV MESH! " + From?.ToString() + "Id:" + id + " by:" + base.gameObject.name + "   par: " + text);
				return false;
			}
			From.position = hitInfo.point;
		}
		Debug.Log("GrenadeForce: " + GrenadeForce + "    GrenadeMass:" + GrenadeMass);
		float maxPower = GrenadeForce / GrenadeMass;
		AIGreanageThrowData aIGreanageThrowData = null;
		List<AIGreanageThrowData> list = new List<AIGreanageThrowData>();
		foreach (AIGreandeAng value in Enum.GetValues(typeof(AIGreandeAng)))
		{
			AIGreanageThrowData aIGreanageThrowData2 = GClass577.CanThrowGrenade2(From.position + vector3_0, Target.position, maxPower, value);
			if (!aIGreanageThrowData2.CanThrow)
			{
				list.Add(aIGreanageThrowData2);
				continue;
			}
			aIGreanageThrowData = aIGreanageThrowData2;
			break;
		}
		if (aIGreanageThrowData == null)
		{
			foreach (AIGreanageThrowData item in list)
			{
				if (withSubDoor)
				{
					Door door = method_0(item);
					if (door != null)
					{
						Door = door;
						Debug.LogWarning("DOOR hit data:   CanThrow:" + item.Ang);
						break;
					}
				}
			}
			HaveDoor = Door != null;
			if (HaveDoor)
			{
				DoorPos = Door.transform.position;
			}
			return false;
		}
		IsOk = true;
		AngleForThrow = aIGreanageThrowData.Ang;
		ThrowForce = aIGreanageThrowData.Force;
		return true;
	}

	public void OnDrawGizmosSelected()
	{
		DrawGizmos();
	}

	public void DrawGizmos()
	{
		Color color = new Color(0.5f, 0.9f, 0.1f);
		Gizmos.color = (IsOk ? color : Color.red);
		Gizmos.DrawSphere(From.position, 0.5f);
		Gizmos.color = (IsOk ? color : Color.red);
		Gizmos.DrawSphere(From.position + vector3_0, 0.3f);
		Gizmos.color = (IsOk ? new Color(0.1f, 0.8f, 0.5f) : Color.red);
		Gizmos.DrawSphere(Target.position, 0.5f);
		Vector3 d = Target.position - From.position;
		if (d.sqrMagnitude > 0.1f)
		{
			Vector3 position = From.position + vector3_0;
			Vector3 vector = GClass855.RotateVectorOnAngToZ(d, AngleForThrow);
			GClass810.DrawArrow(position, vector * 2f);
		}
	}

	public bool IsNowValid()
	{
		if (!IsOk)
		{
			return false;
		}
		if (!HaveDoor)
		{
			return true;
		}
		return Door.DoorState == EDoorState.Open;
	}

	public Door method_0(AIGreanageThrowData aiGreandeOutData)
	{
		GameObject hitsObject = aiGreandeOutData.HitsObject;
		if (hitsObject == null)
		{
			return null;
		}
		Door component = hitsObject.GetComponent<Door>();
		Debug.Log("DOOR   0out hit data:" + hitsObject?.ToString() + "   CanThrow:" + aiGreandeOutData.CanThrow);
		if (component != null)
		{
			return component;
		}
		Transform parent = hitsObject.transform.parent;
		if (parent != null)
		{
			component = parent.GetComponent<Door>();
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}
}
