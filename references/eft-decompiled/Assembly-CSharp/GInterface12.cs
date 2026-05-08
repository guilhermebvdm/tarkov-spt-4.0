using UnityEngine;

public interface GInterface12
{
	void SetPos(Vector3 pos, bool withLogs = false);

	void TryDrawAffectionGizmos();

	void Dispose();
}
