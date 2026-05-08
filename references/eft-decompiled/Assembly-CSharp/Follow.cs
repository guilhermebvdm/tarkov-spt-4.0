using DG.Tweening;
using UnityEngine;

public class Follow : MonoBehaviour
{
	public Transform target;

	private Vector3 vector3_0;

	private Tweener tweener_0;

	public void Start()
	{
		tweener_0 = base.transform.DOMove(target.position, 2f).SetAutoKill(autoKillOnCompletion: false);
		vector3_0 = target.position;
	}

	public void Update()
	{
		if (!(vector3_0 == target.position))
		{
			tweener_0.ChangeEndValue(target.position, snapStartValue: true).Restart();
			vector3_0 = target.position;
		}
	}
}
