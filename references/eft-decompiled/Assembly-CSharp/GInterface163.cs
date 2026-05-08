using System;

public interface GInterface163
{
	string Name { get; set; }

	event Action<string, GInterface163> NameChanged;

	void CancelNameChange();
}
