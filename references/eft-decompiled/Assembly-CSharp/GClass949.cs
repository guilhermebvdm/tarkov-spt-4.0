using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass949
{
	[Serializable]
	[CompilerGenerated]
	public class Class595
	{
		public static readonly Class595 class595_0 = new Class595();

		public static Func<Transform, bool> func_0;

		public bool method_0(Transform child)
		{
			return child.gameObject.activeSelf;
		}
	}

	public static int ActiveChildCount(this Transform transform)
	{
		return transform.Cast<Transform>().Count((Transform child) => child.gameObject.activeSelf);
	}

	public static void SetRectTransformParent(this RectTransform transform, RectTransform objectToCopy)
	{
		transform.SetParent(objectToCopy);
		transform.anchorMin = new Vector2(0f, 0f);
		transform.anchorMax = new Vector2(1f, 1f);
		transform.offsetMax = new Vector2(0f, 0f);
		transform.offsetMin = new Vector2(0f, 0f);
		Vector3 localPosition = transform.localPosition;
		localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
		transform.localPosition = localPosition;
		transform.localScale = Vector3.one;
	}

	public static void ResetTransform(this Transform target)
	{
		target.localPosition = Vector3.zero;
		target.localRotation = Quaternion.identity;
		target.localScale = Vector3.one;
	}

	public static void CopyValues(this Transform target, Vector3 position, Vector3 rotation, Vector3 scale)
	{
		target.localPosition = position;
		target.localRotation = Quaternion.Euler(rotation);
		target.localScale = scale;
	}

	public static void CopyValues(this Transform target, Transform template)
	{
		target.localPosition = template.localPosition;
		target.localRotation = template.localRotation;
		target.localScale = template.localScale;
	}

	public static void CopyValuesWithDiff(this Transform target, Transform template, Vector3 diffPosition, Quaternion diffRotation)
	{
		target.localPosition = template.localPosition - diffPosition;
	}

	public static void CopyValues(this Transform target, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		target.localPosition = position;
		target.localRotation = rotation;
		target.localScale = scale;
	}

	public static void SetInCenter(this RectTransform transform)
	{
		PlaceRelativeToCenter(transform, Vector2.zero);
	}

	public static void PlaceRelativeToCenter(this RectTransform rectTransform, Vector2 relativePosition)
	{
		float num = 0.5f * (1f + relativePosition.x);
		float num2 = 0.5f * (1f + relativePosition.y);
		rectTransform.position = new Vector2((float)Screen.width * num, (float)Screen.height * num2);
		CorrectPositionResolution(rectTransform, rectTransform);
	}

	public static Rect GetGlobalBounds(this RectTransform rectTransform)
	{
		Rect rect = rectTransform.rect;
		Vector3 lossyScale = rectTransform.lossyScale;
		Vector3 vector = GClass855.Divide(rectTransform.TransformPoint(rect.xMin, rect.yMin, 0f), lossyScale);
		return new Rect(size: rect.size, position: vector);
	}

	public static void Move(this Transform transform, Vector3 delta)
	{
		transform.localPosition += GClass855.Divide(delta, transform.lossyScale);
	}

	public static RectTransform RectTransform(this GameObject gameObject)
	{
		return gameObject.transform as RectTransform;
	}

	public static RectTransform RectTransform(this Component component)
	{
		return component.transform as RectTransform;
	}

	public static MarginsStruct GetDistanceToBorders(this RectTransform transform, RectTransform referenceTransform = null, MarginsStruct margins = default(MarginsStruct))
	{
		if (referenceTransform == null)
		{
			referenceTransform = transform;
		}
		Vector3 position = transform.position;
		Rect rect = transform.rect;
		Rect rect2 = referenceTransform.rect;
		margins = margins.Scale(transform.lossyScale);
		rect.Set(rect.x, rect.y, rect2.width, rect2.height);
		Rect rect3 = GClass855.Scale(rect, transform.lossyScale);
		Vector2 vector = new Vector2(position.x + rect3.x, position.y + rect3.y);
		Vector2 vector2 = new Vector2(vector.x + rect3.width, vector.y + rect3.height);
		return new MarginsStruct(vector.x - margins.Left, (float)Screen.width - margins.Right - vector2.x, (float)Screen.height - margins.Top - vector2.y, vector.y - margins.Bottom);
	}

	public static void CorrectPositionResolution(this RectTransform transform, MarginsStruct margins = default(MarginsStruct))
	{
		CorrectPositionResolution(transform, transform, margins);
	}

	public static Vector2 GetTopLeftToPivotDelta(this RectTransform transform)
	{
		Vector2 vector = new Vector2(0f, 1f) - transform.pivot;
		return transform.rect.size * vector;
	}

	public static void CorrectPositionResolution(this RectTransform transform, RectTransform referenceTransform, MarginsStruct margins = default(MarginsStruct))
	{
		MarginsStruct distanceToBorders = GetDistanceToBorders(transform, referenceTransform, margins);
		Vector2 vector = transform.position;
		if (GClass855.Negative(distanceToBorders.Left))
		{
			vector.x -= distanceToBorders.Left;
		}
		else if (GClass855.Negative(distanceToBorders.Right))
		{
			vector.x += distanceToBorders.Right;
		}
		if (GClass855.Negative(distanceToBorders.Bottom))
		{
			vector.y -= distanceToBorders.Bottom;
		}
		else if (GClass855.Negative(distanceToBorders.Top))
		{
			vector.y += distanceToBorders.Top;
		}
		Vector2 vector2 = GetTopLeftToPivotDelta(transform) * transform.lossyScale;
		Vector2 vector3 = vector + vector2;
		Vector2 vector4 = new Vector2((float)Math.Round(vector3.x), (float)Math.Round(vector3.y));
		transform.position = vector4 - vector2;
	}
}
