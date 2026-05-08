using System.Collections.Generic;
using UnityEngine;

public class GClass1271
{
	public List<GClass1272> renderers;

	public ComputeBuffer transformationMatrixAppendBuffer;

	public ComputeBuffer shadowAppendBuffer;

	public bool excludeBounds;

	public RenderTexture transformationMatrixAppendTexture;

	public RenderTexture shadowAppendTexture;

	public int argsBufferOffset
	{
		get
		{
			if (renderers != null && renderers.Count != 0)
			{
				return renderers[0].argsBufferOffset;
			}
			return -1;
		}
	}
}
