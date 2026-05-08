using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1003
{
	public static int MaxPoolSize = 20;

	public static Vector3 CameraPosition = new Vector3(10000f, 10000f);

	[NonSerialized]
	public static Queue<PhotoCamera> Queue_0 = new Queue<PhotoCamera>();

	[NonSerialized]
	public static int Int_0 = 0;

	public static void MakeTexture(GameObject gObject, Action<Texture> callback)
	{
		PhotoCamera photoCamera;
		if (Queue_0.Count == 0)
		{
			GameObject gameObject = new GameObject("PhotoCamera");
			gameObject.transform.position = CameraPosition;
			photoCamera = gameObject.AddComponent<PhotoCamera>();
			photoCamera.OnTextureRenderFinished += smethod_0;
		}
		else
		{
			photoCamera = Queue_0.Dequeue();
		}
		photoCamera.MakeTexture(gObject, callback, 3, new Vector2(1024f, 768f));
		Int_0++;
	}

	public static void smethod_0(PhotoCamera sender)
	{
		Queue_0.Enqueue(sender);
	}
}
