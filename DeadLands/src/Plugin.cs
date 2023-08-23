using Sys = System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
//using DressMySlugcat;
using System.Security.Permissions;
using Fisobs.Core;

using Deadlands.Nomad;
using Deadlands.Enums;

//Private members can't be accesed?, NOT ANYMORE    
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Deadlands;

[BepInPlugin("DeadLands", "DeadLands", "0.1.2")]
class Plugin : BaseUnityPlugin
{
    public static SlugcatStats.Name Name;

    private readonly DeadlandsOptions _options;
    bool _initialized;

    private void LogInfo(object ex) => Logger.LogInfo(ex);

    //Slugbase features, HELL I WANT TO DELETE THEM ALL!!!!!!!
    public static readonly PlayerFeature<int> SlideStamina = PlayerInt("Nomad/SlideStamina");
    public static readonly PlayerFeature<float> SlideRecovery = PlayerFloat("Nomad/SlideRecovery");
    public static readonly PlayerFeature<float> SlideSpeed = PlayerFloat("Nomad/SlideSpeed");
    public static readonly PlayerFeature<bool> Nomad = PlayerBool("Nomad/Nomad");



    public Plugin()
    {
        if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Nomad", true, out var extEnum))
        {
            Name = extEnum as SlugcatStats.Name;
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


    public void OnEnable()
    {
        LogInfo("DeadLands is working or skill issue?, wortking");

        // Content.Register(new CactusChunkFisob());

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.Player.Jump += PlayerJump;
    }


    private void PlayerJump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        // A PhysicalObject is the form of an object you actually see, and are physical objects that can collide with creatures and objects around them, pretty self explanatory
        // PhysicalObjects are created upon 'Realization', and deleted upon 'Abstractization'
        // An AbstractPhysicalObject is the unloaded', simplest form of an object, they hold data that needs to be saved when a PhysicalObject is deleted (ID, Health, etc)

        // Upon Abstractization, the object's PhysicalObject is deleted and becomes just an AbstractPhysicalObject (a piece of data), Abstraction typically happens when something is no longer visible
        //      (ie, a creature going into a den, the player swallowing an item, a creature entering an unloaded room, etc)

        // Upon Realization, the object's PhysicalObject is recreated and starts existing, Realization happens when something becomes visible and it's physical form is needed
        //      (ie, a creature coming out of a den, the player spitting out an item, A creature entering a loaded room, etc)

        // Example, each explosive spear has a unique cloth length, and each unique Espear's cloth length should never change, so it is saved in it's AbstractPhysicalObject form
        // Let's say that the Espear's cloth length stops getting saved to the APO, in that case the length of the Espear cloth would be different every time you go through a pipe
        //      (I think this might happen with electric spears in-game, which is pretty silly)
        //      (going through a pipe, in other words abstraction, and realization again)


        // With this knowledge in mind look at the process of spawning an object now


        //CactusChunkAbstract cacc = new CactusChunkAbstract(self.room.world, self.coord, self.room.game.GetNewID()); // Create an abstract object

        //cacc.Realize(); // Realize the object (Create the PhysicalObject)
        //cacc.realizedObject.PlaceInRoom(self.room); // ...Place the object? Not sure why this is needed, but it doesn't spawn without it

        //self.room.abstractRoom.AddEntity(cacc); // Add the object to the room


        // While I'm pretty sure this method of spawning things should work with most objects, I havent tested it with vanilla objects, and they seem to be a bit different from custom objects
        // Hope this explanation was at least slightly helpful :]

        // CactusChunkExplosion.CactusExplosion(
        //     room: self.room,
        //     pos: new WorldCoordinate(self.coord.room, self.coord.x, self.coord.y + 6, self.coord.abstractNode),
        //     projectiles: Random.value > 0.5f ? 5 : 6); // Explosion
    }


    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            //Security check
            if (_initialized) return;
            _initialized = true;

            //Sounds enums
            DeadlandsEnums.RegisterValues();
            //Fly Logic
            var slideData = NomadFly.OnInit();

            NomadGraphics.OnInit(slideData);

            //DMS compatibility
            /*if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
            {
                NomadDMScompatibility.OnInit();
            }*/
                
            //Remix Menu
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