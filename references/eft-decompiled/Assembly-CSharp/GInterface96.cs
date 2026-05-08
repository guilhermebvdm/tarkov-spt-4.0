using System;

public interface GInterface96
{
	bool IsOccupied { get; }

	event Action<int> Occupied;

	event Action<int> Released;

	void Occupy();

	void Release();
}
