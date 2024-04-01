namespace Deadlands;

public class RoomWeatherModule(Room room)
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

    public WeakReference<Room> roomRef = new(room);
    public Sandstorm sandstorm;
    public Sandstorm roomSandstorm;

    public void CameraUpdate(RoomCamera rCam)
    {
        string str = "";
        str += "CameraUpdate ";
        str += "with roomRef: " + Owner.abstractRoom.name;
        str += " ss: " + sandstorm;
        str += " DT: " + Owner.roomSettings.DangerType;
        WorldHooks.TESTS[0] = str;
        if (sandstorm == null && Owner != null && (Owner.roomSettings.DangerType == DeadlandsEnums.DangerType.Sandstorm || Owner.roomSettings.DangerType == DeadlandsEnums.DangerType.DesertAndSandstorm))
        {
            WorldHooks.TESTS[2] = "Sandstorm Made";
            sandstorm = new Sandstorm(rCam, 0f);
        }
        if (sandstorm != null && Owner != null && (Owner.roomSettings.DangerType != DeadlandsEnums.DangerType.Sandstorm && Owner.roomSettings.DangerType != DeadlandsEnums.DangerType.DesertAndSandstorm))
        {
            sandstorm = null;
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
        if (sandstorm != null && Owner != null && roomSandstorm == null)
        {
            Owner.AddObject(sandstorm);
            roomSandstorm = sandstorm;
        }
    }
}