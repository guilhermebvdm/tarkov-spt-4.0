using EFT.InputSystem;

public interface IInputTree
{
	void AddWithLowPriority(InputNode node);

	void Add(InputNode node);

	void Remove(InputNode node);

	bool Contains(InputNode node);
}
