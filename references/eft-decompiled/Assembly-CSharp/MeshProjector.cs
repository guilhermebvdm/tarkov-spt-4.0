using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class MeshProjector : MonoBehaviour
{
	private LayerMask layerMask_0 = -1;

	[SerializeField]
	[Range(0f, 1f)]
	private float _offset = 0.05f;

	[SerializeField]
	private float _raycastOffset = 10f;

	[HideInInspector]
	public bool UpdateByTimer;

	[HideInInspector]
	public float UpdateTime = 2f;

	private Transform transform_0;

	private MeshFilter meshFilter_0;

	private const float float_0 = 50f;

	private float float_1;

	public static Action<int> OnMeshUpdated;
}
