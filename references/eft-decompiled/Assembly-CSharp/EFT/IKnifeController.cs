using System;
using EFT.InventoryLogic;

namespace EFT;

public interface IKnifeController : GInterface199, IHandsController, GInterface197
{
	KnifeComponent Knife { get; }

	Action ComboPlanning { get; set; }

	Action OnAttackEnd { get; set; }

	void ExamineWeapon();

	bool MakeKnifeKick();

	bool MakeAlternativeKick();

	void BrakeCombo();

	void ContinueCombo();

	void SetBotParameters();
}
