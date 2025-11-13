using MoreSlugcats;
using RWCustom;
using System;
using System;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DeadlandsCreatures.Features.Opal
{
    public class Opal : Weapon, IDrawable, IPlayerEdible
    {
        public AbstractConsumable AbstrConsumable
        {
            get
            {
                return abstractPhysicalObject as AbstractConsumable;
            }
        }
        public Opal.Stalk stalk;

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);

            if (base.firstChunk == null)
            {
                Debug.LogError("Opal.PlaceInRoom: firstChunk is null.");
                return;
            }

            bool arenaCase = false;
            try
            {
                arenaCase = ModManager.MMF && this.room != null && this.room.game != null && this.room.game.IsArenaSession
                            && (MMF.cfgSandboxItemStems.Value || this.room.game.GetArenaGameSession.chMeta != null)
                            && this.room.game.GetArenaGameSession.counter < 10;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Opal.PlaceInRoom: arena-case check failed: " + e);
                arenaCase = false;
            }

            if (arenaCase)
            {
                base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
                CreateAndAddStalkSafe(placeRoom, base.firstChunk.pos);
                return;
            }

            if (this.AbstrConsumable != null)
            {
                int idx = this.AbstrConsumable.placedObjectIndex;
                if (idx >= 0 && placeRoom.roomSettings != null && placeRoom.roomSettings.placedObjects != null
                    && idx < placeRoom.roomSettings.placedObjects.Count)
                {
                    base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[idx].pos);
                    CreateAndAddStalkSafe(placeRoom, base.firstChunk.pos);
                    return;
                }
                else
                {
                    Debug.LogWarning($"Opal.PlaceInRoom: placedObjectIndex invalid ({idx}) or roomSettings missing. Falling back to default placement.");
                }
            }
            else
            {
                Debug.LogWarning("Opal.PlaceInRoom: AbstrConsumable is null — falling back to default placement.");
            }

            base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
            this.rotation = Custom.RNV();
            this.lastRotation = this.rotation;
            CreateAndAddStalkSafe(placeRoom, base.firstChunk.pos);
        }

        private void CreateAndAddStalkSafe(Room placeRoom, Vector2 spawnPos)
        {
            try
            {
                if (this.stalk == null)
                {
                    this.stalk = new Opal.Stalk(this, placeRoom, spawnPos);
                    if (this.stalk != null)
                    {

                        placeRoom.AddObject(this.stalk);
                    }
                    else
                        Debug.LogWarning("Opal.PlaceInRoom: new Stalk returned null.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Opal.PlaceInRoom: failed to create/add stalk: " + e);
            }
        }



        public Vector2 rotation;

        public Vector2 lastRotation;

        public Vector2? setRotation;

        public float darkness;

        public float lastDarkness;


        public int bites = 3;
        public Opal(AbstractPhysicalObject abstractPhysicalObject, OpalAbstract abstr) : base(abstractPhysicalObject, abstr.world)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 14f, 0.2f);
            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.99999f;
            gravity = 0.9f;
            bounce = 0.3f;
            surfaceFriction = 0.7f;
            collisionLayer = 1;
            waterFriction = 0.95f;
            buoyancy = 1.1f;
        }



        public void ThrowByPlayer()
        {
        }

        public int FoodPoints
        {
            get
            {
                return 1;
            }
        }

        public bool Edible => true;

        public bool AutomaticPickUp
        {
            get
            {
                return false;
            }
        }

        public int BitesLeft
        {
            get
            {
                return bites;
            }
        }
        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound(bites == 0 ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();


                Destroy();
            }
        }
        private bool ConnectedToStalk;
        public override void Update(bool eu)
        {
            base.Update(eu);
            var fc = firstChunk;
            if (stalk != null && grabbedBy.Count == 0)
            {
                Vector2 targetPos = stalk.RootPos + (firstChunk.pos - stalk.RootPos).normalized * 0.1f;
                firstChunk.pos = targetPos;
                firstChunk.vel = Vector2.zero;
                rotation = Vector2.up;
                lastRotation = rotation;
            }
            else
            {
                if (stalk != null)
                {
                    stalk.DetachFruit();
                    stalk = null;
                }
                if (grabbedBy.Count > 0)
                {
                    rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                    rotation.y = Math.Abs(rotation.y);
                }
                if (setRotation is Vector2 v)
                {
                    rotation = v;
                    setRotation = null;
                }
                if (fc.ContactPoint.y < 0)
                {
                    rotation = (rotation - Custom.PerpendicularVector(rotation) * .1f * fc.vel.x).normalized;
                    fc.vel.x *= .8f;
                }
            }
          

        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            Futile.atlasManager.LoadImage("assets/Opal2");
            Futile.atlasManager.LoadImage("assets/Opal1");
            Futile.atlasManager.LoadImage("assets/Opal0");
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("assets/Opal1", true);
            sLeaser.sprites[1] = new FSprite("assets/Opal0", true);
            sLeaser.sprites[2] = new FSprite("assets/Opal2", true);
            AddToContainer(sLeaser, rCam, null);
        }
        public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {

            Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
            lastDarkness = darkness;
            darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
            if (darkness != lastDarkness)
            {
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            }
            for (int i = 0; i < 3; i++)
            {
                sLeaser.sprites[i].x = vector.x - camPos.x;
                sLeaser.sprites[i].y = vector.y - camPos.y;
                sLeaser.sprites[i].rotation = Custom.VecToDeg(v);
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("assets/Opal" + Custom.IntClamp(3 - bites, 0, 2).ToString());
            }
            if (blink > 0 && Random.value < 0.5f)
            {
                sLeaser.sprites[1].color = blinkColor;
            }
            else
            {
                sLeaser.sprites[1].color = color;
            }
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color White = new Color(0.9f, 0.9f, 0.9f, 0.60f);


            sLeaser.sprites[0].color = White;
            color = Color.Lerp(new Color(0.5f, 1f, 0.7f), palette.blackColor, darkness);
        }
        








        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);
        }
        public class Stalk : UpdatableAndDeletable, IDrawable
        {
            public Opal Fruit;
            public Vector2 RootPos;
            public FSprite stalkSprite;

            public Stalk(Opal fruit, Room room, Vector2 spawnPos)
            {
                Fruit = fruit;
                RootPos = spawnPos;
                base.room = room;
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                Futile.atlasManager.LoadImage("assets/OpalPlant");
                sLeaser.sprites = new FSprite[1];
                stalkSprite = new FSprite("assets/OpalPlant"); 
                sLeaser.sprites[0] = stalkSprite;
                AddToContainer(sLeaser, rCam, null);
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (stalkSprite == null)
                    return;

                if (Fruit == null)
                {
                    // Optionally hide stalk if fruit is gone
                    stalkSprite.isVisible = true;
                    return;
                }

                Vector2 mid = Vector2.Lerp(RootPos, Fruit.firstChunk.pos, 0.5f);
                stalkSprite.x = mid.x - camPos.x;
                stalkSprite.y = mid.y - camPos.y;

                Vector2 dir = Fruit.firstChunk.pos - RootPos;

                if (slatedForDeletetion || room != rCam.room)
                    sLeaser.CleanSpritesAndRemove();
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                if (stalkSprite != null)
                    stalkSprite.color = Color.white;
            }

            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer container)
            {
                container ??= rCam.ReturnFContainer("Background");
                container.AddChild(stalkSprite);
            }
            public void DetachFruit()
            {
                if (Fruit != null)
                {
                    Fruit.stalk = null;
                    Fruit = null;
                }
            }
        }




    }
}