using System;

public class GClass849<T> : GClass848<T>
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public T Gparam_0;

	public override T Value
	{
		get
		{
			if (Bool_0)
			{
				Gparam_0 = Func_0();
				Bool_0 = false;
			}
			return Gparam_0;
		}
	}

	public bool IsDirty => Bool_0;

	public GClass849(Func<T> updateFunc)
		: base(updateFunc)
	{
		Bool_0 = true;
	}

	public override void SetDirty()
	{
		Bool_0 = true;
	}

	public static implicit operator T(GClass849<T> deferred)
	{
		return deferred.Value;
	}
}
