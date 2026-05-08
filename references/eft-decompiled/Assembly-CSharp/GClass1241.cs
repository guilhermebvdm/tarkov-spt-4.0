using Koenigz.PerfectCulling;
using UnityEngine;

public abstract class GClass1241
{
	public static (MeshRenderer[], GameObject) Create(float height, float centerSize, float branchLength, float branchSize, int numBranches)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "TreeProxy";
		Vector3 vector = new Vector3(0f, height, 0f);
		GameObject gameObject2 = smethod_1(height, centerSize);
		gameObject2.transform.Translate(vector * 0.5f, Space.Self);
		gameObject2.transform.SetParent(gameObject.transform);
		for (int i = 0; i < numBranches; i++)
		{
			smethod_0(gameObject.transform, branchLength, branchSize, vector);
			vector += -Vector3.up * branchSize * 3f;
		}
		return (gameObject.GetComponentsInChildren<MeshRenderer>(), gameObject);
	}

	public static void smethod_0(Transform root, float len, float size, Vector3 localPosition)
	{
		GameObject gameObject = smethod_1(len, size, first: false);
		gameObject.transform.position = localPosition;
		gameObject.transform.up = Vector3.forward;
		GameObject gameObject2 = smethod_1(len, size, first: true, second: false);
		gameObject2.transform.position = localPosition;
		gameObject2.transform.up = Vector3.right;
		gameObject.transform.SetParent(root, worldPositionStays: true);
		gameObject2.transform.SetParent(root, worldPositionStays: true);
	}

	public static GameObject smethod_1(float height, float size, bool first = true, bool second = true)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		if (first)
		{
			GameObject gameObject2 = smethod_2();
			gameObject2.transform.localScale = new Vector3(size, height, 1f);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: true);
		}
		if (second)
		{
			GameObject gameObject3 = smethod_2();
			gameObject3.transform.localScale = new Vector3(size, height, 1f);
			gameObject3.transform.Rotate(Vector3.up, 90f, Space.Self);
			gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: true);
		}
		return gameObject;
	}

	public static GameObject smethod_2()
	{
		GameObject gameObject = new GameObject();
		GameObject gameObject2 = Object.Instantiate(PerfectCullingResourcesLocator.Instance.QuadPrefab);
		GameObject gameObject3 = Object.Instantiate(PerfectCullingResourcesLocator.Instance.QuadPrefab);
		gameObject3.transform.forward = -gameObject3.transform.forward;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: true);
		gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: true);
		return gameObject;
	}
}
