using System;
using System.Threading;
using System.Threading.Tasks;
using EFT.Weather;
using UnityEngine;

public class WinterEventVisual : MonoBehaviour, GInterface49
{
	public WeatherObstacle WinterWeatherObstacle;

	[Range(0f, 1f)]
	public float WIND_MAX = 1f;

	[Range(0f, 1f)]
	public float CLOUDS_MAX = 0.3f;

	[Range(0f, 0.255f)]
	public float FOG_MAX = 0.025f;

	[Range(0f, 1f)]
	public float SNOW_FLAKES_MAX = 1f;

	[Range(0f, 1f)]
	public float SNOWY_MAX = 1f;

	public Vector2 Common = new Vector2(0f, 120f);

	public Vector2 Wind = new Vector2(0f, 30f);

	public Vector2 Clouds = new Vector2(10f, 30f);

	public Vector2 Fog = new Vector2(20f, 40f);

	public Vector2 SnowFlakes = new Vector2(30f, 50f);

	public Vector2 Snowy = new Vector2(30f, 60f);

	public Vector2 Opaqueness = new Vector2(90f, 120f);

	public float SnowFlakesSideSpeedIncrementSeconds = 5f;

	public float SnowFlakesSideSpeedMaximum = 0.25f;

	private Task task_0;

	private CancellationTokenSource cancellationTokenSource_0;

	public static LoggerClass LoggerClass => Class443.Logger;

	WeatherObstacle GInterface49.WinterWeatherObstacle => WinterWeatherObstacle;

	public float Single_0 => Common.x;

	public float Single_1 => Common.y;

	public void Start()
	{
		GInterface29 controller = Class443.Controller;
		if (controller != null)
		{
			controller.Register(this);
		}
		else
		{
			Class443.OnInitialized += method_0;
		}
	}

	public void OnDestroy()
	{
		cancellationTokenSource_0?.Cancel();
		Class443.OnInitialized -= method_0;
		Class443.Controller?.Unregister(this);
	}

	public void method_0(GInterface29 controller)
	{
		Class443.OnInitialized -= method_0;
		controller.Register(this);
	}

	public void StormSetup()
	{
		LoggerClass.LogTrace("StormSetup()");
		if (cancellationTokenSource_0 != null)
		{
			LoggerClass.LogError("Storm already started");
			return;
		}
		WeatherDebug weather = smethod_0();
		cancellationTokenSource_0 = new CancellationTokenSource();
		task_0 = method_2(weather, cancellationTokenSource_0.Token);
		task_0.HandleExceptions();
	}

	public void StormSetupOnReconnect()
	{
		LoggerClass.LogTrace("StormSetupOnReconnect()");
		if (cancellationTokenSource_0 != null)
		{
			LoggerClass.LogError("Storm already started");
			return;
		}
		WeatherDebug weather = smethod_0();
		method_1(weather);
	}

	public static WeatherDebug smethod_0()
	{
		WeatherController instance = WeatherController.Instance;
		WeatherDebug weatherDebug = instance.WeatherDebug;
		weatherDebug.CopyParams(instance.WeatherCurve);
		weatherDebug.Rain = 0f;
		weatherDebug.Enabled = true;
		return weatherDebug;
	}

	public void method_1(WeatherDebug weather)
	{
		LoggerClass.LogTrace("StormSetupOnReconnect begin");
		weather.WindMagnitude = WIND_MAX;
		weather.CloudDensity = CLOUDS_MAX;
		weather.Fog = FOG_MAX;
		weather.Rain = SNOWY_MAX;
		weather.Enabled = true;
		LoggerClass.LogTrace("StormSetupOnReconnect complete");
	}

	public async Task method_2(WeatherDebug weather, CancellationToken token)
	{
		LoggerClass.LogTrace($"StormSetup delay:{Common.x}");
		if (Common.x > 0f)
		{
			await Task.Delay(TimeSpan.FromSeconds(Common.x), token);
		}
		RainController rainController = WeatherController.Instance.RainController;
		LoggerClass.LogTrace("StormSetup begin");
		Task task = method_3(weather, token);
		Task task2 = method_4(weather, token);
		Task task3 = method_5(weather, token);
		Task task4 = method_6(weather, token);
		Task task5 = smethod_1(rainController, SnowFlakes.x, SnowFlakesSideSpeedIncrementSeconds, SnowFlakesSideSpeedMaximum, token);
		Task task6 = method_7(rainController, token);
		Task task7 = method_8(rainController, token);
		await Task.WhenAll(task, task2, task3, task4, task5, task6, task7);
		LoggerClass.LogTrace("StormSetup compelete");
	}

	public async Task method_3(WeatherDebug weather, CancellationToken token)
	{
		float x = Wind.x;
		float num = Wind.y - Wind.x;
		LoggerClass.LogTrace($"WindSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("WindSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = weather.WindMagnitude;
		LoggerClass.LogTrace($"WindSetup begin incrementSeconds:{num} windMagnitude:{num2}");
		while (num2 < WIND_MAX)
		{
			LoggerClass.LogTrace($"windMagnitude:{num2}");
			await Task.Yield();
			if (!token.IsCancellationRequested)
			{
				float num3 = Time.time - time;
				time = Time.time;
				num2 += num3 / num;
				if (num2 > WIND_MAX)
				{
					break;
				}
				weather.WindMagnitude = num2;
				continue;
			}
			return;
		}
		if (!token.IsCancellationRequested)
		{
			weather.WindMagnitude = WIND_MAX;
		}
		LoggerClass.LogTrace($"WindSetup complete weather.WindMagnitude:{weather.WindMagnitude}");
	}

	public async Task method_4(WeatherDebug weather, CancellationToken token)
	{
		float x = Clouds.x;
		float num = Clouds.y - Clouds.x;
		LoggerClass.LogTrace($"CloudsSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("CloudsSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = weather.CloudDensity;
		LoggerClass.LogTrace($"CloudsSetup begin incrementSeconds:{num} cloudDensity:{num2}");
		while (num2 < CLOUDS_MAX)
		{
			LoggerClass.LogTrace($"cloudDensity:{num2}");
			await Task.Yield();
			if (!token.IsCancellationRequested)
			{
				float num3 = Time.time - time;
				time = Time.time;
				num2 += 2f * num3 / num;
				if (num2 > CLOUDS_MAX)
				{
					break;
				}
				weather.CloudDensity = num2;
				continue;
			}
			return;
		}
		if (!token.IsCancellationRequested)
		{
			weather.CloudDensity = CLOUDS_MAX;
		}
		LoggerClass.LogTrace($"CloudsSetup complete weather.CloudDensity:{weather.CloudDensity}");
	}

	public async Task method_5(WeatherDebug weather, CancellationToken token)
	{
		float x = Fog.x;
		float num = Fog.y - Fog.x;
		LoggerClass.LogTrace($"FogSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("FogSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = weather.Fog;
		LoggerClass.LogTrace($"FogSetup begin incrementSeconds:{num} fog:{num2}");
		while (num2 < FOG_MAX)
		{
			LoggerClass.LogTrace($"fog:{num2}");
			await Task.Yield();
			if (!token.IsCancellationRequested)
			{
				float num3 = Time.time - time;
				time = Time.time;
				num2 += num3 * FOG_MAX / num;
				if (num2 > FOG_MAX)
				{
					break;
				}
				weather.Fog = num2;
				continue;
			}
			return;
		}
		if (!token.IsCancellationRequested)
		{
			weather.Fog = FOG_MAX;
		}
		LoggerClass.LogTrace($"FogSetup complete weather.Fog:{weather.Fog}");
	}

	public async Task method_6(WeatherDebug weather, CancellationToken token)
	{
		float x = SnowFlakes.x;
		float num = SnowFlakes.y - SnowFlakes.x;
		LoggerClass.LogTrace($"SnowFlakesSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("SnowFlakesSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = weather.Rain;
		LoggerClass.LogTrace($"SnowFlakesSetup begin incrementSeconds:{num} snowFlakes:{num2}");
		while (num2 < SNOW_FLAKES_MAX)
		{
			LoggerClass.LogTrace($"snowFlakes:{num2}");
			await Task.Yield();
			if (!token.IsCancellationRequested)
			{
				float num3 = Time.time - time;
				time = Time.time;
				num2 += num3 / num;
				if (num2 > SNOW_FLAKES_MAX)
				{
					break;
				}
				weather.Rain = num2;
				continue;
			}
			return;
		}
		if (!token.IsCancellationRequested)
		{
			weather.Rain = SNOW_FLAKES_MAX;
		}
		LoggerClass.LogTrace($"SnowFlakesSetup complete weather.Rain:{weather.Rain}");
	}

	public static async Task smethod_1(RainController rainController, float delaySeconds, float incrementSeconds, float maximum, CancellationToken token)
	{
		LoggerClass.LogTrace($"SnowFlakesSideSpeedSetup delaySeconds:{delaySeconds}");
		if (delaySeconds < 0f)
		{
			LoggerClass.LogInfo("SnowFlakesSideSpeedSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
		float time = Time.time;
		float num = rainController.StormSideSpeed;
		LoggerClass.LogTrace($"SnowFlakesSideSpeedSetup begin incrementSeconds:{incrementSeconds} maximum:{maximum} sideSpeed:{num}");
		while (true)
		{
			if (num < maximum)
			{
				LoggerClass.LogTrace($"sideSpeed:{num}");
				await Task.Yield();
				if (!token.IsCancellationRequested)
				{
					float num2 = Time.time - time;
					time = Time.time;
					num = (rainController.StormSideSpeed = num + maximum * num2 / incrementSeconds);
					continue;
				}
				break;
			}
			if (!token.IsCancellationRequested)
			{
				rainController.StormSideSpeed = maximum;
			}
			LoggerClass.LogTrace($"SnowFlakesSideSpeedSetup complete RainController.StormSideSpeed:{rainController.StormSideSpeed}");
			break;
		}
	}

	public async Task method_7(RainController rainController, CancellationToken token)
	{
		float x = Snowy.x;
		float num = Snowy.y - Snowy.x;
		LoggerClass.LogTrace($"WettingSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("WettingSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = 0f;
		LoggerClass.LogTrace($"WettingSetup begin incrementSeconds:{num} wetting:{num2}");
		while (true)
		{
			if (num2 < SNOWY_MAX)
			{
				LoggerClass.LogTrace($"wetting:{num2}");
				await Task.Yield();
				if (!token.IsCancellationRequested)
				{
					float num3 = Time.time - time;
					time = Time.time;
					num2 += num3 / num;
					rainController.method_10(num2);
					continue;
				}
				break;
			}
			if (!token.IsCancellationRequested)
			{
				rainController.method_10(SNOWY_MAX);
			}
			LoggerClass.LogTrace($"WettingSetup complete RainController.Wetting:{RainController.Wetting}");
			break;
		}
	}

	public async Task method_8(RainController rainController, CancellationToken token)
	{
		float x = Opaqueness.x;
		float num = Opaqueness.y - Opaqueness.x;
		LoggerClass.LogTrace($"WettingSetup delaySeconds:{x}");
		if (x < 0f)
		{
			LoggerClass.LogInfo("WettingSetup skipped");
			return;
		}
		await Task.Delay(TimeSpan.FromSeconds(x), token);
		float time = Time.time;
		float num2 = 0f;
		LoggerClass.LogTrace($"WettingSetup begin incrementSeconds:{num} wetting:{num2}");
		while (true)
		{
			if (num2 < 1f)
			{
				LoggerClass.LogTrace($"opaqueness:{num2}");
				await Task.Yield();
				if (!token.IsCancellationRequested)
				{
					float num3 = Time.time - time;
					time = Time.time;
					num2 += num3 / num;
					rainController.method_13(num2);
					continue;
				}
				break;
			}
			if (!token.IsCancellationRequested)
			{
				rainController.method_13(1f);
			}
			LoggerClass.LogTrace($"WettingSetup complete RainController.Wetting:{RainController.Wetting}");
			break;
		}
	}
}
