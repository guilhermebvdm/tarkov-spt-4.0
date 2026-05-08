using System.Collections.Generic;
using ChartAndGraph;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public interface Interface7
{
	List<BillboardText> TextObjects { get; }

	float Tile { get; set; }

	float Offset { get; set; }

	float Length { get; set; }

	[CanBeNull]
	BillboardText AddText(AnyChart chart, Text prefab, Transform parentTransform, int fontSize, float fontScale, string text, float x, float y, float z, float angle, object userData);

	void AddYZRect(Rect rect, int subMeshGroup, float xPosition);

	void AddXZRect(Rect rect, int subMeshGroup, float yPosition);

	void AddXYRect(Rect rect, int subMeshGroup, float depth);

	void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom);
}
