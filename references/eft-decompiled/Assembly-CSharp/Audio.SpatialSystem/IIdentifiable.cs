using System.Collections.Generic;

namespace Audio.SpatialSystem;

public interface IIdentifiable
{
	IReadOnlyCollection<string> Ids { get; }
}
