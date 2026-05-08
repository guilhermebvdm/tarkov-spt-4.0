using UnityEngine;

public class ShowTextureStreamingSummary : MonoBehaviour
{
	public float X = 10f;

	public float Y = 20f;

	public int MemoryBudgetPercentThreshold = 80;

	public int TextureStreamingPercentThreshold = 50;

	private ulong ulong_0;

	private Rect rect_0;

	public void Start()
	{
		ulong_0 = 0uL;
	}

	public string HumanReadableSize(ulong size)
	{
		return $"{(float)size / 1048576f:0.0}M";
	}

	public void method_0(string text)
	{
		float num = GUI.skin.font.lineHeight;
		GUI.Label(rect_0, text);
		rect_0.y += num;
	}

	public void OnGUI()
	{
		rect_0 = new Rect(X, Y, (float)Screen.width - X, (float)Screen.height - Y);
		if (!SystemInfo.supportsMipStreaming)
		{
			method_0("Texture streaming unsupported");
		}
		if (!QualitySettings.streamingMipmapsActive)
		{
			method_0("Texture streaming disabled");
			return;
		}
		if (QualitySettings.streamingMipmapsMemoryBudget == 0f)
		{
			method_0("No texture streaming budget");
			return;
		}
		if (Texture.totalTextureMemory == 0L)
		{
			method_0("No texture memory needed");
			return;
		}
		if (Texture.desiredTextureMemory > ulong_0)
		{
			ulong_0 = Texture.desiredTextureMemory;
		}
		ulong num = (ulong)(1048576f * QualitySettings.streamingMipmapsMemoryBudget);
		float num2 = (float)(100L * Texture.desiredTextureMemory) / (float)num;
		method_0($"Memory budget utilisation {num2:0.0}% of {HumanReadableSize(num)} texture budget");
		if (ulong_0 > num)
		{
			ulong size = ulong_0 - num;
			method_0($"Memory exceeds budget by {HumanReadableSize(size)}");
		}
		else
		{
			ulong size2 = Texture.totalTextureMemory - ulong_0;
			float num3 = (float)(100L * ulong_0) / (float)Texture.totalTextureMemory;
			method_0($"Memory saving at least {HumanReadableSize(size2)} with streaming enabled ( at {num3:0.0}% of original {HumanReadableSize(Texture.totalTextureMemory)}) - ignoring caching");
		}
		if (num2 < (float)MemoryBudgetPercentThreshold)
		{
			method_0($"Reduce the Memory Budget closer to {HumanReadableSize(Texture.desiredTextureMemory)}");
		}
		else if (Texture.desiredTextureMemory > num)
		{
			method_0($"Raise Memory Budget above {HumanReadableSize(Texture.desiredTextureMemory)}");
		}
		float num4 = (float)(100L * (Texture.totalTextureMemory - Texture.nonStreamingTextureMemory)) / (float)Texture.totalTextureMemory;
		if (num4 < (float)TextureStreamingPercentThreshold)
		{
			method_0($"Mark more textures streaming to improve savings ({num4:0.0}% texture memory marked as streaming)");
		}
		if (!Texture.streamingTextureDiscardUnusedMips)
		{
			method_0("Consider turning on Texture.streamingTextureDiscardUnusedMips to analyse without cached textures");
		}
	}
}
