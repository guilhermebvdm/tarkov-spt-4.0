public interface GInterface32<T> where T : GInterface31
{
	void Register(T component);

	void Unregister(T component);
}
