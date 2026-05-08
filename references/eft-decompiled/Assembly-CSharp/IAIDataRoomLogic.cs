using System.Collections.Generic;

public interface IAIDataRoomLogic
{
	void AddRoom(ISpatialRoom room);

	void RemoveRoom(ISpatialRoom room);

	bool HaveRoom();

	List<ISpatialPortal> GetPortals();
}
