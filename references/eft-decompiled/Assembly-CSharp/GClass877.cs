using UnityEngine;

public abstract class GClass877
{
	public struct GStruct57
	{
		public int vertCount;

		public Vector3[] verts;

		public int indCount;

		public int[] indices;

		public int colorCount;

		public Color[] colors;

		public Hash128 ComputeHash()
		{
			Hash128 result = Hash128.Compute("h");
			result.Append(vertCount);
			result.Append(verts);
			result.Append(indCount);
			result.Append(indices);
			result.Append(colorCount);
			result.Append(colors);
			return result;
		}

		public override string ToString()
		{
			return $"vc={vertCount} - {Hash128.Compute(verts)} ic={indCount} - {Hash128.Compute(indices)} cc={colorCount} - {Hash128.Compute(colors)}";
		}
	}

	public struct GStruct58
	{
		public Vector3 boundsCenter;

		public Vector3 boundsSize;

		public Matrix4x4 mat4x4;
	}

	public struct GStruct59
	{
		public enum NativeMeshRenderMode
		{
			TriangleFill,
			Wireframe
		}

		public GStruct57 meshData;

		public int transformationCount;

		public GStruct58[] transformations;

		public int renderMode;

		public Hash128 ComputeHash()
		{
			Hash128 result = meshData.ComputeHash();
			result.Append(transformations);
			return result;
		}
	}

	public class GClass878
	{
		public string NativeLibPath;

		public Vector3[] SamplingPositions;

		public GStruct59[] NativeMeshRenderers;
	}
}
