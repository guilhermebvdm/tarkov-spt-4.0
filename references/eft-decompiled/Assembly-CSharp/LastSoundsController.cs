using System;
using EFT;
using UnityEngine;

public class LastSoundsController
{
	[NonSerialized]
	public const float DIST_IS_KILLER_SQR = 25f;

	[NonSerialized]
	public const int NeutralSoundsCount = 100;

	[NonSerialized]
	public const int NeutralSoundsCountHalf = 50;

	[NonSerialized]
	public const float OlD_TIME = 5f;

	public GClass582[] NeutralSounds = new GClass582[100];

	[NonSerialized]
	public int LastNeutralSoundIndex;

	[NonSerialized]
	public bool IsCycle;

	[NonSerialized]
	public BotsGroup Group;

	[field: NonSerialized]
	public ZeroGoalTarget ZeroGoalTarget { get; set; }

	public LastSoundsController(BotsGroup group)
	{
		Group = group;
	}

	public void AddNeutralSound(IPlayer player, Vector3 position)
	{
		GClass582 gClass = new GClass582(player, position);
		NeutralSounds[LastNeutralSoundIndex] = gClass;
		LastNeutralSoundIndex++;
		if (!IsCycle && LastNeutralSoundIndex > 50)
		{
			IsCycle = true;
		}
		if (LastNeutralSoundIndex >= 100)
		{
			LastNeutralSoundIndex = 0;
		}
		ZeroGoalTarget = new ZeroGoalTarget();
	}

	public GClass582 GetLastSound()
	{
		int num = 0;
		num = ((LastNeutralSoundIndex != 0) ? (LastNeutralSoundIndex - 1) : 99);
		return NeutralSounds[num];
	}

	public IPlayer PossibleKiller(Vector3 pos, float time)
	{
		bool flag = false;
		int num = LastNeutralSoundIndex - 1;
		if (num <= 0)
		{
			num = 0;
		}
		int num2 = 0;
		while (!flag && num2 < 50)
		{
			if (NeutralSounds.Length > num)
			{
				GClass582 gClass = NeutralSounds[num];
				if (gClass != null)
				{
					if ((flag = Mathf.Abs(time - gClass.Time) > 5f) || (pos - gClass.Position).sqrMagnitude >= 25f)
					{
						num--;
						if (num == 0 && IsCycle)
						{
							num = 99;
						}
						num2++;
						continue;
					}
					return gClass.Player;
				}
				return null;
			}
			return null;
		}
		return null;
	}
}
