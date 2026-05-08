using System.Collections.Generic;
using Diz.Binding;

public interface GInterface157<T> where T : IUpdatable<T>
{
	void UpdateItems(ICollection<T> newItems);
}
