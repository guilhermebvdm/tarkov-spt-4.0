using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Settings.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class GClass2395
{
	public class Class1836 : GClass1094.GInterface67
	{
		public void Upgrade(JObject data)
		{
			Debug.Log(JsonParserClass.ToPrettyJson(data));
			smethod_0(data);
		}

		public static void smethod_0(JObject data)
		{
			foreach (JProperty item in data.Properties().ToList())
			{
				if (item.Name != "Version")
				{
					data.Remove(item.Name);
				}
			}
			data["SystemMessage"] = "Settings/Graphics/ResetWarning";
		}
	}

	public class Class1837 : GClass1094.GInterface67
	{
		public void Upgrade(JObject data)
		{
			JsonParserClass.ToPrettyJson(data);
			smethod_0(data);
		}

		public static void smethod_0(JObject data)
		{
			JProperty jProperty = data.Property("SSR");
			if (jProperty != null && !(jProperty.Value.Value<string>() == "Off"))
			{
				jProperty.Value = "Low";
			}
		}
	}

	public class Class1838 : GClass1094.GInterface67
	{
		[NonSerialized]
		public float[] Float_0 = new float[6] { 400f, 1000f, 1500f, 2000f, 2500f, 3000f };

		[NonSerialized]
		public float[] Float_1 = new float[33]
		{
			40f, 45f, 50f, 55f, 60f, 65f, 70f, 75f, 80f, 85f,
			90f, 95f, 100f, 105f, 110f, 115f, 120f, 125f, 130f, 135f,
			140f, 145f, 150f, 155f, 160f, 165f, 170f, 175f, 180f, 185f,
			190f, 195f, 200f
		};

		[NonSerialized]
		public float[] Float_2 = new float[5] { 2f, 2.5f, 3f, 3.5f, 4f };

		[NonSerialized]
		public float[] Float_3 = new float[31]
		{
			0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f,
			1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f,
			2f, 2.1f, 2.2f, 2.3f, 2.4f, 2.5f, 2.6f, 2.7f, 2.8f, 2.9f,
			3f
		};

		public void Upgrade(JObject data)
		{
			smethod_0(data);
			smethod_1(data);
			GClass1092.ConvertIndexToObject(data, "AspectRatio", Class1824.AspectRatio_0);
			GClass1092.ConvertIndexToValue(data, "OverallVisibility", Float_0);
			GClass1092.ConvertIndexToValue(data, "ShadowVisibility", Float_1);
			GClass1092.ConvertIndexToValue(data, "Sharpen", Float_3);
			GClass1092.ConvertIndexToValue(data, "ObjectLodQuality", Float_2);
			GClass1092.RenameProperty(data, "ObjectLodQuality", "LodBias");
		}

		public static void smethod_0(JObject data)
		{
			JProperty jProperty = data.Property("TextureQuality");
			if (jProperty != null && jProperty.Value.Value<int>() > 2)
			{
				jProperty.Value = new JValue(2L);
				JProperty jProperty2 = new JProperty("MipStreaming", true);
				JProperty jProperty3 = data.Property("MipStreaming");
				if (jProperty3 != null)
				{
					jProperty3.Replace(jProperty2);
				}
				else
				{
					data.Add(jProperty2);
				}
			}
		}

		public static void smethod_1(JObject data)
		{
			JProperty jProperty = data.Property("GraphicsQuality");
			if (jProperty != null)
			{
				JProperty jProperty2 = data.Property("CustomGraphicsQuality");
				if (jProperty2 != null && jProperty2.Value.Value<bool>())
				{
					jProperty.Value.Replace(JValue.CreateNull());
				}
				else
				{
					GClass1092.MakeIntOffset(data, "GraphicsQuality", -1);
				}
			}
		}
	}

	public class Class1839 : GClass1094.GInterface67
	{
		public void Upgrade(JObject data)
		{
			smethod_0(data);
		}

		public static void smethod_0(JObject data)
		{
			if (data.Property("DisplaySettings") == null)
			{
				GStruct293 gStruct = Class1826.smethod_17();
				if (data.TryGetValue("Display", out JToken value))
				{
					gStruct.Display = value.Value<byte>();
				}
				if (data.TryGetValue("FullscreenMode", out JToken value2))
				{
					gStruct.FullScreenMode = GClass2392.ToFullScreenMode(value2.Value<int>());
				}
				if (data.TryGetValue("Resolution", out JToken value3))
				{
					gStruct.Resolution = JsonConvert.DeserializeObject<EftResolution>(value3.ToString());
				}
				if (data.TryGetValue("AspectRatio", out JToken value4))
				{
					gStruct.AspectRatio = JsonConvert.DeserializeObject<AspectRatio>(value4.ToString());
				}
				data.Add("DisplaySettings", JObject.FromObject(gStruct));
			}
		}
	}

	public class Class1840 : GClass1094.GInterface67
	{
		public void Upgrade(JObject data)
		{
			Debug.Log(JsonParserClass.ToPrettyJson(data));
			smethod_0(data);
		}

		public static void smethod_0(JObject data)
		{
			if (data.Property("Stored") != null)
			{
				data.Remove("Stored");
			}
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static GClass1087.GInterface66 Ginterface66_0;

	[NonSerialized]
	public const string String_0 = "Stored";

	public static GClass1087.GInterface66 Serializer
	{
		[CompilerGenerated]
		get
		{
			return Ginterface66_0;
		}
	}

	static GClass2395()
	{
		GClass1094.GInterface67[] versions = new GClass1094.GInterface67[5]
		{
			new Class1838(),
			new Class1839(),
			new Class1840(),
			new Class1836(),
			new Class1837()
		};
		Ginterface66_0 = new GClass1094(JsonSerializerSettingsClass.SerializerSettings, versions);
	}
}
