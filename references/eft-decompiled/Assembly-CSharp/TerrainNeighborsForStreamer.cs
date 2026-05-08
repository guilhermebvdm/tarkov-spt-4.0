using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainNeighborsForStreamer : MonoBehaviour
{
	public int X;

	public int Y;

	public int ArrayWidth;

	public int ArrayHeight;

	[CompilerGenerated]
	private Terrain terrain_0;

	[CompilerGenerated]
	private Terrain terrain_1;

	[CompilerGenerated]
	private Terrain terrain_2;

	[CompilerGenerated]
	private Terrain terrain_3;

	private Terrain terrain_4;

	private static TerrainNeighborsForStreamer[,] terrainNeighborsForStreamer_0;

	public Terrain Left
	{
		[CompilerGenerated]
		get
		{
			return terrain_0;
		}
		[CompilerGenerated]
		set
		{
			terrain_0 = value;
		}
	}

	public Terrain Right
	{
		[CompilerGenerated]
		get
		{
			return terrain_1;
		}
		[CompilerGenerated]
		set
		{
			terrain_1 = value;
		}
	}

	public Terrain Top
	{
		[CompilerGenerated]
		get
		{
			return terrain_2;
		}
		[CompilerGenerated]
		set
		{
			terrain_2 = value;
		}
	}

	public Terrain Bottom
	{
		[CompilerGenerated]
		get
		{
			return terrain_3;
		}
		[CompilerGenerated]
		set
		{
			terrain_3 = value;
		}
	}

	public void Awake()
	{
		if (terrainNeighborsForStreamer_0 == null)
		{
			terrainNeighborsForStreamer_0 = new TerrainNeighborsForStreamer[ArrayWidth, ArrayHeight];
		}
		terrain_4 = GetComponent<Terrain>();
		method_0(remove: false);
	}

	public void OnDestroy()
	{
		method_0(remove: true);
	}

	public void method_0(bool remove)
	{
		terrainNeighborsForStreamer_0[X, Y] = (remove ? null : this);
		TerrainNeighborsForStreamer terrainNeighborsForStreamer = method_1(X - 1, Y);
		TerrainNeighborsForStreamer terrainNeighborsForStreamer2 = method_1(X + 1, Y);
		TerrainNeighborsForStreamer terrainNeighborsForStreamer3 = method_1(X, Y + 1);
		TerrainNeighborsForStreamer terrainNeighborsForStreamer4 = method_1(X, Y - 1);
		Terrain terrain = (remove ? null : terrain_4);
		if (terrainNeighborsForStreamer != null)
		{
			terrainNeighborsForStreamer.Right = terrain;
			terrainNeighborsForStreamer.UpdateNeighbors();
			Left = terrainNeighborsForStreamer.terrain_4;
		}
		if (terrainNeighborsForStreamer2 != null)
		{
			terrainNeighborsForStreamer2.Left = terrain;
			terrainNeighborsForStreamer2.UpdateNeighbors();
			Right = terrainNeighborsForStreamer2.terrain_4;
		}
		if (terrainNeighborsForStreamer3 != null)
		{
			terrainNeighborsForStreamer3.Bottom = terrain;
			terrainNeighborsForStreamer3.UpdateNeighbors();
			Top = terrainNeighborsForStreamer3.terrain_4;
		}
		if (terrainNeighborsForStreamer4 != null)
		{
			terrainNeighborsForStreamer4.Top = terrain;
			terrainNeighborsForStreamer4.UpdateNeighbors();
			Bottom = terrainNeighborsForStreamer4.terrain_4;
		}
		if (!remove)
		{
			UpdateNeighbors();
		}
	}

	public TerrainNeighborsForStreamer method_1(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < ArrayWidth && y < ArrayHeight)
		{
			return terrainNeighborsForStreamer_0[x, y];
		}
		return null;
	}

	public void UpdateNeighbors()
	{
		terrain_4.SetNeighbors(Left, Top, Right, Bottom);
	}
}
