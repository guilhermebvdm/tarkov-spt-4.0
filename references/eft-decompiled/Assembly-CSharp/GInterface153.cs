using System;
using System.Collections.Generic;

public interface GInterface153<T, U>
{
	IEnumerable<KeyValuePair<T, U>> Items { get; }

	event Action<T, U> ItemAdded;

	event Action<T> ItemRemoved;
}
