using UnityEngine;

public interface GInterface5
{
	Vector3 Position { get; }

	int Id { get; }

	void AddWay(int id, GInterface5 wayTo);

	void ClearConnections();
}
