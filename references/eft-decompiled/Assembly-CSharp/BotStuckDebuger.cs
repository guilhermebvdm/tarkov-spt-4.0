using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotStuckDebuger : MonoBehaviour
{
	private Dictionary<int, Vector3> dictionary_0 = new Dictionary<int, Vector3>();

	private Dictionary<int, float> dictionary_1 = new Dictionary<int, float>();

	private Dictionary<int, int> dictionary_2 = new Dictionary<int, int>();

	private IBotGame ibotGame_0;

	private float float_0;

	public Dictionary<int, float> Collected => dictionary_1;

	public void Awake()
	{
	}

	public void Update()
	{
		if (ibotGame_0 == null && Singleton<IBotGame>.Instance != null)
		{
			ibotGame_0 = Singleton<IBotGame>.Instance;
		}
		else
		{
			if (ibotGame_0 == null)
			{
				return;
			}
			foreach (BotOwner botOwner in ibotGame_0.BotsController.Bots.BotOwners)
			{
				if (dictionary_0.TryGetValue(botOwner.Id, out var value))
				{
					if (botOwner.HasPathAndNotComplete)
					{
						float magnitude = (value - botOwner.Position).magnitude;
						dictionary_0[botOwner.Id] = botOwner.Position;
						if (!dictionary_1.ContainsKey(botOwner.Id))
						{
							dictionary_1.Add(botOwner.Id, 0f);
						}
						if (!dictionary_2.ContainsKey(botOwner.Id))
						{
							dictionary_2.Add(botOwner.Id, 0);
						}
						dictionary_2[botOwner.Id]++;
						dictionary_1[botOwner.Id] = dictionary_1[botOwner.Id] + magnitude;
						if (dictionary_2[botOwner.Id] > 200 && dictionary_1[botOwner.Id] < 3f && float_0 < Time.time)
						{
							float_0 = Time.time + 7f;
							Debug.LogError($"looks like bot stucked:{botOwner.Id} at:{botOwner.Position}");
						}
						if (dictionary_1[botOwner.Id] > 3f)
						{
							dictionary_1[botOwner.Id] = 0f;
							dictionary_2[botOwner.Id] = 0;
						}
					}
				}
				else
				{
					dictionary_0.Add(botOwner.Id, botOwner.Position);
				}
			}
		}
	}
}
