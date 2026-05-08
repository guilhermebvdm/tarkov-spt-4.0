using EFT;
using EFT.InventoryLogic;

public interface GInterface200
{
	Weapon Weapon { get; }

	float ErgonomicWeight { get; }

	float TotalErgonomics { get; }

	BifacialTransform CurrentFireport { get; }

	bool MouseLookControl { get; }

	void RecalculateErgonomic();
}
