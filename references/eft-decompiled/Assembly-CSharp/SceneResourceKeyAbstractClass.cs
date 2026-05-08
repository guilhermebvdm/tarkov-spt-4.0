using EFT;

public abstract class SceneResourceKeyAbstractClass
{
	public static readonly ResourceKey EftMainScene = new ResourceKey
	{
		path = "assets/scenes/eftmainscene.bundle",
		rcid = "EftMainScene"
	};

	public static readonly ResourceKey ArenMainScene = new ResourceKey
	{
		path = "assets/scenes/arenmainscene.bundle",
		rcid = "ArenaMainScene"
	};

	public static readonly ResourceKey EmptyScene = new ResourceKey
	{
		path = "assets/commonassets/scenes/emptyscene.bundle",
		rcid = "EmptyScene"
	};

	public static readonly ResourceKey CommonUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/commonuiscene.bundle",
		rcid = "CommonUIScene"
	};

	public static readonly ResourceKey EnvironmentUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/environmentuiscene.bundle",
		rcid = "EnvironmentUIScene"
	};

	public static readonly ResourceKey LoginUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/loginuiscene.bundle",
		rcid = "LoginUIScene"
	};

	public static readonly ResourceKey MenuUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/menuuiscene.bundle",
		rcid = "MenuUIScene"
	};

	public static readonly ResourceKey SessionEndUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/sessionenduiscene.bundle",
		rcid = "SessionEndUIScene"
	};

	public static readonly ResourceKey GameUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/gameuiscene.bundle",
		rcid = "GameUIScene"
	};

	public static readonly ResourceKey PreloaderUIScene = new ResourceKey
	{
		path = "assets/scenes/ui/preloaderuiscene.bundle",
		rcid = "PreloaderUIScene"
	};

	public static readonly ResourceKey HideoutScenesPreset = new ResourceKey
	{
		path = "assets/content/locations/_presets/bunker.scenespreset.bundle"
	};

	public const string HideoutSceneName = "bunker_2";

	public static readonly ResourceKey DissonanceSetupScene = new ResourceKey
	{
		path = "assets/dissonance/integrations/eft/dissonancesetup.bundle",
		rcid = "DissonanceSetup"
	};
}
