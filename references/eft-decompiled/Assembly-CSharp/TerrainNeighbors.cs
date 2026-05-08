using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainNeighbors : MonoBehaviour
{
	public Terrain Left;

	public Terrain Right;

	public Terrain Top;

	public Terrain Bottom;

	public void Awake()
	{
		method_0();
	}

	public void OnValidate()
	{
		method_0();
	}

	public void method_0()
	{
		GetComponent<Terrain>().SetNeighbors(Left, Top, Right, Bottom);
	}
}
