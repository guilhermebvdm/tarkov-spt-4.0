using System.Collections.Generic;

public interface ISpatialRoom
{
	short ID { get; }

	List<ISpatialPortal> GetPortals();
}
