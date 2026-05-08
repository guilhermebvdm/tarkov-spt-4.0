using EFT;

public struct ServerScenesDataStruct(ResourceKey key, bool disableServerScenes)
{
	public ResourceKey key = key;

	public bool DisableServerScenes = disableServerScenes;
}
