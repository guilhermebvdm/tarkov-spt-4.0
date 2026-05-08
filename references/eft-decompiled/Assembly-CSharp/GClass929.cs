using System;
using System.Runtime.CompilerServices;
using Diz.Binding;
using UnityEngine;

public class GClass929 : GInterface39
{
	public bool IsGeneratedInRaid;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public Sprite Sprite_0;

	[NonSerialized]
	[CompilerGenerated]
	public BindableEvent BindableEvent_0;

	public int Hash
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public Sprite Sprite
	{
		[CompilerGenerated]
		get
		{
			return Sprite_0;
		}
		[CompilerGenerated]
		set
		{
			Sprite_0 = value;
		}
	}

	public BindableEvent Changed
	{
		[CompilerGenerated]
		get
		{
			return BindableEvent_0;
		}
	}

	public GClass929(int hash)
	{
		Int_0 = hash;
		BindableEvent_0 = new BindableEvent();
	}
}
