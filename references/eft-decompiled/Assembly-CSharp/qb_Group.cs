using UnityEngine;

public class qb_Group : MonoBehaviour
{
	public string groupName;

	private bool bool_0;

	private bool bool_1;

	public void AddObject(GameObject newObject)
	{
		newObject.transform.parent = base.transform;
	}

	public void Hide()
	{
		bool_0 = false;
	}

	public void Show()
	{
		bool_0 = true;
	}

	public void Freeze()
	{
		bool_1 = true;
	}

	public void UnFreeze()
	{
		bool_1 = false;
	}

	public void CleanUp()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
