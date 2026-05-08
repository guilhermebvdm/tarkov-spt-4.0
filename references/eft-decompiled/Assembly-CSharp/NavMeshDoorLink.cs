using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshDoorLink : MonoBehaviour
{
	private const float float_0 = 9999f;

	public int Id;

	public string DoorId;

	public BotOwner Opening;

	public Vector3 Close1;

	public Vector3 Close2_Normal;

	public Vector3 Close2_Breach;

	public Vector3 FarestClosePoint;

	public Vector3 Open1;

	public Vector3 Open2;

	public float BottomY;

	public Vector3 MidOpen;

	public Vector3 MidClose;

	public NavMeshObstacle Carver_Opened;

	public NavMeshObstacle Carver_Breached;

	public NavMeshObstacle Carver_Closed;

	public bool ShallTryInteract;

	public int ConnectedNavGraphPathOpenStateId;

	public int ConnectedNavGraphPathCloseStateId;

	public NavGraphEditorPath ConnectedNavGraphOpenStatePath;

	public NavGraphEditorPath ConnectedNavGraphCloseStatePath;

	private GClass365 gclass365_0;

	private GClass365 gclass365_1;

	private BotsController botsController_0;

	private bool bool_0;

	private float float_1;

	private bool bool_1;

	[CompilerGenerated]
	private float float_2;

	[CompilerGenerated]
	private float float_3;

	[CompilerGenerated]
	private Door door_0;

	public float LastOpenByBotTime
	{
		[CompilerGenerated]
		get
		{
			return float_2;
		}
		[CompilerGenerated]
		set
		{
			float_2 = value;
		}
	}

	public float LastInteractByBotTime
	{
		[CompilerGenerated]
		get
		{
			return float_3;
		}
		[CompilerGenerated]
		set
		{
			float_3 = value;
		}
	}

	public Door Door
	{
		[CompilerGenerated]
		get
		{
			return door_0;
		}
		[CompilerGenerated]
		set
		{
			door_0 = value;
		}
	}

	public GClass365 SegmentOpen
	{
		get
		{
			if (gclass365_0 == null)
			{
				gclass365_0 = new GClass365(Open1, Open2);
			}
			return gclass365_0;
		}
	}

	public GClass365 SegmentClose
	{
		get
		{
			if (gclass365_1 == null)
			{
				gclass365_1 = new GClass365(Close1, Close2_Normal);
			}
			return gclass365_1;
		}
	}

	public void Awake()
	{
		MidOpen = (Open1 + Open2) / 2f;
		MidClose = (Close1 + Close2_Normal) / 2f;
	}

	public void SetDoor(Door door, bool withSubscribe)
	{
		Door = door;
		if (Door != null)
		{
			DoorId = Door.Id;
		}
		if (withSubscribe && ShallTryInteract)
		{
			if (door.DoorState == EDoorState.Open)
			{
				NavMeshObstacle carver_Breached = Carver_Breached;
				Carver_Opened.carving = true;
				carver_Breached.carving = true;
			}
			else
			{
				NavMeshObstacle carver_Breached2 = Carver_Breached;
				Carver_Opened.carving = false;
				carver_Breached2.carving = false;
			}
			Door.OnDoorStateChanged += method_0;
		}
	}

	public void Init(BotsController bc)
	{
		botsController_0 = bc;
	}

	public void ManualUpdate()
	{
		if (bool_0 && !Carver_Opened.carving && !Carver_Breached.carving)
		{
			method_1();
		}
	}

	public bool ShallInteract()
	{
		if (!ShallTryInteract)
		{
			return true;
		}
		if (!Carver_Opened.carving)
		{
			return !Carver_Breached.carving;
		}
		return false;
	}

	public void TryCreateCrave()
	{
		Vector3 position = (Open1 + Open2) / 2f;
		Vector3 vector = Open1 - Open2;
		Vector3 vector2 = Close2_Normal - Close1;
		Vector3 vector3 = Close2_Breach - Close1;
		Vector3 position2 = (Close1 + Close2_Normal) / 2f;
		Vector3 position3 = (Close1 + Close2_Breach) / 2f;
		GameObject obj = new GameObject("Carver_Opened");
		GameObject gameObject = new GameObject("Carver_Breached");
		GameObject gameObject2 = new GameObject("Carver_Closed");
		NavMeshObstacle navMeshObstacle = obj.AddComponent<NavMeshObstacle>();
		NavMeshObstacle navMeshObstacle2 = gameObject.AddComponent<NavMeshObstacle>();
		NavMeshObstacle navMeshObstacle3 = gameObject2.AddComponent<NavMeshObstacle>();
		obj.transform.SetParent(base.transform);
		gameObject.transform.SetParent(base.transform);
		gameObject2.transform.SetParent(base.transform);
		float y = 0f;
		vector2.y = 0f;
		vector3.y = y;
		float num = Vector3.Angle(vector2, Vector3.right);
		float y2 = Vector3.Angle(vector, Vector3.right);
		if (vector2.z < 0f)
		{
			num = 360f - num;
		}
		num = 0f - num;
		float num2 = Vector3.Angle(vector3, Vector3.right);
		if (vector3.z < 0f)
		{
			num2 = 360f - num2;
		}
		num2 = 0f - num2;
		obj.transform.rotation = Quaternion.Euler(new Vector3(0f, num, 0f));
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, num2, 0f));
		gameObject2.transform.rotation = Quaternion.Euler(new Vector3(0f, y2, 0f));
		GClass810.DebugArrow(Close1, vector2, Color.red, 2f);
		navMeshObstacle.size = new Vector3(vector2.magnitude, 1f, 0.1f);
		navMeshObstacle2.size = new Vector3(vector3.magnitude, 1f, 0.1f);
		navMeshObstacle3.size = new Vector3(vector.magnitude, 1f, 0.1f);
		obj.transform.position = position2;
		gameObject.transform.position = position3;
		gameObject2.transform.position = position;
		Carver_Opened = navMeshObstacle;
		Carver_Breached = navMeshObstacle2;
		Carver_Closed = navMeshObstacle3;
	}

	public void CheckAfterCreatedCarver()
	{
		Vector3 rhs = Close2_Normal - Close1;
		Vector3 vector = Open2 - Open1;
		Vector3 vector2 = GClass855.Rotate90(vector, GClass855.SideTurn.right);
		if (vector2.sqrMagnitude <= 0f)
		{
			Debug.LogError("Door have strange collider:" + base.gameObject.name);
			return;
		}
		Vector3 vector3 = ((!(Vector3.Dot(vector2, rhs) > 0f)) ? GClass855.NormalizeFastSelf(-vector2) : GClass855.NormalizeFastSelf(vector2));
		Vector3 sourcePosition = Close1 + vector + vector3 * 0.4f;
		Vector3 sourcePosition2 = Close1 - vector + vector3 * 0.4f;
		CarveOff();
		if (!NavMesh.SamplePosition(sourcePosition, out var hit, 2f, -1) || !NavMesh.SamplePosition(sourcePosition2, out var hit2, 2f, -1))
		{
			return;
		}
		GClass810.DebugPoint(hit.position, Color.yellow, 0.5f, 2f);
		GClass810.DebugPoint(hit2.position, Color.green, 0.5f, 2f);
		float magnitude = (hit2.position - hit.position).magnitude;
		NavMeshPath navMeshPath = new NavMeshPath();
		if (!NavMesh.CalculatePath(hit.position, hit2.position, -1, navMeshPath))
		{
			return;
		}
		if (navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			float num = Mathf.Abs(GClass371.CalculatePathLength(navMeshPath) - magnitude);
			if (num < 4f)
			{
				if ((Carver_Opened != null) & (Carver_Breached != null))
				{
					NavMeshObstacle carver_Opened = Carver_Opened;
					Carver_Breached.enabled = true;
					carver_Opened.enabled = true;
				}
			}
			else
			{
				Debug.LogWarning("doot hav path too long " + num + "   " + Door.gameObject.name + "   id:" + Door.Id);
			}
		}
		else
		{
			Debug.LogWarning("doot hav path status " + navMeshPath.status.ToString() + "   " + Door.gameObject.name + "   id:" + Door.Id);
		}
	}

	public void CarveOff()
	{
		if (Carver_Opened != null && Carver_Breached != null && Carver_Closed != null)
		{
			NavMeshObstacle carver_Breached = Carver_Breached;
			NavMeshObstacle carver_Closed = Carver_Closed;
			Carver_Opened.carving = false;
			carver_Closed.carving = false;
			carver_Breached.carving = false;
		}
	}

	public void SetAllCarvers(bool val)
	{
		Carver_Opened.carving = val;
		Carver_Breached.carving = val;
		Carver_Closed.carving = val;
		Carver_Opened.enabled = val;
		Carver_Breached.enabled = val;
		Carver_Closed.enabled = val;
	}

	public void SetCoef()
	{
		bool_1 = true;
		if (Door.DoorState == EDoorState.Open)
		{
			ConnectedNavGraphOpenStatePath?.SetCoef(9999f);
		}
		if (Door.DoorState == EDoorState.Shut)
		{
			ConnectedNavGraphCloseStatePath?.SetCoef(9999f);
		}
	}

	public void DropCoef()
	{
		bool_1 = false;
		ConnectedNavGraphOpenStatePath?.DropCoef(9999f);
		ConnectedNavGraphCloseStatePath?.DropCoef(9999f);
	}

	public void method_0(WorldInteractiveObject obj, EDoorState prevstate, EDoorState nextstate)
	{
		if (ShallTryInteract)
		{
			if (nextstate == EDoorState.Open)
			{
				bool_0 = true;
				method_1();
				return;
			}
			bool_0 = false;
			NavMeshObstacle carver_Breached = Carver_Breached;
			Carver_Opened.carving = false;
			carver_Breached.carving = false;
		}
	}

	public void method_1()
	{
		if (!(float_1 < Time.time))
		{
			return;
		}
		float_1 = Time.time + 1f;
		BotOwner botOwner = botsController_0.ClosestBotToPoint(MidOpen);
		if (botOwner != null)
		{
			if ((botOwner.Position - MidOpen).sqrMagnitude > 4f)
			{
				method_2();
			}
		}
		else
		{
			method_2();
		}
	}

	public void method_2()
	{
		bool_0 = false;
		NavMeshObstacle carver_Breached = Carver_Breached;
		Carver_Opened.carving = true;
		carver_Breached.carving = true;
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Vector3 vector = Open1 + Vector3.up;
		Vector3 to = Open2 + Vector3.up;
		Gizmos.DrawLine(Open1, vector);
		Gizmos.DrawLine(Open2, to);
		Gizmos.DrawLine(vector, to);
		Gizmos.color = Color.green;
		Vector3 vector2 = Close1 + Vector3.up;
		Vector3 to2 = Close2_Normal + Vector3.up;
		Vector3 to3 = Close2_Breach + Vector3.up;
		Gizmos.DrawLine(Close1, vector2);
		Gizmos.DrawLine(Close2_Normal, to2);
		Gizmos.DrawLine(Close2_Breach, to3);
		Gizmos.DrawLine(vector2, to2);
		Gizmos.DrawLine(vector2, to3);
	}
}
