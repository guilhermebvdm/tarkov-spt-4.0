using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class GClass622<T, U, V>
{
	[NonSerialized]
	public Dictionary<T, V> Dictionary_0 = new Dictionary<T, V>();

	[NonSerialized]
	public Dictionary<T, U> Dictionary_1 = new Dictionary<T, U>();

	[NonSerialized]
	public Dictionary<U, T> Dictionary_2 = new Dictionary<U, T>();

	[NonSerialized]
	public ReaderWriterLockSlim ReaderWriterLockSlim_0 = new ReaderWriterLockSlim();

	public V this[U subKey]
	{
		get
		{
			if (!TryGetValue(subKey, out var val))
			{
				U val2 = subKey;
				throw new KeyNotFoundException("sub key not found: " + val2);
			}
			return val;
		}
	}

	public V this[T primaryKey]
	{
		get
		{
			if (!TryGetValue(primaryKey, out var val))
			{
				T val2 = primaryKey;
				throw new KeyNotFoundException("primary key not found: " + val2);
			}
			return val;
		}
	}

	public List<V> Values
	{
		get
		{
			ReaderWriterLockSlim_0.EnterReadLock();
			try
			{
				return Dictionary_0.Values.ToList();
			}
			finally
			{
				ReaderWriterLockSlim_0.ExitReadLock();
			}
		}
	}

	public int Count
	{
		get
		{
			ReaderWriterLockSlim_0.EnterReadLock();
			try
			{
				return Dictionary_0.Count;
			}
			finally
			{
				ReaderWriterLockSlim_0.ExitReadLock();
			}
		}
	}

	public void Associate(U subKey, T primaryKey)
	{
		ReaderWriterLockSlim_0.EnterUpgradeableReadLock();
		try
		{
			if (!Dictionary_0.ContainsKey(primaryKey))
			{
				throw new KeyNotFoundException($"The base dictionary does not contain the key '{primaryKey}'");
			}
			if (Dictionary_1.ContainsKey(primaryKey))
			{
				ReaderWriterLockSlim_0.EnterWriteLock();
				try
				{
					if (Dictionary_2.ContainsKey(Dictionary_1[primaryKey]))
					{
						Dictionary_2.Remove(Dictionary_1[primaryKey]);
					}
					Dictionary_1.Remove(primaryKey);
				}
				finally
				{
					ReaderWriterLockSlim_0.ExitWriteLock();
				}
			}
			Dictionary_2[subKey] = primaryKey;
			Dictionary_1[primaryKey] = subKey;
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitUpgradeableReadLock();
		}
	}

	public bool TryGetValue(U subKey, out V val)
	{
		val = default(V);
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			if (Dictionary_2.TryGetValue(subKey, out var value))
			{
				return Dictionary_0.TryGetValue(value, out val);
			}
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
		return false;
	}

	public bool TryGetValue(T primaryKey, out V val)
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			return Dictionary_0.TryGetValue(primaryKey, out val);
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public bool ContainsKey(U subKey)
	{
		V val;
		return TryGetValue(subKey, out val);
	}

	public bool ContainsKey(T primaryKey)
	{
		V val;
		return TryGetValue(primaryKey, out val);
	}

	public void Remove(T primaryKey)
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		try
		{
			if (Dictionary_1.ContainsKey(primaryKey))
			{
				if (Dictionary_2.ContainsKey(Dictionary_1[primaryKey]))
				{
					Dictionary_2.Remove(Dictionary_1[primaryKey]);
				}
				Dictionary_1.Remove(primaryKey);
			}
			Dictionary_0.Remove(primaryKey);
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitWriteLock();
		}
	}

	public void Remove(U subKey)
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		try
		{
			Dictionary_0.Remove(Dictionary_2[subKey]);
			Dictionary_1.Remove(Dictionary_2[subKey]);
			Dictionary_2.Remove(subKey);
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitWriteLock();
		}
	}

	public void Add(T primaryKey, V val)
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		try
		{
			Dictionary_0.Add(primaryKey, val);
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitWriteLock();
		}
	}

	public void Add(T primaryKey, U subKey, V val)
	{
		Add(primaryKey, val);
		Associate(subKey, primaryKey);
	}

	public V[] CloneValues()
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			V[] array = new V[Dictionary_0.Values.Count];
			Dictionary_0.Values.CopyTo(array, 0);
			return array;
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public T[] ClonePrimaryKeys()
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			T[] array = new T[Dictionary_0.Keys.Count];
			Dictionary_0.Keys.CopyTo(array, 0);
			return array;
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public U[] CloneSubKeys()
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			U[] array = new U[Dictionary_2.Keys.Count];
			Dictionary_2.Keys.CopyTo(array, 0);
			return array;
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}

	public void Clear()
	{
		ReaderWriterLockSlim_0.EnterWriteLock();
		try
		{
			Dictionary_0.Clear();
			Dictionary_2.Clear();
			Dictionary_1.Clear();
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitWriteLock();
		}
	}

	public IEnumerator<KeyValuePair<T, V>> GetEnumerator()
	{
		ReaderWriterLockSlim_0.EnterReadLock();
		try
		{
			return Dictionary_0.GetEnumerator();
		}
		finally
		{
			ReaderWriterLockSlim_0.ExitReadLock();
		}
	}
}
