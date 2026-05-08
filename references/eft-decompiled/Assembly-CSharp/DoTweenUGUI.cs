using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DoTweenUGUI : MonoBehaviour
{
	public Image dotweenLogo;

	public Image circleOutline;

	public Text text;

	public Text relativeText;

	public Text scrambledText;

	public Slider slider;

	public void Start()
	{
		dotweenLogo.DOFade(0f, 1.5f).SetAutoKill(autoKillOnCompletion: false).Pause();
		circleOutline.DOColor(method_0(), 1.5f).SetEase(Ease.Linear).Pause();
		circleOutline.DOFillAmount(0f, 1.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo)
			.OnStepComplete(delegate
			{
				circleOutline.fillClockwise = !circleOutline.fillClockwise;
				circleOutline.DOColor(method_0(), 1.5f).SetEase(Ease.Linear);
			})
			.Pause();
		text.DOText("This text will replace the existing one", 2f).SetEase(Ease.Linear).SetAutoKill(autoKillOnCompletion: false)
			.Pause();
		relativeText.DOText(" - This text will be added to the existing one", 2f).SetRelative().SetEase(Ease.Linear)
			.SetAutoKill(autoKillOnCompletion: false)
			.Pause();
		scrambledText.DOText("This text will appear from scrambled chars", 2f, richTextEnabled: true, ScrambleMode.All).SetEase(Ease.Linear).SetAutoKill(autoKillOnCompletion: false)
			.Pause();
		slider.DOValue(1f, 1.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo)
			.Pause();
	}

	public void StartTweens()
	{
		DOTween.PlayAll();
	}

	public void RestartTweens()
	{
		DOTween.RestartAll();
	}

	public Color method_0()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
	}

	[CompilerGenerated]
	public void method_1()
	{
		circleOutline.fillClockwise = !circleOutline.fillClockwise;
		circleOutline.DOColor(method_0(), 1.5f).SetEase(Ease.Linear);
	}
}
