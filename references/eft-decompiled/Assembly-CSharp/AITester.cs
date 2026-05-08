using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AITester : MonoBehaviour
{
	[CompilerGenerated]
	public class Class147
	{
		public EAITester id;

		public bool method_0(AITester x)
		{
			if (x.gameObject.activeInHierarchy)
			{
				return x.Id == id;
			}
			return false;
		}
	}

	public EAITester Id;

	private NavGraphVoxelSimple navGraphVoxelSimple_0;

	public Vector3 Position => base.transform.position;

	public static AITester GetTester(EAITester id)
	{
		AITester[] array = GClass870.FindUnityObjectsOfType<AITester>();
		if (array != null && array.Length != 0)
		{
			return array.FirstOrDefault((AITester x) => x.gameObject.activeInHierarchy && x.Id == id);
		}
		return null;
	}

	public void OnDrawGizmosSelected()
	{
		if (navGraphVoxelSimple_0 != null && Id == EAITester.FindVoxel)
		{
			Vector3 size = navGraphVoxelSimple_0.GetSize();
			float num = 1f;
			Gizmos.DrawWireCube(navGraphVoxelSimple_0.Position + size * 0.5f, size * num);
			Gizmos.DrawWireCube(navGraphVoxelSimple_0.Position, Vector3.one * 0.01f);
		}
	}

	public void DebugDrawSet(NavGraphVoxelSimple voxel)
	{
		navGraphVoxelSimple_0 = voxel;
	}
}
