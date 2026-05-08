using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotServerControl : MonoBehaviour
{
	private Vector3 vector3_0;

	[CompilerGenerated]
	private BotOwner botOwner_0;

	public BotOwner ControllableBot
	{
		[CompilerGenerated]
		get
		{
			return botOwner_0;
		}
		[CompilerGenerated]
		set
		{
			botOwner_0 = value;
		}
	}

	public void SetSpring(bool sprint)
	{
		ControllableBot.Mover.Sprint(sprint);
	}

	public void SetBotUnderControl(BotOwner bot)
	{
		ControllableBot = bot;
		if (ControllableBot != null)
		{
			ControllableBot.Brain.Deactivate();
		}
	}

	public void Update()
	{
		method_0();
	}

	public void method_0()
	{
		_ = ControllableBot == null;
	}
}
