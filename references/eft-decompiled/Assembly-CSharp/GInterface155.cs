using System.Collections;
using System.Collections.Generic;
using Diz.Binding;

public interface GInterface155<out T> : GInterface156<T>, IEnumerable<T>, IEnumerable
{
	BindableEvent ItemsChanged { get; }
}
