using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class Class679
{
	public class Class680 : IDisposable
	{
		[NonSerialized]
		[CompilerGenerated]
		public string String_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public Camera Camera_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public Struct160 Struct160_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public List<UnityEngine.Object> List_0_1 = new List<UnityEngine.Object>();

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public SSAA Ssaa_0;

		public string String_0
		{
			[CompilerGenerated]
			get
			{
				return String_0_1;
			}
		}

		public Camera Camera_0
		{
			[CompilerGenerated]
			get
			{
				return Camera_0_1;
			}
			[CompilerGenerated]
			set
			{
				Camera_0_1 = value;
			}
		}

		public Struct160 Struct160_0
		{
			[CompilerGenerated]
			get
			{
				return Struct160_0_1;
			}
			[CompilerGenerated]
			set
			{
				Struct160_0_1 = value;
			}
		}

		public List<UnityEngine.Object> List_0
		{
			[CompilerGenerated]
			get
			{
				return List_0_1;
			}
		}

		public static Class680 smethod_0(Camera camera)
		{
			camera.TryGetComponent<SSAA>(out var component);
			int num = ((component != null) ? component.GetInputWidth() : camera.pixelWidth);
			int num2 = ((component != null) ? component.GetInputHeight() : camera.pixelHeight);
			return new Class680(camera.name)
			{
				Camera_0 = camera,
				Struct160_0 = Struct160.smethod_0(num, num2, camera.allowHDR),
				Int_0 = num,
				Int_1 = num2,
				Bool_0 = camera.allowHDR,
				Ssaa_0 = component
			};
		}

		public Class680(string name)
		{
			String_0_1 = name;
		}

		public void Dispose()
		{
			Struct160.smethod_1(Struct160_0);
			Struct160_0 = default(Struct160);
			Camera_0 = null;
			Ssaa_0 = null;
			List_0.Clear();
		}

		public int method_0()
		{
			if (!(Ssaa_0 != null))
			{
				return Camera_0.pixelWidth;
			}
			return Ssaa_0.GetInputWidth();
		}

		public int method_1()
		{
			if (!(Ssaa_0 != null))
			{
				return Camera_0.pixelHeight;
			}
			return Ssaa_0.GetInputHeight();
		}

		public bool method_2()
		{
			if (Camera_0 == null)
			{
				return false;
			}
			int num = method_0();
			int num2 = method_1();
			bool allowHDR = Camera_0.allowHDR;
			if (Int_0 == num && Int_1 == num2 && Bool_0 == allowHDR)
			{
				return true;
			}
			Struct160.smethod_1(Struct160_0);
			Int_0 = num;
			Int_1 = num2;
			Bool_0 = allowHDR;
			Struct160_0 = Struct160.smethod_0(num, num2, allowHDR);
			Debug.Log($"Data {String_0} updated to width:{Int_0} height:{Int_1}");
			return true;
		}

		public bool method_3(UnityEngine.Object owner)
		{
			int num = 0;
			while (true)
			{
				if (num < List_0.Count)
				{
					if (List_0[num] == owner)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
	}

	[NonSerialized]
	public List<Class680> List_0 = new List<Class680>();

	[NonSerialized]
	public static Class679 Class679_0_1;

	public static Class679 Class679_0 => Class679_0_1 ?? (Class679_0_1 = new Class679());

	public void Update()
	{
		int num = 0;
		while (num < List_0.Count)
		{
			if (List_0[num].method_2())
			{
				num++;
			}
			else
			{
				List_0.RemoveAt(num);
			}
		}
	}

	public Struct160 method_0(Camera currentCamera, UnityEngine.Object owner)
	{
		return method_5(currentCamera, owner).Struct160_0;
	}

	public RenderTexture method_1(Camera currentCamera, UnityEngine.Object owner)
	{
		return method_5(currentCamera, owner).Struct160_0.RenderTexture_1;
	}

	public void method_2(UnityEngine.Object owner)
	{
		Debug.Log("TryUnregister owner:" + owner.name);
		int num = 0;
		while (num < List_0.Count)
		{
			Class680 @class = List_0[num];
			if (@class.method_3(owner))
			{
				@class.List_0.Remove(owner);
				Debug.Log("SnowRenderer:" + @class.String_0 + " owner:" + owner.name + " removed");
			}
			if (@class.List_0.Count == 0)
			{
				List_0.RemoveAt(num);
				@class.Dispose();
			}
			else
			{
				num++;
			}
		}
	}

	public void method_3(UnityEngine.Object owner, Camera currentCamera)
	{
		Class680 @class = method_4(currentCamera);
		if (@class == null)
		{
			Debug.LogError("Invalid currentCamera:" + currentCamera.name);
			return;
		}
		if (@class.method_3(owner))
		{
			@class.List_0.Remove(owner);
			Debug.Log("SnowRenderer:" + @class.String_0 + " owner:" + owner.name + " removed");
		}
		else
		{
			Debug.LogError("Invalid SnowRenderer data owner:" + owner.name + " camera:" + currentCamera.name);
		}
		if (@class.List_0.Count == 0)
		{
			List_0.Remove(@class);
			@class.Dispose();
		}
	}

	[CanBeNull]
	public Class680 method_4(Camera currentCamera)
	{
		int num = 0;
		Class680 result = null;
		while (num < List_0.Count)
		{
			Class680 @class = List_0[num];
			if (@class.Camera_0 == currentCamera)
			{
				result = @class;
			}
			if (@class.Camera_0 != null)
			{
				num++;
				continue;
			}
			Debug.Log("SnowRenderer:" + @class.String_0 + " camera is null");
			List_0.RemoveAt(num);
			@class.Dispose();
		}
		return result;
	}

	public Class680 method_5(Camera currentCamera, UnityEngine.Object owner)
	{
		Class680 @class = method_4(currentCamera);
		if (@class == null)
		{
			@class = Class680.smethod_0(currentCamera);
			@class.List_0.Add(owner);
			List_0.Add(@class);
			Debug.Log("SnowRenderer:" + currentCamera.name + " owner:" + owner.name + " Created");
		}
		else if (!@class.method_3(owner))
		{
			@class.List_0.Add(owner);
			Debug.Log("SnowRenderer:" + currentCamera.name + " owner:" + owner.name + " Added");
		}
		return @class;
	}
}
