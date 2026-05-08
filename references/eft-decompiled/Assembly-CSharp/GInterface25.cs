using EFT.Vehicle;

public interface GInterface25
{
	PathConfig CurrentPathConfig { get; set; }

	MoveByPathType MoveByPathType { get; set; }

	void Initialization(MapPathConfig mapPathsConfig);

	void SpawnInPoint(int index);

	void MoveEnable();

	void MoveDisable();

	void MoveToDestination(string destinationID);

	void MovePauseOn(float pauseDuration);

	void MovePauseOff();

	void MoveProcess();

	void PauseProcess();

	void Reset();
}
