using Deadlands.Enums;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Deadlands.Features
{
    internal class SunShade : UpdatableAndDeletable, IDrawable
    {
        private float amount;
        public SunShade(Room room)
        {
            this.room = room;
            this.amount = 0.003921569f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room == null)
            {
                return;
            }
            foreach (var effect in room.roomSettings.effects)
            {
                if (effect.type == DeadlandsEnums.SunShade)
                {
                    this.amount = effect.amount;
                }
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = rCam.pos - room.cameraPositions[rCam.currentCameraPosition];
            Vector2 vector2 = room.game.rainWorld.options.ScreenSize * 0.5f;
            sLeaser.sprites[0].x = vector2.x - vector.x;
            sLeaser.sprites[0].y = vector2.y - vector.y;
            sLeaser.sprites[0].alpha = amount;
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].anchorX = 0.5f;
            sLeaser.sprites[0].anchorY = 0.5f;
            sLeaser.sprites[0].scaleX = 100f;
            sLeaser.sprites[0].scaleY = 100f;
            sLeaser.sprites[0].color = new Color(0.003921569f, 0f, 0f);
            sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FlatLightNoFrag"];
            this.AddToContainer(sLeaser, rCam, null);
        }
    }
}
