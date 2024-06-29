namespace Deadlands;

[BepInPlugin(GUID: "DeadLands", "DeadLands", "0.1.2")]
internal class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "DeadLands";
    public const string MOD_NAME = "DeadLands";
    public const string VERSION = "0.2.49.9.2";

    private DeadlandsOptions _options;
    private bool _initialized;

    private void OnEnable()
    {
        Debug.LogWarning($"{MOD_NAME} is loading....");

        try
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogException(ex);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (_initialized) return;
            _initialized = true;

            DeadlandsEnums.Init();

            LoadShaders();

            // Core
            MenuHooks.Apply();

            // World
            DataPearlHooks.Apply();
            SLOracleHooks.Apply();
            WorldHooks.Apply();

            //Nomad
            NomadGliding.Apply();
            NomadGraphics.Apply();

            // Remix Menu
            MachineConnector.SetRegisteredOI("DeadLands", _options = new DeadlandsOptions());
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Remix Menu: Hook_OnModsInit options failed init error {_options}{ex}");
            Debug.LogError(ex);
        }
    }

    private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        for (int i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "DeadLands")
            {
                DeadlandsEnums.Unregister();
            }
            if (newlyDisabledMods[i].id == "moreslugcats")
            {
                DeadlandsEnums.Unregister();
            }
        }
    }

    private void LoadShaders()
    {
        var assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/deadlandsshaders"));
        if (assetBundle == null)
        {
            Debug.LogError("DeadLands Shaders Failed to load.");
            return;
        }

        // Wish Rain World had a built in way to do this :p
        Custom.rainWorld.Shaders["FlatLightNoFrag"] = FShader.CreateShader("FlatLightNoFrag", assetBundle.LoadAsset<Shader>("FlatLightNoFrag.shader"));
    }
}