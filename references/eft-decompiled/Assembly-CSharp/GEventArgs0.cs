using System;
using EFT;

public class GEventArgs0 : EventArgs
{
	public enum ETriggerEventType
	{
		TriggerEnter,
		TriggerExit
	}

	public ETriggerEventType TriggerEventType;

	public IPlayer Player;
}
