using System;

public interface GInterface61
{
	void Initialize();

	void Release(bool allowFadeOut = false, Action onCompleted = null);
}
