using System;
using System.Collections;
using System.Collections.Generic;

public interface GInterface156<out T> : IEnumerable<T>, IEnumerable
{
	event Action<T> ItemAdded;

	event Action<T> ItemRemoved;

	event Action<IEnumerable<T>> ItemsAdded;

	event Action<IEnumerable<T>> ItemsRemoved;

	event Action AllItemsRemoved;
}
