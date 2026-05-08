using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass426
{
	public IPlayer Requester;

	public EInteraction? GestureByRequester;

	public BotOwner Executer;

	public AIRequestStage Executing;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass425 Gclass425_0;

	public GClass425 Answer
	{
		[CompilerGenerated]
		get
		{
			return Gclass425_0;
		}
	}

	public bool ShallDelete
	{
		get
		{
			AIRequestStage executing = Executing;
			if (executing != AIRequestStage.none && (uint)(executing - 1) <= 1u)
			{
				return Time.time > Float_1;
			}
			return Time.time > Float_0;
		}
	}

	public GClass426(IPlayer requester, EInteraction? gesture, GClass425 answer)
	{
		Requester = requester;
		GestureByRequester = gesture;
		Gclass425_0 = answer;
		Float_0 = Time.time + LocalBotSettingsProviderClass.Core.GESTUS_REQUEST_LIFETIME;
	}

	public void StartFirstExecuting()
	{
		Executing = AIRequestStage.lookAt;
		Float_1 = Time.time + LocalBotSettingsProviderClass.Core.GESTUS_FIRST_STAGE_MAX_TIME;
	}

	public void StartSecondExecuting()
	{
		Executing = AIRequestStage.gestus;
		Float_1 = Time.time + LocalBotSettingsProviderClass.Core.GESTUS_SECOND_STAGE_MAX_TIME;
	}
}
