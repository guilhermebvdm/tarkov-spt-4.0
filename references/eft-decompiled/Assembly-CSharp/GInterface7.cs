using EFT;
using UnityEngine;

public interface GInterface7
{
	bool EnoughtHaveGoodCovers { get; }

	Vector3? PointForBoss { get; }

	bool CanStartUseStimulator();

	void SetFightPosition(bool b, BotOwner owner);

	void SomebodyUseStimulator();

	void WantAskHeal(BotOwner owner);
}
