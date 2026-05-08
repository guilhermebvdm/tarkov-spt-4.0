using System;

[Serializable]
[GAttribute29]
public class ClassTransformSync
{
	public ClassVector3 Position;

	public ClassQuaternion Rotation;

	public GStruct138 ToUnity()
	{
		return new GStruct138
		{
			Position = Position.ToUnityVector3(),
			Rotation = Rotation.ToUnityQuaternion()
		};
	}

	public static ClassTransformSync FromUnity(GStruct138 transformSync)
	{
		return new ClassTransformSync
		{
			Position = transformSync.Position,
			Rotation = transformSync.Rotation
		};
	}

	public static GStruct138[] ToUnity(ClassTransformSync[] syncs)
	{
		GStruct138[] array = new GStruct138[syncs.Length];
		for (int i = 0; i < syncs.Length; i++)
		{
			ClassTransformSync classTransformSync = syncs[i];
			array[i] = classTransformSync.ToUnity();
		}
		return array;
	}

	public static ClassTransformSync[] FromUnity(GStruct138[] syncs)
	{
		ClassTransformSync[] array = new ClassTransformSync[syncs.Length];
		for (int i = 0; i < syncs.Length; i++)
		{
			GStruct138 transformSync = syncs[i];
			array[i] = FromUnity(transformSync);
		}
		return array;
	}
}
