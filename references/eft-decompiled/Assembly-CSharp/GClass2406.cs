using System;
using System.Collections.Generic;
using EFT.InputSystem;

public abstract class GClass2406
{
	[NonSerialized]
	public static HashSet<EGameKey> HashSet_0 = new HashSet<EGameKey>
	{
		EGameKey.Shoot,
		EGameKey.Nidnod,
		EGameKey.PushToTalk,
		EGameKey.Interact,
		EGameKey.BlindShootAbove,
		EGameKey.BlindShootRight,
		EGameKey.MakeScreenshot,
		EGameKey.ThrowItem,
		EGameKey.HighThrow,
		EGameKey.LowThrow,
		EGameKey.ToggleVoip,
		EGameKey.LeftStance,
		EGameKey.Tactical,
		EGameKey.ThrowGrenade
	};

	[NonSerialized]
	public static Dictionary<EGameKey, EPressType> Dictionary_0 = new Dictionary<EGameKey, EPressType>
	{
		{
			EGameKey.Vaulting,
			EPressType.Continuous
		},
		{
			EGameKey.Tactical,
			EPressType.Press
		},
		{
			EGameKey.ThrowGrenade,
			EPressType.Press
		}
	};

	[NonSerialized]
	public static Dictionary<ECommand, ECommand> Dictionary_1 = new Dictionary<ECommand, ECommand>(GClass866<ECommand>.EqualityComparer)
	{
		{
			ECommand.DoubleF1,
			ECommand.F1
		},
		{
			ECommand.DoubleF2,
			ECommand.F2
		},
		{
			ECommand.DoubleF3,
			ECommand.F3
		},
		{
			ECommand.DoubleF4,
			ECommand.F4
		},
		{
			ECommand.DoubleF5,
			ECommand.F5
		},
		{
			ECommand.DoubleF6,
			ECommand.F6
		},
		{
			ECommand.DoubleF7,
			ECommand.F7
		},
		{
			ECommand.DoubleF8,
			ECommand.F8
		},
		{
			ECommand.DoubleF9,
			ECommand.F9
		},
		{
			ECommand.DoubleF10,
			ECommand.F10
		},
		{
			ECommand.DoubleF11,
			ECommand.F11
		},
		{
			ECommand.DoubleF12,
			ECommand.F12
		}
	};

	public static EPressType GetSpecificPressType(this EGameKey gameKey)
	{
		return Dictionary_0[gameKey];
	}

	public static bool IsPressTypeSpecific(this EGameKey gameKey)
	{
		return Dictionary_0.ContainsKey(gameKey);
	}

	public static bool IsPressTypeSelectionAllowed(this EGameKey gameKey)
	{
		return !HashSet_0.Contains(gameKey);
	}

	public static bool TryGetAssociatedCommand(this ECommand command, out ECommand associatedCommand)
	{
		return Dictionary_1.TryGetValue(command, out associatedCommand);
	}

	public static bool IsCommand(this ECommand command, ECommand checkCommand)
	{
		return command == checkCommand;
	}

	public static bool IsCommand(this ECommand command, params ECommand[] checkCommands)
	{
		int num = 0;
		while (true)
		{
			if (num < checkCommands.Length)
			{
				ECommand checkCommand = checkCommands[num];
				if (IsCommand(command, checkCommand))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}
}
