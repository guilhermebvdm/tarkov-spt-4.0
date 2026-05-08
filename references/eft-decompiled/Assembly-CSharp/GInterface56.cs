using System;
using UnityEngine;

public interface GInterface56
{
	void ResetComponentsToDefaults();

	void SortComponentsToTurnOff();

	void EditorSetComponentsToTurnOff(Component[] components);

	T[] GetComponentsInChildren<T>(bool includeInactive);

	Component[] GetComponentsInChildren(Type t, bool includeInactive);
}
