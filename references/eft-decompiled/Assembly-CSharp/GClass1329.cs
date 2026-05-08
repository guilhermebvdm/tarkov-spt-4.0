using UnityEngine;

public class GClass1329 : GClass1328
{
	public string Name;

	public override void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		base.OnStateEnter(fastAnimatorProcessor, in layerInfo, layerIndex);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateEnter [nt {2}]", Time.frameCount, Name, layerInfo.normalizedTime);
	}

	public override void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		base.OnStateExit(fastAnimatorProcessor, in layerInfo, layerIndex);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateExit [nt {2}]", Time.frameCount, Name, layerInfo.normalizedTime);
	}
}
