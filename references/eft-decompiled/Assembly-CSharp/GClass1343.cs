using System;

public class GClass1343
{
	[NonSerialized]
	public FastAnimatorControllerClass FastAnimatorControllerClass;

	[NonSerialized]
	public GClass1344[] Gclass1344_0;

	public GClass1343(FastAnimatorControllerClass fastAnimatorController)
	{
		FastAnimatorControllerClass = fastAnimatorController;
		Gclass1344_0 = new GClass1344[FastAnimatorControllerClass.LayersCount];
		for (int i = 0; i < FastAnimatorControllerClass.LayersCount; i++)
		{
			CurrentStateAbstractClass currentState = FastAnimatorControllerClass.GetLayerStateMachine(i).FindDefaultState();
			Gclass1344_0[i] = new GClass1344
			{
				CurrentState = currentState,
				CurrentStateAbsoluteTime = 0f
			};
		}
	}

	public GClass1344 GetStateInfo(int layerIndex)
	{
		return Gclass1344_0[layerIndex];
	}
}
