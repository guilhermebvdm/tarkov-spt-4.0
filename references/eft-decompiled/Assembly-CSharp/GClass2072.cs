using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class GClass2072
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0 = true;

	public List<ArraySegment<byte>> messages = new List<ArraySegment<byte>>();

	public bool Enabled
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public void Add(ArraySegment<byte> message)
	{
		messages.Add(GClass3068.Get(message));
	}

	public void Finish(Action<ArraySegment<byte>> handler)
	{
		foreach (ArraySegment<byte> message in messages)
		{
			handler(message);
			GClass3068.Return(message);
		}
		Enabled = false;
		messages.Clear();
	}
}
