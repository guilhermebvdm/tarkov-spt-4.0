using UnityEngine;

namespace ChartAndGraph;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererPathGenerator : PathGenerator
{
	private LineRenderer lineRenderer_0;

	public void EnsureRenderer()
	{
		lineRenderer_0 = GetComponent<LineRenderer>();
	}

	public override void Clear()
	{
		EnsureRenderer();
		if (lineRenderer_0 != null)
		{
			lineRenderer_0.positionCount = 0;
		}
	}

	public override void Generator(Vector3[] path, float thickness, bool closed)
	{
		EnsureRenderer();
		if (lineRenderer_0 != null)
		{
			lineRenderer_0.startWidth = thickness;
			lineRenderer_0.endWidth = thickness;
			lineRenderer_0.positionCount = path.Length;
			for (int i = 0; i < path.Length; i++)
			{
				lineRenderer_0.SetPosition(i, path[i]);
			}
		}
	}
}
