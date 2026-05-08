public interface IBundleLock
{
	bool IsLocked { get; }

	void Lock();

	void Unlock();
}
