using System;

public interface GInterface152<T>
{
	Action Subscribe(Action<T> handler);

	Action Bind(Action<T> handler, T defaultValue);
}
