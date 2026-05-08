using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public abstract class GClass860
{
	[CompilerGenerated]
	public class Class485
	{
		public Bounds bounds;

		public bool method_0(Plane plane)
		{
			return smethod_0(bounds).All(((Plane)plane).GetSide);
		}
	}

	[NonSerialized]
	public const int Int_0 = -3;

	[NonSerialized]
	public const float Float_0 = 0.15f;

	[NonSerialized]
	public static Dictionary<string, Vector3> Dictionary_0 = new Dictionary<string, Vector3>();

	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		return GeometryUtility.TestPlanesAABB(GClass823.CalculateFrustumPlanesNonAlloc(camera), renderer.bounds);
	}

	public static bool IsFullyVisibleByCamera(this Camera camera, Bounds bounds)
	{
		return GClass823.CalculateFrustumPlanesNonAlloc(camera).All((Plane plane) => smethod_0(bounds).All(((Plane)plane).GetSide));
	}

	public static void ClearTexture(this RenderTexture tex)
	{
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(tex);
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = active;
	}

	public static void PlaceInClosestPosition(this Camera camera, Bounds bounds, string itemTemplateId, GStruct55<float>? clampPosition = null)
	{
		if (camera == null)
		{
			return;
		}
		Transform transform = camera.transform;
		if (Dictionary_0.ContainsKey(itemTemplateId))
		{
			transform.localPosition = Dictionary_0[itemTemplateId];
			return;
		}
		transform.localPosition = new Vector3(0f, 0f, -3f);
		for (int i = 0; (float)i < Mathf.Abs(-20f) + 1f; i++)
		{
			transform.Translate(new Vector3(0f, 0f, 0.15f));
			if (!IsFullyVisibleByCamera(camera, bounds))
			{
				do
				{
					transform.Translate(new Vector3(0f, 0f, -0.01f));
				}
				while (!IsFullyVisibleByCamera(camera, bounds));
				break;
			}
		}
		if (clampPosition.HasValue)
		{
			Vector3 localPosition = transform.localPosition;
			localPosition.z = Mathf.Clamp(localPosition.z, clampPosition.Value.Min, clampPosition.Value.Max);
			transform.localPosition = localPosition;
		}
		Dictionary_0.Add(itemTemplateId, transform.localPosition);
	}

	public static IEnumerable<Vector3> smethod_0(Bounds bounds)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		list.Add(min);
		list.Add(max);
		list.Add(new Vector3(min.x, min.y, max.z));
		list.Add(new Vector3(min.x, max.y, min.z));
		list.Add(new Vector3(max.x, min.y, min.z));
		list.Add(new Vector3(min.x, max.y, max.z));
		list.Add(new Vector3(max.x, min.y, max.z));
		list.Add(new Vector3(max.x, max.y, min.z));
		return list;
	}

	public static bool ContainsPointConsiderCenter(this BoxCollider box, Vector3 point)
	{
		Vector3 vector = box.transform.InverseTransformPoint(point) - box.center;
		if (Mathf.Abs(vector.x) < box.size.x / 2f && Mathf.Abs(vector.y) < box.size.y / 2f)
		{
			return Mathf.Abs(vector.z) < box.size.z / 2f;
		}
		return false;
	}

	public static bool ContainsPoint(this BoxCollider box, Vector3 point)
	{
		Vector3 vector = box.transform.InverseTransformPoint(point);
		if (Mathf.Abs(vector.x) < box.size.x / 2f && Mathf.Abs(vector.y) < box.size.y / 2f)
		{
			return Mathf.Abs(vector.z) < box.size.z / 2f;
		}
		return false;
	}

	public static void BlitOrCopy(RenderTexture scr, RenderTexture dst)
	{
		if (dst == null)
		{
			Graphics.Blit(scr, dst);
		}
		else
		{
			Graphics.CopyTexture(scr, dst);
		}
	}

	public static void BlitOrCopy(CommandBuffer cmdBuf, RenderTexture scr, RenderTexture dst)
	{
		if (dst == null)
		{
			cmdBuf.BlitFullscreenTriangle(scr, dst);
		}
		else
		{
			cmdBuf.CopyTexture(scr, dst);
		}
	}
}
