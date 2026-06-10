using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EFT.Hideout;

public class ShrinkingCircleQTE : QTEAction
{
	private const float float_0 = 3f;

	[SerializeField]
	private float _speed = 1f;

	[SerializeField]
	private Vector2 _successRange = new Vector2(0.3f, 0.05f);

	[SerializeField]
	private float _minScale = 0.25f;

	[SerializeField]
	private KeyCode _targetInput;

	[Space(10f)]
	[SerializeField]
	private Image _dynamicCircleImage;

	[SerializeField]
	private Transform _dynamicCircleInnerBorder;

	[SerializeField]
	private Transform _dynamicCircleOuterBorder;

	[Space(10f)]
	[SerializeField]
	private Image _staticCircleImage;

	[SerializeField]
	private Transform _staticCircleInnerBorder;

	[SerializeField]
	private Transform _staticCircleOuterBorder;

	[Space(10f)]
	[SerializeField]
	private Image _staticBackgroundImage;

	[SerializeField]
	private Sprite _staticBackgroundSpriteBad;

	[SerializeField]
	private Sprite _staticBackgroundSpriteGood;

	[Space(10f)]
	[SerializeField]
	private Sprite _spriteBad;

	[SerializeField]
	private Sprite _spriteGood;

	private double double_0;

	private double double_1;

	private bool bool_0;

	private int int_0;

	public override async Task<bool> StartAction(QuickTimeEvent quickTimeEvent)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		_speed = quickTimeEvent.Speed;
		_targetInput = quickTimeEvent.Key;
		_successRange = quickTimeEvent.SuccessRange;
		int_0 = (int)(quickTimeEvent.EndDelay * 1000f);
		bool_0 = false;
		_dynamicCircleInnerBorder.DOScale(_minScale, 3f / _speed).SetEase(Ease.Linear);
		_dynamicCircleOuterBorder.DOScale(_minScale, 3f / _speed).SetEase(Ease.Linear);
		float num = (1f - _minScale) * _successRange.x + _minScale;
		_staticCircleOuterBorder.localScale = Vector3.one * (num + _successRange.y);
		_staticCircleInnerBorder.localScale = Vector3.one * num;
		double_0 = num + _successRange.y;
		double_1 = num;
		return await method_0();
	}

	public async Task<bool> method_0()
	{
		while (true)
		{
			await Task.Yield();
			if (!((double)_dynamicCircleOuterBorder.transform.localScale.x <= double_1))
			{
				if (Input.GetKeyDown(_targetInput))
				{
					if (!method_1())
					{
						await method_3(success: false);
					}
					else
					{
						await method_3(success: true);
					}
					break;
				}
				continue;
			}
			await method_3(success: false);
			break;
		}
		return bool_0;
	}

	public bool method_1()
	{
		float x = _dynamicCircleOuterBorder.transform.localScale.x;
		bool flag = (double)x >= double_1 && (double)x <= double_0;
		return Input.GetKeyDown(_targetInput) && flag;
	}

	public void method_2(bool success)
	{
		if (success)
		{
			_staticCircleImage.sprite = _spriteGood;
			_staticBackgroundImage.sprite = _staticBackgroundSpriteGood;
			_dynamicCircleImage.sprite = _spriteGood;
		}
		else
		{
			_staticCircleImage.sprite = _spriteBad;
			_staticBackgroundImage.sprite = _staticBackgroundSpriteBad;
			_dynamicCircleImage.sprite = _spriteBad;
		}
	}

	public async Task method_3(bool success)
	{
		bool_0 = success;
		method_2(success);
		DOTween.Kill(_dynamicCircleInnerBorder);
		DOTween.Kill(_dynamicCircleOuterBorder);
		await Task.Delay(int_0);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.0.8386' (yours is '10.0.1.8346')
