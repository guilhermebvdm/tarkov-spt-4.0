using EFT.Vehicle;
using UnityEngine;

public class BTRExhaustSmokeController : MonoBehaviour
{
	private const float float_0 = 100f;

	private readonly Vector2 vector2_0 = new Vector2(0.4f, 0.7f);

	private readonly Vector2 vector2_1 = new Vector2(0.3f, 0.4f);

	[SerializeField]
	private BTRExhaustSmokeVisual[] _exhaustEffects;

	[SerializeField]
	private BTRExhaustSmokeProperties _movingProperties;

	[SerializeField]
	private BTRExhaustSmokeProperties _idleProperties;

	[SerializeField]
	private float _lightModeTimeChange = 5f;

	private BTRView btrview_0;

	private bool bool_0;

	private bool bool_1;

	private float float_1;

	public void Start()
	{
		btrview_0 = BTRControllerClass.Instance.BtrView;
		method_4();
		btrview_0.OnStateChanged += method_2;
	}

	public void OnDestroy()
	{
		if (btrview_0 != null)
		{
			btrview_0.OnStateChanged -= method_2;
		}
	}

	public void Update()
	{
		if (!(btrview_0 == null))
		{
			method_0();
			method_1();
		}
	}

	public void method_0()
	{
		if (!bool_1 && btrview_0.RouteState == EVehicleRouteState.OnRoute)
		{
			method_3();
			bool_1 = true;
		}
		else if (bool_1 && btrview_0.RouteState == EVehicleRouteState.OnDepot)
		{
			method_4();
			bool_1 = false;
		}
	}

	public void method_1()
	{
		if (!bool_0 && btrview_0.CurrentSpeed > 0f)
		{
			float_1 += Time.deltaTime;
			if (float_1 >= _lightModeTimeChange)
			{
				method_4();
				method_6();
				bool_0 = true;
				float_1 = 0f;
			}
			else
			{
				method_5();
			}
		}
	}

	public void method_2(EBtrState state)
	{
		switch (state)
		{
		case EBtrState.Stop:
			method_4();
			break;
		case EBtrState.StopPaid:
			method_4();
			break;
		case EBtrState.Running:
			method_3();
			bool_0 = false;
			break;
		}
	}

	public void method_3()
	{
		BTRExhaustSmokeVisual[] exhaustEffects = _exhaustEffects;
		for (int i = 0; i < exhaustEffects.Length; i++)
		{
			exhaustEffects[i].SetProperties(_movingProperties.LifetimeMinMax, _movingProperties.SizeMinMax, _movingProperties.Rate, _movingProperties.Speed, _movingProperties.Color);
		}
	}

	public void method_4()
	{
		BTRExhaustSmokeVisual[] exhaustEffects = _exhaustEffects;
		for (int i = 0; i < exhaustEffects.Length; i++)
		{
			exhaustEffects[i].SetProperties(_idleProperties.LifetimeMinMax, _idleProperties.SizeMinMax, _idleProperties.Rate, _idleProperties.Speed, _idleProperties.Color);
		}
	}

	public void method_5()
	{
		BTRExhaustSmokeVisual[] exhaustEffects = _exhaustEffects;
		for (int i = 0; i < exhaustEffects.Length; i++)
		{
			exhaustEffects[i].SetProperties(vector2_0, 100f);
		}
	}

	public void method_6()
	{
		BTRExhaustSmokeVisual[] exhaustEffects = _exhaustEffects;
		for (int i = 0; i < exhaustEffects.Length; i++)
		{
			exhaustEffects[i].SetProperties(vector2_1, 100f);
		}
	}
}
