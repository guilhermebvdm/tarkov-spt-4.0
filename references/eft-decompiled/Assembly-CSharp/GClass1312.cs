using System;

public abstract class GClass1312 : IDisposable
{
	public readonly int Hash;

	public readonly string Name;

	[NonSerialized]
	public GClass1328[] Gclass1328_0 = new GClass1328[0];

	public int BehaviorsCount => Gclass1328_0.Length;

	public GClass1312(string name, int hash)
	{
		Name = name;
		Hash = hash;
	}

	public GClass1328 GetBehavior(int index)
	{
		return Gclass1328_0[index];
	}

	public T GetBehavior<T>() where T : GClass1328
	{
		int num = 0;
		while (true)
		{
			if (num < Gclass1328_0.Length)
			{
				if (Gclass1328_0[num] is T)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return (T)Gclass1328_0[num];
	}

	public override string ToString()
	{
		return $"[{GetType().Name}] Name: {Name}";
	}

	public virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Gclass1328_0 = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~GClass1312()
	{
		Dispose(disposing: false);
	}

	public virtual void ResetCache()
	{
		for (int i = 0; i < Gclass1328_0.Length; i++)
		{
			Gclass1328_0[i].ResetCache();
		}
	}
}
