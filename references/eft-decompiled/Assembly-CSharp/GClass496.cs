using System;
using System.Text;
using EFT;
using UnityEngine;

public class GClass496(BotOwner owner) : GClass429(owner)
{
	public struct Struct14
	{
		public Vector3 WayPos;

		public Vector3 Pos;

		public Vector3 StartPos;

		public EBotLinkResult Type;
	}

	[NonSerialized]
	public const int Int_0 = 100;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public Struct14[] Struct14_0 = new Struct14[100];

	public void OnDrawGizmosPrevWay()
	{
		Gizmos.color = Color.magenta;
		Color black = Color.black;
		for (int i = Int_1; i < 99; i++)
		{
			Struct14 @struct = Struct14_0[i];
			Gizmos.color = black;
			Gizmos.DrawLine(@struct.WayPos, Struct14_0[i + 1].WayPos);
			Gizmos.DrawWireSphere(@struct.WayPos, 0.01f);
			method_0(@struct.Type);
			Gizmos.DrawLine(@struct.Pos, Struct14_0[i + 1].Pos);
			Gizmos.DrawWireSphere(@struct.Pos, 0.01f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(@struct.StartPos, Struct14_0[i + 1].StartPos);
		}
		for (int j = 0; j < Int_1 - 1; j++)
		{
			Struct14 struct2 = Struct14_0[j];
			Gizmos.color = black;
			Gizmos.DrawLine(struct2.WayPos, Struct14_0[j + 1].WayPos);
			Gizmos.DrawWireSphere(struct2.WayPos, 0.01f);
			method_0(struct2.Type);
			Gizmos.DrawLine(struct2.Pos, Struct14_0[j + 1].Pos);
			Gizmos.DrawWireSphere(struct2.Pos, 0.01f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(struct2.StartPos, Struct14_0[j + 1].StartPos);
		}
	}

	public void CantFindWayErrorTraceLog()
	{
		if (GClass398.Instance.IsTraceEnable())
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = Int_1; i < 99; i++)
			{
				Vector3 pos = Struct14_0[i].Pos;
				stringBuilder.Append($"_{pos}_");
			}
			for (int j = 0; j < Int_1 - 1; j++)
			{
				Vector3 pos2 = Struct14_0[j].Pos;
				stringBuilder.Append($"_{pos2}_");
			}
		}
	}

	public void AddDebugPos(EBotLinkResult debugType, Vector3 startPos, Vector3 realWorldPos, Vector3 originalWayPos)
	{
		if (Int_1 >= 100)
		{
			Int_1 = 0;
		}
		Struct14_0[Int_1] = new Struct14
		{
			StartPos = startPos,
			Pos = realWorldPos,
			WayPos = originalWayPos,
			Type = debugType
		};
		Int_1++;
	}

	public void method_0(EBotLinkResult type)
	{
		Color color = Color.magenta;
		switch (type)
		{
		case EBotLinkResult.complete:
			color = Color.yellow;
			break;
		case EBotLinkResult.fail:
			color = Color.red;
			break;
		case EBotLinkResult.superFail:
			color = Color.white;
			break;
		case EBotLinkResult.extraConnect:
			color = Color.blue;
			break;
		}
		Gizmos.color = color;
	}
}
