using DG.Tweening;
using UnityEngine;

public class Materials : MonoBehaviour
{
	public GameObject target;

	public Color toColor;

	private Tween tween_0;

	private Tween tween_1;

	private Tween tween_2;

	public void Start()
	{
		Material material = target.GetComponent<Renderer>().material;
		tween_0 = material.DOColor(toColor, 1f).SetLoops(-1, LoopType.Yoyo).Pause();
		tween_1 = material.DOColor(new Color(0f, 0f, 0f, 0f), "_EmissionColor", 1f).SetLoops(-1, LoopType.Yoyo).Pause();
		tween_2 = material.DOOffset(new Vector2(1f, 1f), 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental)
			.Pause();
	}

	public void ToggleColor()
	{
		tween_0.TogglePause();
	}

	public void ToggleEmission()
	{
		tween_1.TogglePause();
	}

	public void ToggleOffset()
	{
		tween_2.TogglePause();
	}
}
