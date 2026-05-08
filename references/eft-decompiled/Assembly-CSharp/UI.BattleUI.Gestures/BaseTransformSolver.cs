using UnityEngine;

namespace UI.BattleUI.Gestures;

public class BaseTransformSolver : MonoBehaviour
{
	public virtual void SetChild(Transform child)
	{
		if (child.parent != base.transform)
		{
			child.SetParent(base.transform);
		}
	}

	public virtual void CleanUp()
	{
	}
}
