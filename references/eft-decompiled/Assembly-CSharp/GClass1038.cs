using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public abstract class GClass1038
{
	[CompilerGenerated]
	public class Class723
	{
		public Light target;

		public float method_0()
		{
			return target.range;
		}

		public void method_1(float x)
		{
			target.range = x;
		}
	}

	public static TweenerCore<float, float, FloatOptions> DORange(this Light target, float endValue, float duration)
	{
		TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(() => target.range, delegate(float x)
		{
			target.range = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static Tween FadeSwitchImages(Tween target, Image currentImage, Image newImage, float duration = 0.5f, float startAlpha = 0f, float endAlpha = 1f, bool forced = false)
	{
		if (target != null && target.IsActive())
		{
			target.Complete();
		}
		if (forced)
		{
			duration = 0f;
		}
		Color color = newImage.color;
		newImage.color = new Color(color.r, color.g, color.b, startAlpha);
		newImage.gameObject.SetActive(value: true);
		return DOTween.Sequence().Append(currentImage.DOFade(startAlpha, duration).From(endAlpha)).Join(newImage.DOFade(endAlpha, duration).From(startAlpha));
	}
}
