using Deadlands.Enums;
using Deadlands.Features.Weather;
using Deadlands.Hooks.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Deadlands.Features
{
    internal class RoomWeatherModule
    {
        public bool ActiveSandstorm
        {
            get { return roomSandstorm != null; }
        }

        public Room Owner
        {
            get
            {
                roomRef.TryGetTarget(out var room);
                return room;
            }
        }

        public WeakReference<Room> roomRef;
        public Sandstorm? sandstorm;
        public Sandstorm? roomSandstorm;
        public RoomWeatherModule(Room room)
        {
            roomRef = new(room);
        }

        internal void CameraUpdate(RoomCamera rCam)
        {
            string str = "";
            str += "CameraUpdate ";
            str += "with roomRef: " + Owner.abstractRoom.name;
            str += " ss: " + this.sandstorm;
            str += " DT: " + Owner.roomSettings.DangerType;
            WorldHooks.TESTS[0] = str;
            if (this.sandstorm == null && Owner != null && (Owner.roomSettings.DangerType == DeadlandsEnums.Sandstorm || Owner.roomSettings.DangerType == DeadlandsEnums.DesertAndSandstorm))
            {
                WorldHooks.TESTS[2] = "Sandstorm Made";
                this.sandstorm = new Sandstorm(rCam, 0f);
            }
            if (this.sandstorm != null && Owner != null && (Owner.roomSettings.DangerType != DeadlandsEnums.Sandstorm && Owner.roomSettings.DangerType != DeadlandsEnums.DesertAndSandstorm))
            {
                this.sandstorm = null;
                if (roomSandstorm != null)
                {
                    Owner.RemoveObject(roomSandstorm);
                    roomSandstorm.Destroy();
                    roomSandstorm = null;
                }
            }
        }

        internal void CameraDrawUpdate(RoomCamera rCam)
        {
            WorldHooks.TESTS[1] = "CameraDrawUpdate";
            if (this.sandstorm != null && Owner != null && roomSandstorm == null)
            {
                Owner.AddObject(this.sandstorm);
                roomSandstorm = this.sandstorm;
            }
        }
    }
}
