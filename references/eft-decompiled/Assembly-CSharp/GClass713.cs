using NLog;
using UnityEngine;

public class GClass713 : LoggerClass
{
	public GClass713(LoggerMode mode)
		: base("fast-animator-controller", mode)
	{
	}

	public void LogTriggInfo(CurrentStateAbstractClass currentState, float currentNormTime, float absoluteStartFrameTime, float absoluteEndFrameTime)
	{
		if (IsEnabled(LogLevel.Trace))
		{
			float motionDuration = currentState.MotionDuration;
			float num = ((currentState.SpeedAnimParamClass != null) ? currentState.SpeedAnimParamClass.GetFloatValue() : 1f);
			object[] obj = new object[4]
			{
				"<b>[{0}]</b> Triggered info {1}",
				LogLevel.Debug,
				Time.frameCount,
				null
			};
			string[] obj2 = new string[15]
			{
				currentState?.ToString(),
				" motDuration ",
				motionDuration.ToString(),
				" speedMult ",
				num.ToString(),
				" speedMult2 ",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			float float_ = currentState.Float_0;
			obj2[6] = float_.ToString();
			obj2[7] = " currentNormTime ";
			obj2[8] = currentNormTime.ToString();
			obj2[9] = " absStartFrameTime ";
			obj2[10] = absoluteStartFrameTime.ToString();
			obj2[11] = " absEndFrameTime ";
			obj2[12] = absoluteEndFrameTime.ToString();
			obj2[13] = " real motion duration ";
			obj2[14] = currentState.Real.ToString();
			obj[3] = string.Concat(obj2);
			LogTrace("[{0}] Triggered info: {1}", obj);
		}
	}

	public void LogTriggeredTransition(TransitionClass triggeredTransition)
	{
		if (IsEnabled(LogLevel.Trace))
		{
			LogTrace("[{0}] Triggered transition: {1}", "<b>[{0}]</b> Triggered transition: {1}", LogLevel.Debug, Time.frameCount, triggeredTransition);
		}
	}

	public void LogNotUsed(string notUsedFeature, IAnimator animator)
	{
		LogDebug("{0} - not used by FastAC, {1}", notUsedFeature, animator);
	}

	public void LogLayerGetByNameWarning(string layerName, IAnimator animator)
	{
		if (IsEnabled(LogLevel.Warn))
		{
			string text = string.Empty;
			for (int i = 0; i < animator.layerCount; i++)
			{
				text = text + animator.GetLayerName(i) + ", ";
			}
			Log("[{0}][{1}] Attempting to get not existed layer '{2}' index. Available layers: [{3}]", "<color=red>[{0}][{1}] Attempting to get not existed layer '{2}' index. Available layers: [{3}]</color>", LogLevel.Warn, Time.frameCount, animator.name, layerName, text);
		}
	}

	public void LogErrorOnSetLayerWeight(int layerIndex, float weight, IAnimator animator)
	{
		LogLevel warn = LogLevel.Warn;
		Log(warn, "[{0}][{1}] Attempting to set layer '{2}' weight: {3}. Layers count: {4}", Time.frameCount, animator.name, layerIndex, weight, animator.layerCount);
	}

	public void LogErrorOnGetLayerWeight(int layerIndex, IAnimator animator)
	{
		LogLevel warn = LogLevel.Warn;
		Log(warn, "[{0}][{1}] Attempting to get layer '{2}'. Layers count: {3}", Time.frameCount, animator.name, layerIndex, animator.layerCount);
	}

	public void LogGetLayerNameWarning(int layerIndex, IAnimator animator)
	{
		Log("[{0}][{1}] Attempting to get not existed layer '{2}' name. Layers count: [{3}]", "<color=red>[{0}][{1}] Attempting to get not existed layer '{2}' name. Layers count: [{3}]</color>", LogLevel.Warn, Time.frameCount, animator.name, layerIndex, animator.layerCount);
	}
}
