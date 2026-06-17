using BepInEx;
using Deadlands.Features.Opal;
using Fisobs.Core;
using Deadlands.Hooks;
using System.Security.Permissions;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;
using System.Security;

// IMPORTANT
// This requires Fisobs to work!
// Big thx to Dual-Iron (on github) for help with Fisobs!
// This code was based off of Dual-Iron's Centishield as practice, I didn't make parts of this! (Probably add more details on that later)

#pragma warning disable CS0618 // Do not remove the following line.

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Deadlands;

[BepInPlugin(GUID: "DeadLands", "DeadLands", "0.1.2")]
internal class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger { get; private set; } = null!;
    public const string MOD_ID = "DeadLands";
    public const string MOD_NAME = "DeadLands";
    public const string VERSION = "0.2.49.9.2";
    private DeadlandsOptions _options;
    private bool _initialized;

    private void OnEnable()
    {
        Logger = base.Logger;
        On.Room.Loaded += Room_Loaded;
        Debug.LogWarning($"{MOD_NAME} is loading....");
        //custom Items and creatures
        Content.Register(new OpalCritob());
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
            //loadResources(self);
            DeadlandsEnums.Init();
            DeadlandsEnums.RegisterEnums();

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

            //creatures and items
            CreatureHooks.Apply();

            
            Futile.atlasManager.LoadImage("assets/OpalPlant");
            if (!Futile.atlasManager.DoesContainAtlas("iguanahead"))
            {
                Futile.atlasManager.LoadAtlas("atlas/iguanahead");

            }
            if (!Futile.atlasManager.DoesContainAtlas("icon"))
            {
                Futile.atlasManager.LoadAtlas("atlas/icon");

            }
            if (!Futile.atlasManager.DoesContainAtlas("Buzzard_Mask"))
            {
                Futile.atlasManager.LoadAtlas("atlas/Buzzard_Mask");

            }
            if (!Futile.atlasManager.DoesContainAtlas("Buzzard_Mask"))
            {
                Futile.atlasManager.LoadAtlas("atlas/Buzzard_Mask");
            }
            

        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Remix Menu: Hook_OnModsInit options failed init error {_options}{ex}");
            Debug.LogError(ex);
        }
    }
    private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);

        for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
        {
            Debug.Log($"AbstractRoom entity count before: {self.abstractRoom.entities.Count}");
            if (self.roomSettings.placedObjects[i].type == OpalCritob.Opal)
            {

                PlacedObject currentObject = self.roomSettings.placedObjects[i];
                Debug.Log($"Adding Opal to room: {self.abstractRoom.name} at {currentObject.pos}");
                AbstractPhysicalObject Opalabstr = new OpalAbstract(self.world, self.GetWorldCoordinate(currentObject.pos), self.game.GetNewID());
                self.abstractRoom.AddEntity(Opalabstr);
            }
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
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(UnlockID.Buzzard))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(UnlockID.Buzzard);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(UnlockID.Iguana))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(UnlockID.Iguana);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(UnlockID.BrownLizard))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(UnlockID.BrownLizard);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(UnlockID.GlowLizard))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(UnlockID.GlowLizard);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(UnlockID.SaltWorm))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(UnlockID.SaltWorm);
                DLCreature.UnregisterValues();
                UnlockID.UnregisterValues();
                return;
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