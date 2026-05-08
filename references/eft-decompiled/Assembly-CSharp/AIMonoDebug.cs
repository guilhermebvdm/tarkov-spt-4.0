using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class AIMonoDebug : MonoBehaviour
{
	private BotsController botsController_0;

	[CompilerGenerated]
	private bool bool_0;

	public BotZoneGroupsDictionary ZoneGroups => botsController_0.BotSpawner.Groups;

	public bool IsInited
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Init(BotsController bController)
	{
		IsInited = true;
		botsController_0 = bController;
	}
}
