using System;
using EFT;

public class LocalPlayerCullingHandlerClass : GClass917
{
	[NonSerialized]
	public LocalPlayer LocalPlayer_0;

	public void Initialize(LocalPlayer player, PlayerBones playerBones)
	{
		LocalPlayer_0 = player;
		method_0(playerBones);
	}

	public override void ApplyVisibleState()
	{
		base.ApplyVisibleState();
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].forceRenderingOff = false;
		}
		List_0.Clear();
		bool isVisible = base.IsVisible;
		LocalPlayer_0.PlayerBody.GetRenderersNonAlloc(List_0);
		for (int j = 0; j < List_0.Count; j++)
		{
			List_0[j].forceRenderingOff = !isVisible;
		}
	}
}
