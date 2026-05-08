using EFT;
using UnityEngine;

public interface GInterface8
{
	bool WantSuppress();

	void SetWantSuppress();

	void ReportEnemy(IPlayer enemy, Vector3 enemypos, EEnemyPartVisibleType visibleType);
}
