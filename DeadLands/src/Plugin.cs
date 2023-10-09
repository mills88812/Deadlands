using Sys = System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;

using Deadlands.Enums;

//Private members can't be accesed?, NOT ANYMORE    
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Deadlands;

[BepInPlugin("DeadLands", "DeadLands", "0.1.2")]
internal class Plugin : BaseUnityPlugin
{
    public static SlugcatStats.Name Name = null!;

    private readonly DeadlandsOptions _options;
    private bool _initialized;

    // private void LogInfo(object ex) => Logger.LogInfo(ex);

    //Slugbase features, HELL I WANT TO DELETE THEM ALL!!!!!!!
    public static readonly PlayerFeature<bool> Nomad = PlayerBool("Nomad/Nomad");



    public Plugin()
    {
        if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Nomad", true, out var extEnum))
        {
            Name = (extEnum as SlugcatStats.Name)!;
        }

        try
        {
            _options = new DeadlandsOptions();
        } 
        catch (Sys.Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }


    private void OnEnable()
    {
        // LogInfo("DeadLands is working or skill issue?, wortking");

        // Content.Register(new CactusChunkFisob());

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void Awake()
    {
        //Sounds enums
        DeadlandsEnums.RegisterValues();
    }


    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            // Security check
            if (_initialized) return;
            _initialized = true;
            
            Deadlands.Nomad.Nomad.OnInit();
                
            // Remix Menu
            MachineConnector.SetRegisteredOI("DeadLands", _options);
        }
        catch (Sys.Exception ex)
        {
            Debug.Log($"Remix Menu: Hook_OnModsInit options failed init error {_options}{ex}");
            Logger.LogError(ex);
            Logger.LogMessage("WHOOPS something go wrong");
        }
        finally
        {
            orig.Invoke(self);
        }
    }
}