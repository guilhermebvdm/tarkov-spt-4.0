using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PhysicsCustomOverlapBoxSimulationTest : MonoBehaviour
{
	public class BoxColliderGizmoDrawer : MonoBehaviour
	{
		public BoxCollider Tester;

		public BoxCollider Collider;

		public bool IsOverlapDetected;

		public bool IsTriggerDetected;

		public bool IsTriggerDetectedPrevFrame;

		public Func<bool> AllowSimulatePhysics;

		public void OnDrawGizmos()
		{
			if (!AllowSimulatePhysics())
			{
				IsTriggerDetected = IsOverlapDetected;
			}
			if (IsOverlapDetected && IsTriggerDetected)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			}
			else if (IsOverlapDetected && !IsTriggerDetected)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
			}
			else if (!IsOverlapDetected && IsTriggerDetected)
			{
				Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
			}
			else
			{
				Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(Collider.center, Collider.size);
			if (AllowSimulatePhysics() && IsTriggerDetectedPrevFrame != IsOverlapDetected)
			{
				Debug.LogError($"Incorrect calculation IsTriggerDetected={IsTriggerDetectedPrevFrame} IsOverlapDetected={IsOverlapDetected}");
			}
			IsTriggerDetectedPrevFrame = IsTriggerDetected;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!(other != Tester))
			{
				IsTriggerDetected = true;
			}
		}

		public void OnTriggerExit(Collider other)
		{
			if (!(other != Tester))
			{
				IsTriggerDetected = false;
			}
		}
	}

	[SerializeField]
	private BoxCollider _tester;

	[SerializeField]
	private LayerMask _layerMask;

	[SerializeField]
	private bool _manualMode;

	[SerializeField]
	private bool _simulateTriggers;

	private BoxCollider[] boxCollider_0;

	private Dictionary<Collider, BoxColliderGizmoDrawer> dictionary_0;

	private BoxColliderGizmoDrawer[] boxColliderGizmoDrawer_0;

	private GClass742 gclass742_0;

	private Collider[] collider_0;

	private Collider[][] collider_1;

	private Transform transform_0;

	private bool bool_0;

	public void Awake()
	{
		Physics.simulationMode = SimulationMode.Script;
		Physics.autoSyncTransforms = false;
		Physics2D.simulationMode = SimulationMode2D.Script;
		Physics2D.autoSyncTransforms = false;
		boxCollider_0 = (from col in UnityEngine.Object.FindObjectsOfType<BoxCollider>()
			where col != _tester
			select col).ToArray();
		gclass742_0 = new GClass742();
		transform_0 = _tester.transform;
	}

	public void Start()
	{
		dictionary_0 = new Dictionary<Collider, BoxColliderGizmoDrawer>();
		boxColliderGizmoDrawer_0 = new BoxColliderGizmoDrawer[boxCollider_0.Length];
		collider_0 = new Collider[boxCollider_0.Length];
		for (int i = 0; i < boxCollider_0.Length; i++)
		{
			BoxCollider boxCollider = boxCollider_0[i];
			gclass742_0.RegisterCollider(boxCollider);
			BoxColliderGizmoDrawer boxColliderGizmoDrawer = boxCollider.gameObject.AddComponent<BoxColliderGizmoDrawer>();
			boxColliderGizmoDrawer.Collider = boxCollider;
			boxColliderGizmoDrawer.Tester = _tester;
			boxColliderGizmoDrawer.AllowSimulatePhysics = () => _simulateTriggers;
			dictionary_0[boxCollider] = boxColliderGizmoDrawer;
			boxColliderGizmoDrawer_0[i] = boxColliderGizmoDrawer;
		}
		collider_1 = new Collider[1][];
		collider_1[0] = new Collider[boxCollider_0.Length];
		Physics.SyncTransforms();
		StartCoroutine(method_0());
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			bool_0 = true;
		}
		if (!_manualMode && _simulateTriggers)
		{
			Physics.Simulate(Time.deltaTime);
		}
	}

	public void OnDestroy()
	{
		gclass742_0.Dispose();
		gclass742_0 = null;
	}

	public IEnumerator method_0()
	{
		EFTPhysicsClass.GClass747.GClass748 gClass = new EFTPhysicsClass.GClass747.GClass748
		{
			Results = collider_0,
			Buffers = collider_1
		};
		while (true)
		{
			if (!_manualMode || bool_0)
			{
				bool_0 = false;
				gClass.Reset();
				gClass.TotalWorldsCount = 1;
				gclass742_0.BoxOverlap(0, transform_0.position, _tester.size / 2f, transform_0.rotation, collider_1[0], _layerMask, QueryTriggerInteraction.Collide, gClass);
				while (!gClass.IsComplete)
				{
					yield return null;
				}
				int count = gClass.Count;
				method_1(count, collider_0);
			}
			else
			{
				yield return null;
			}
		}
	}

	public void method_1(int count, Collider[] result)
	{
		for (int i = 0; i < boxColliderGizmoDrawer_0.Length; i++)
		{
			boxColliderGizmoDrawer_0[i].IsOverlapDetected = false;
		}
		for (int j = 0; j < count; j++)
		{
			dictionary_0[result[j]].IsOverlapDetected = true;
		}
	}

	[CompilerGenerated]
	public bool method_2(BoxCollider col)
	{
		return col != _tester;
	}

	[CompilerGenerated]
	public bool method_3()
	{
		return _simulateTriggers;
	}
}
