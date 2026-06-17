using Deadlands.Hooks;
using System;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

namespace Deadlands.Creatures.Buzzard
{

    /// <summary>
    /// Extension of VultureAI with ItemTracking capabilities used for the Buzzard
    /// </summary>
    internal class BuzzardAI : VultureAI, IUseItemTracker
    {

        private BuzzardModule Module
        {
            get {
                BuzzardHooks.BuzzardModule.TryGetValue(vulture, out var value);
                return value;
            }
        }


        public ItemTracker.ItemRepresentation focusWepon;

        public List<EntityID> unreachableWepon;

        public int iCantGetTheWeponHelp;
        public BuzzardAI(AbstractCreature creature, World world) : base(creature, world)
        {
            base.AddModule(new ItemTracker(this, 10, 15, 600, 4000, true));

            this.unreachableWepon = new List<EntityID>();

        }
        public override void Update()
        {
            if (behavior == Behavior.Hunt && !RainWorldGame.RequestHeavyAi(vulture))
            {
                return;
            }

            if (ModManager.MSC && vulture.LickedByPlayer != null)
            {
                base.tracker.SeeCreature(vulture.LickedByPlayer.abstractCreature);
                if (timeInRoom - 2 > 6000)
                {
                    timeInRoom -= 2;
                }
            }

            if (debugDestinationVisualizer != null)
            {
                debugDestinationVisualizer.Update();
            }

            if (creatureLooker != null)
            {
                creatureLooker.Update();
            }
            if (vulture.room != null)
            {
                for (int j = 0; j < this.itemTracker.ItemCount; j++)
                {
                    if (focusWepon == null)
                    {
                        focusWepon = this.itemTracker.GetRep(j);
                    }
                    else if (Custom.DistLess(vulture.mainBodyChunk.pos, base.itemTracker.GetRep(j).representedItem.realizedObject.bodyChunks[0].pos, Custom.Dist(vulture.mainBodyChunk.pos, focusWepon.representedItem.realizedObject.bodyChunks[0].pos)))
                    {
                        focusWepon = this.itemTracker.GetRep(j);
                    }
                }
            }
            else focusWepon = null; 


                timeInRoom++;
            if (vulture.room.game.IsStorySession && vulture.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
            {
                timeInRoom++;
            }

            disencouraged = Mathf.Max(0f, disencouraged - 1f / Mathf.Lerp(600f, 4800f, disencouraged));
            preyInTuskChargeRange = false;
            behavior = Behavior.Idle;
            base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.05f + 0.95f * Mathf.InverseLerp(IsMiros ? 4000f : 9600f, IsMiros ? 7600f : 6000f, timeInRoom);
            

            if (ModManager.MMF && vulture.bodyChunks[0].pos.y < 0f - vulture.bodyChunks[0].restrictInRoomRange + 1f)
            {
                creature.abstractAI.SetDestination(vulture.room.GetWorldCoordinate(new Vector2(vulture.bodyChunks[0].pos.x, 500f)));
                return;
            }

            AIModule aIModule = base.utilityComparer.HighestUtilityModule();
            if (base.utilityComparer.HighestUtility() > 0.01f && aIModule != null)
            {
                if (aIModule is PreyTracker)
                {
                    behavior = Behavior.Hunt;
                }

                if (aIModule is StuckTracker)
                {
                    behavior = Behavior.GetUnstuck;
                }

                if (aIModule is DisencouragedTracker)
                {
                    behavior = Behavior.Disencouraged;
                }
            }

            if (vulture.grasps[0] != null && vulture.grasps[0].grabbed is Creature && vulture.Template.CreatureRelationship(vulture.grasps[0].grabbed as Creature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                behavior = (base.denFinder.GetDenPosition().HasValue ? Behavior.ReturnPrey : Behavior.Idle);
            }

            if ( (creature.abstractAI as VultureAbstractAI).lostMask != null && base.utilityComparer.HighestUtility() < 0.4f && (creature.abstractAI as VultureAbstractAI).lostMask.Room.realizedRoom == vulture.room && (creature.abstractAI as VultureAbstractAI).lostMask.realizedObject != null)
            {
                behavior = Behavior.GoToMask;
                WorldCoordinate worldCoordinate = vulture.room.GetWorldCoordinate((creature.abstractAI as VultureAbstractAI).lostMask.realizedObject.firstChunk.pos);
                if (creature.world.GetAbstractRoom(worldCoordinate.room).AttractionForCreature(creature) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                {
                    SetDestination(worldCoordinate);
                }
            }

            if (!(behavior == Behavior.GoToMask))
            {
                if (behavior == Behavior.Idle)
                {
                    creature.abstractAI.AbstractBehavior(1);
                    if (creature.world.GetAbstractRoom(creature.abstractAI.destination.room).AttractionForCreature(creature) != AbstractRoom.CreatureRoomAttraction.Forbidden && creature.abstractAI.destination.room == creature.pos.room && creature.abstractAI.destination.NodeDefined && creature.world.GetNode(creature.abstractAI.destination).type == AbstractRoomNode.Type.SkyExit && (!creature.abstractAI.destination.TileDefined || creature.abstractAI.destination.Tile.FloatDist(creature.pos.Tile) < 10f))
                    {
                        RoomBorderExit roomBorderExit = vulture.room.borderExits[creature.abstractAI.destination.abstractNode - vulture.room.exitAndDenIndex.Length];
                        if (roomBorderExit.borderTiles.Length != 0)
                        {
                            IntVector2 intVector = roomBorderExit.borderTiles[Random.Range(0, roomBorderExit.borderTiles.Length)];
                            IntVector2 intVector2 = new IntVector2(0, 1);
                            if (intVector.x == 0)
                            {
                                intVector2 = new IntVector2(-1, 0);
                            }
                            else if (intVector.x == vulture.room.TileWidth - 1)
                            {
                                intVector2 = new IntVector2(1, 0);
                            }
                            else if (intVector.y == 0)
                            {
                                intVector2 = new IntVector2(0, -1);
                            }
                            intVector += intVector2 * ((intVector2.y == 1) ? Random.Range(0, 40) : Random.Range(0, 10));
                            creature.abstractAI.SetDestination(new WorldCoordinate(creature.abstractAI.destination.room, intVector.x, intVector.y, creature.abstractAI.destination.abstractNode));

                        }
                    }
                }
                else if (behavior == Behavior.ReturnPrey || behavior == Behavior.EscapeRain || behavior == Behavior.Disencouraged)
                {
                    focusCreature = null; 
                    focusWepon = null;
                    if (base.denFinder.GetDenPosition().HasValue)
                    {
                        creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
                    }
                }
                else if (behavior == Behavior.Hunt)
                {
                    focusCreature = base.preyTracker.MostAttractivePrey;
                    if (focusCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
                    {
                        timeInRoom = 0;
                    }

                    WorldCoordinate destination = focusCreature.BestGuessForPosition();
                    bool flag = ((!IsMiros) ? focusCreature.representedCreature.creatureTemplate.IsVulture : (focusCreature.representedCreature.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture));
                    if (flag && focusCreature.VisualContact && focusCreature.representedCreature.realizedCreature != null)
                    {
                        destination = vulture.room.GetWorldCoordinate(focusCreature.representedCreature.realizedCreature.bodyChunks[4].pos);
                    }
                    if (creature.world.GetAbstractRoom(destination.room).AttractionForCreature(creature) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                    {
                        //Debug.Log("Is null? "+ focusWepon == null);
                        ///Decide if you want to go for the prey or for a weapon depending on the distance
                        if (focusCreature.representedCreature.realizedCreature != null && focusWepon != null && !this.unreachableWepon.Contains(focusWepon.representedItem.ID) && !focusCreature.representedCreature.realizedCreature.dead && focusWepon.visualContact && vulture.grasps[0] == null && Custom.DistLess(vulture.mainBodyChunk.pos, focusWepon.representedItem.realizedObject.bodyChunks[0].pos, Custom.Dist(vulture.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.bodyChunks[0].pos)))
                        {
                            Debug.Log("Goes for Wepon");
                            WorldCoordinate destination2 = focusWepon.BestGuessForPosition();
                            if (focusWepon.representedItem.realizedObject != null && /*this.focusWepon.representedItem.realizedObject.grabbedBy.Count > 0 &&*/ Custom.DistLess(this.vulture.mainBodyChunk.pos, this.focusWepon.representedItem.realizedObject.firstChunk.pos, 150f))
                            {
                                this.iCantGetTheWeponHelp++;
                                if (this.iCantGetTheWeponHelp > 60)
                                {
                                    Debug.Log(new string[]
                                    {
                                    string.Format("Unreachable Wepon {0}", this.focusWepon.representedItem.ID)
                                    });
                                    if (!this.unreachableWepon.Contains(this.focusWepon.representedItem.ID))
                                    {
                                        this.unreachableWepon.Add(this.focusWepon.representedItem.ID);
                                    }
                                    this.iCantGetTheWeponHelp = 0;
                                }
                            }
                            creature.abstractAI.SetDestination(destination2);
                        }
                        else
                        {
                            Debug.Log("Goes for Criature");
                            creature.abstractAI.SetDestination(destination);
                        }

                    }

                    if (focusCreature.VisualContact)
                    {
                        Creature realizedCreature = focusCreature.representedCreature.realizedCreature;
                        if (realizedCreature.bodyChunks.Length != 0)
                        {
                            BodyChunk bodyChunk = realizedCreature.bodyChunks[Random.Range(0, realizedCreature.bodyChunks.Length)];
                            preyInTuskChargeRange = Custom.DistLess(vulture.mainBodyChunk.pos, bodyChunk.pos, 230f);
                            if ((!vulture.AirBorne || Random.value < 1f / 60f) && vulture.tuskCharge == 1f && vulture.snapFrames == 0 && !vulture.isLaserActive() && !vulture.safariControlled && Custom.DistLess(vulture.mainBodyChunk.pos, bodyChunk.pos, 130f) && vulture.room.VisualContact(vulture.bodyChunks[4].pos, bodyChunk.pos))
                            {
                                vulture.Snap(bodyChunk);
                            }
                        }
                    }
                }
                else if (behavior == Behavior.GetUnstuck)
                {
                    creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
                }
            }

            timeInRoom++;
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Update();
            }

            if (ModManager.Expedition && creature.world.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-hunted") && creature.world.rainCycle.CycleProgression > 0.05f && tracker != null && creature.world.game.Players != null)
            {
                for (int j = 0; j < creature.world.game.Players.Count; j++)
                {
                    if (creature.world.game.Players[j].realizedCreature != null && !(creature.world.game.Players[j].realizedCreature as Player).dead)
                    {
                        if (creature.Room != creature.world.game.Players[j].Room)
                        {
                            tracker.SeeCreature(creature.world.game.Players[j]);
                        }

                        break;
                    }
                }
            }

            if (ModManager.Watcher && creature.rippleCreature)
            {
                int num = ((creature.rippleLayer != creature.world.game.ActiveRippleLayer) ? 30 : 120);
                if (Random.value <= 1f / (40f * (float)num) && creature.realizedCreature != null && creature.realizedCreature.room != null)
                {
                    List<CosmeticRipple> list = new List<CosmeticRipple>();
                    for (int k = 0; k < creature.realizedCreature.room.cosmeticRipples.Count; k++)
                    {
                        if (creature.realizedCreature.room.cosmeticRipples[k].Data != null && creature.realizedCreature.room.cosmeticRipples[k].Data.cycleExpiry > 0)
                        {
                            list.Add(creature.realizedCreature.room.cosmeticRipples[k]);
                        }
                    }

                    if (list.Count > 0)
                    {
                        int index = Random.Range(0, list.Count);
                        ripplePathingTarget = list[index];
                        ripplePathingTime = 400;
                    }
                }
            }

            if (ripplePathingTarget != null)
            {
                ripplePathingTime--;
                if (ripplePathingTarget.slatedForDeletetion || ripplePathingTime <= 0)
                {
                    ripplePathingTarget = null;
                    ripplePathingTime = 0;
                }
                else if (creature.realizedCreature != null && creature.realizedCreature.room != null)
                {
                    SetDestination(creature.realizedCreature.room.GetWorldCoordinate(ripplePathingTarget.pos));
                }
            }

        }

        public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
        {
            this.creatureLooker.ReevaluateLookObject(creatureRep, firstSpot ? 3f : 2f);
        }
        public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
        {
        }


        public bool TrackItem(AbstractPhysicalObject obj)
        {
            return obj.realizedObject != null && obj.realizedObject is Weapon;
        }

        public void GrabObject(PhysicalObject obj)
        {
            
            if (Module != null)
            {
                
                try
                {
                    
                        Debug.Log("it gets to the grab222222");
                        Module.wantToGrabChunk = obj.firstChunk;
                    
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error running Act");
                    Debug.LogError(ex);
                }
            }
            
        }
    }
}
