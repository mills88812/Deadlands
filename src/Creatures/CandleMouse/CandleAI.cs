using Noise;
using RWCustom;
using System;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;
using Random = UnityEngine.Random;

namespace Deadlands.Creatures.CandleMouse
{
    internal class CandleAI : ArtificialIntelligence, IUseARelationshipTracker, IUseItemTracker, IAINoiseReaction
    {
        public CandleMouse mouse;
        public AbstractCreature targetCreature;

        public Behavior behavior;
        public float fear;
        public DebugDestinationVisualizer debugDestinationVisualizer;
        private float currentUtility;

        public Tracker.CreatureRepresentation focusCreature;
        public ItemTracker.ItemRepresentation focusItem;
        public WorldCoordinate? walkWithMouse;
        private int noiseRectionDelay;
        private int huntAttackCounter;
        private AbstractCreature tiredOfHuntingCreature;
        private int tiredOfHuntingCounter;
        private int idlePosCounter;

        public CandleAI(AbstractCreature creature, World world) : base(creature, world)
        {
            this.mouse = (creature.realizedCreature as CandleMouse);
            this.mouse.AI = this;
            base.AddModule(new StandardPather(this, world, creature));
            base.AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
            base.AddModule(new PreyTracker(this, 5, 1f, 5f, 15f, 0.95f));
            base.AddModule(new ThreatTracker(this, 3));
            base.AddModule(new RainTracker(this));
            base.AddModule(new DenFinder(this, creature));
            base.AddModule(new NoiseTracker(this, base.tracker));
            base.AddModule(new UtilityComparer(this));
            base.AddModule(new RelationshipTracker(this, base.tracker));
            base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
            //this.tracker.visualize = true;
        }

        public override void Update()
        {
            base.Update();
            if (this.mouse.room == null)
            {
                return;
            }
            /*
            if (this.debugDestinationVisualizer == null)
            {
                this.debugDestinationVisualizer = new DebugDestinationVisualizer(this.mouse.room.game.abstractSpaceVisualizer, this.mouse.room.world, this.pathFinder, Color.cyan);
            } else
            {
                this.debugDestinationVisualizer.Update();
            }
            */
            base.pathFinder.walkPastPointOfNoReturn = (this.stranded || base.denFinder.GetDenPosition() == null || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value) || base.threatTracker.Utility() > 0.95f);
            if (this.mouse.sitting)
            {
                this.noiseTracker.hearingSkill = 1.4f;
            }
            else
            {
                this.noiseTracker.hearingSkill = 0.6f;
            }
            if (base.preyTracker.MostAttractivePrey != null)
            {
                utilityComparer.GetUtilityTracker(this.preyTracker).weight = Custom.LerpMap(this.creature.pos.Tile.FloatDist(base.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile), 26f, 36f, 1f, 0.1f);
            }
            AIModule aimodule = utilityComparer.HighestUtilityModule();
            this.currentUtility = utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    this.behavior = Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    this.behavior = Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    this.behavior = Behavior.Hunt;
                }
            }
            if (this.currentUtility < 0.1f)
            {
                this.behavior = Behavior.Idle;
            }
            if (this.mouse.grasps[0] != null && (this.currentUtility < 0.7f || this.behavior == Behavior.Hunt))
            {
                var grabbed = this.mouse.grasps[0].grabbed;
                if (grabbed is IPlayerEdible || (grabbed is Creature && this.StaticRelationship((grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats))
                {
                    if (this.denFinder.GetDenPosition != null && (this.behavior != Behavior.Flee || this.currentUtility < 0.2f))
                    {
                        this.behavior = Behavior.ReturnPrey;
                    }
                    else if (this.behavior == Behavior.Hunt)
                    {
                        this.behavior = Behavior.Idle;
                    }
                    else if (Random.value < 0.015f)
                    {
                        this.mouse.LoseAllGrasps();
                    }
                }
            }
            if (behavior == Behavior.Idle)
            {
                this.mouse.runSpeed = Mathf.Lerp(this.mouse.runSpeed, 1f, 0.05f);
                if (!this.pathFinder.CoordinateReachableAndGetbackable(this.pathFinder.GetDestination))
                {
                    this.creature.abstractAI.SetDestination(this.creature.pos);
                }
                if (this.walkWithMouse != null)
                {
                    this.creature.abstractAI.SetDestination(this.walkWithMouse.Value);
                    if (Random.value < 0.02f || Custom.ManhattanDistance(this.creature.pos, this.walkWithMouse.Value) < 4)
                    {
                        this.walkWithMouse = null;
                    }
                }
                if (this.mouse.charge < 9)
                {

                }
                if (pathFinder.GetDestination.room == mouse.room.abstractRoom.index && (Random.value < 0.0045454544f || this.idlePosCounter <= 0))
                {
                    IntVector2 pos = new IntVector2(Random.Range(0, this.mouse.room.TileWidth), Random.Range(0, this.mouse.room.TileHeight));
                    if (base.pathFinder.CoordinateReachableAndGetbackable(this.mouse.room.GetWorldCoordinate(pos)))
                    {
                        this.creature.abstractAI.SetDestination(this.mouse.room.GetWorldCoordinate(pos));
                        this.idlePosCounter = Random.Range(200, 1900);
                    }
                }
                if (!this.mouse.sitting)
                {
                    this.idlePosCounter--;
                }
                this.idlePosCounter--;
            }
            else if (behavior == Behavior.Flee)
            {
                this.mouse.runSpeed = Mathf.Lerp(this.mouse.runSpeed, 2f, 0.08f);
                this.creature.abstractAI.SetDestination(base.threatTracker.FleeTo(this.creature.pos, 6, 20, true));
            }
            else if (this.behavior == Behavior.ReturnPrey)
            {
                this.mouse.runSpeed = Mathf.Lerp(this.mouse.runSpeed, 1f, 0.08f);
                if (base.denFinder.GetDenPosition() != null)
                {
                    this.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
                }
            }
            else if (this.behavior == Behavior.EscapeRain)
            {
                this.mouse.runSpeed = Mathf.Lerp(this.mouse.runSpeed, 2f, 0.08f);
                if (base.denFinder.GetDenPosition() != null)
                {
                    this.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
                }
            }
            else if (this.behavior == Behavior.Hunt)
            {
                this.mouse.runSpeed = Mathf.Lerp(this.mouse.runSpeed, 1.2f, 0.08f);
                if (preyTracker.MostAttractivePrey != null)
                {
                    this.focusCreature = this.preyTracker.MostAttractivePrey;
                    // TODO Item Tracking and comparison.
                    //this.focusItem = this.FindBestFood();
                    this.creature.abstractAI.SetDestination(this.focusCreature.BestGuessForPosition());
                    if (this.focusCreature.representedCreature.realizedCreature != null && this.mouse.grasps[0] == null
                        && this.mouse.Footing && this.focusCreature.representedCreature.realizedCreature.room == this.mouse.room)
                    {
                        var chunk = this.focusCreature.representedCreature.realizedCreature.bodyChunks[Random.Range(0, this.focusCreature.representedCreature.realizedCreature.bodyChunks.Length)];
                        if (Custom.DistLess(this.mouse.mainBodyChunk.pos, chunk.pos, 120f) &&
                            (this.mouse.room.aimap.TileAccessibleToCreature(this.mouse.room.GetTilePosition(this.mouse.bodyChunks[1].pos - Custom.DirVec(this.mouse.bodyChunks[1].pos, chunk.pos) * 30f), this.mouse.Template) ||
                            this.mouse.room.GetTile(this.mouse.bodyChunks[1].pos - Custom.DirVec(this.mouse.bodyChunks[1].pos, chunk.pos) * 30f).Solid) &&
                            this.mouse.room.VisualContact(this.mouse.mainBodyChunk.pos, chunk.pos))
                        {
                            if (Vector2.Dot((this.mouse.mainBodyChunk.pos - chunk.pos).normalized, (this.mouse.bodyChunks[1].pos - this.mouse.mainBodyChunk.pos).normalized) > 0.2f)
                            {
                                this.mouse.InitiateJump(chunk);
                            }
                            else
                            {
                                this.mouse.mainBodyChunk.vel += Custom.DirVec(this.mouse.mainBodyChunk.pos, chunk.pos) * 2f;
                                this.mouse.bodyChunks[1].vel -= Custom.DirVec(this.mouse.mainBodyChunk.pos, chunk.pos) * 2f;
                            }
                            if (this.huntAttackCounter < 50)
                            {
                                this.huntAttackCounter++;
                                if (Custom.DistLess(this.mouse.mainBodyChunk.pos, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, 50f))
                                {
                                    this.mouse.TryToGrabPrey(preyTracker.MostAttractivePrey.representedCreature.realizedCreature);
                                }
                            }
                            else if (Random.value < 0.1f)
                            {
                                this.huntAttackCounter++;
                                if (this.huntAttackCounter > 200)
                                {
                                    this.huntAttackCounter = 0;
                                }
                            }
                        }
                        this.tiredOfHuntingCounter++;
                        if (this.tiredOfHuntingCounter > 200)
                        {
                            this.tiredOfHuntingCreature = base.preyTracker.MostAttractivePrey.representedCreature;
                            this.tiredOfHuntingCounter = 0;
                            base.preyTracker.ForgetPrey(this.tiredOfHuntingCreature);
                            base.tracker.ForgetCreature(this.tiredOfHuntingCreature);
                            return;
                        }
                    }
                }
            }
            if (this.behavior == Behavior.Flee)
            {
                this.fear = Mathf.Lerp(this.fear, Mathf.Pow(base.threatTracker.Panic, 0.7f), 0.5f);
            }
            else
            {
                this.fear = Mathf.Max(this.fear - 0.0125f, 0f);
            }
            if (this.fear > 0.7f)
            {
                this.mouse.Ignite(null);
            }
            if (this.mouse.graphicsModule.DEBUGLABELS != null)
            {
                this.mouse.graphicsModule.DEBUGLABELS[0].label.text = "fear: " + this.fear + "    brn:" + this.mouse.burning;
                this.mouse.graphicsModule.DEBUGLABELS[1].label.text = "behav: " + this.behavior + "    pry:" + this.preyTracker.Utility();
            }
        }

        private ItemTracker.ItemRepresentation FindBestFood()
        {
            ItemTracker.ItemRepresentation bestItem = null;
            float bestScore = 0;
            for (int i = 0; i < base.itemTracker.ItemCount; i++)
            {
                var item = base.itemTracker.GetRep(i);
                var realized = base.itemTracker.GetRep(i).representedItem.realizedObject;
                float score = Vector2.Distance(this.mouse.mainBodyChunk.pos, this.mouse.room.MiddleOfTile(item.BestGuessForPosition()));
                if (realized != null && realized.grabbedBy.Count > 0)
                {
                    score = score * 2f + 20f;
                }
                if (realized != null && item.VisualContact)
                {
                    score -= Vector2.Distance(this.mouse.mainBodyChunk.pos, this.mouse.room.MiddleOfTile(realized.firstChunk.pos)) / 2;
                }
                if (!this.pathFinder.CoordinateReachableAndGetbackable(item.BestGuessForPosition()))
                {
                    score += 300;
                }
                if (score < bestScore)
                {
                    bestItem = item;
                    bestScore = score;
                }
            }
            return bestItem;
        }

        public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
        {
            if (this.mouse.graphicsModule != null)
            {
                (this.mouse.graphicsModule as CandleGraphics).creatureLooker.ReevaluateLookObject(otherCreature, 2f);
            }
        }

        public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return new RelationshipTracker.TrackedCreatureState();
        }

        public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            {
                return base.threatTracker;
            }
            if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
            {
                return base.preyTracker;
            }
            return null;
        }

        public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if (dRelation.trackerRep.VisualContact)
            {
                dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
            }
            CreatureTemplate.Relationship relationship = base.StaticRelationship(dRelation.trackerRep.representedCreature);
            if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            {
                if (!dRelation.state.alive)
                {
                    relationship.intensity = 0f;
                }
                else if (dRelation.trackerRep.BestGuessForPosition().room == this.mouse.room.abstractRoom.index && !dRelation.trackerRep.representedCreature.creatureTemplate.canFly)
                {
                    float num = Mathf.Lerp(0.1f, 1.6f, Mathf.InverseLerp(-100f, 200f, this.mouse.room.MiddleOfTile(dRelation.trackerRep.BestGuessForPosition().Tile).y - this.mouse.mainBodyChunk.pos.y));
                    float value = float.MaxValue;
                    num = Mathf.Lerp(num, 1f, Mathf.InverseLerp(50f, 500f, value));
                    relationship.intensity *= num;
                }
            }
            return relationship;
        }

        public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
        {
            if (this.behavior != Behavior.Flee)
            {
                return cost;
            }
            return new PathCost(cost.resistance + base.threatTracker.ThreatOfTile(coord.destinationCoord, true) * 100f, cost.legality);
        }

        public bool TrackItem(AbstractPhysicalObject obj)
        {
            return obj is IPlayerEdible;
        }

        public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
        {
        }

        public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
        {
            if (this.noiseRectionDelay > 0)
            {
                return;
            }
            (this.mouse.graphicsModule as CandleGraphics).creatureLooker.ReevaluateLookObject(source.creatureRep, 1.5f);
            this.noiseRectionDelay = Random.Range(0, 25);
        }

        public class Behavior : ExtEnum<Behavior>
        {
            public Behavior(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly Behavior Idle = new Behavior("Idle", true);

            public static readonly Behavior Flee = new Behavior("Flee", true);

            public static readonly Behavior Hunt = new Behavior("Hunt", true);

            public static readonly Behavior EscapeRain = new Behavior("EscapeRain", true);

            public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", true);
        }
    }
}
