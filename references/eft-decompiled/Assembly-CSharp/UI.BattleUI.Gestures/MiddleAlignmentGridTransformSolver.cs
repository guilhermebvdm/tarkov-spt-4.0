using System.Collections.Generic;
using UnityEngine;

namespace UI.BattleUI.Gestures;

public class MiddleAlignmentGridTransformSolver : BaseTransformSolver
{
	[SerializeField]
	private Transform _rowTemplate;

	[SerializeField]
	private int _maxItemsInRow = 4;

	private readonly List<Transform> list_0 = new List<Transform>();

	private int int_0;

	public override void SetChild(Transform child)
	{
		Transform parent = method_0();
		child.SetParent(parent);
		int_0++;
	}

	public override void CleanUp()
	{
		foreach (Transform item in list_0)
		{
			Object.Destroy(item);
		}
		list_0.Clear();
		int_0 = 0;
	}

	public Transform method_0()
	{
		if (int_0 < _maxItemsInRow && list_0.Count > 0)
		{
			return list_0[list_0.Count - 1];
		}
		Transform transform = Object.Instantiate(_rowTemplate, base.transform);
		transform.gameObject.SetActive(value: true);
		list_0.Add(transform);
		int_0 = 0;
		return transform;
	}
}
