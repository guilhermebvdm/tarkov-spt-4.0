using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PaintingTarget : MonoBehaviour
{
	public bool ApplyVertexColors;

	public Mesh SharedMesh;

	public Mesh TargetMesh;

	public List<Color> VerticesColors;

	public float GizmosScale = 0.2f;

	[HideInInspector]
	public Vector4 ChannelScale = new Vector4(1f, 1f, 1f, 1f);

	[HideInInspector]
	public bool ShowVertices;

	private Color color_0 = new Color32(0, 0, 0, byte.MaxValue);

	private List<int> list_0 = new List<int>();

	public void Awake()
	{
	}
}
