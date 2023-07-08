using System.Collections.Generic;
using HUD;
using UnityEngine;

namespace Deadlands;

public class HydrationMeter : HudPart // i have no idea what im doing
{
    public static void AddToHud(int maxHydration)
    {
        On.HUD.HUD.InitSinglePlayerHud += (orig, self, cam) =>
        {
            orig(self, cam);
            self.AddPart(new HydrationMeter(self, self.fContainers[1], maxHydration));
        };
    }

    public Vector2 pos;
    public Vector2 lastPos;
    public int remainVisibleCounter;
    public float fade;
    public float lastFade;
    public HUDCircle[] circles;


    public HydrationMeter(HUD.HUD hud, FContainer fContainer, int maxHydration) : base(hud)
    {
        pos = new Vector2(80f, 20f);
        lastPos = pos;
        circles = new HUDCircle[maxHydration];
        fade = 0.0f;
        lastFade = 0.0f;
        for (int index = 0; index < circles.Length; ++index)
        {
            circles[index] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0)
            {
                fade = 0.0f,
                lastFade = 0.0f
            };
        }
    }

    private bool Show => hud.showKarmaFoodRain || hud.owner.RevealMap;

    public override void Update()
    {
        float hydration = (hud.owner as Player).GetHydration();
        if (hud.foodMeter != null)
        { //this is copied from the saint hypothermia, im not gonna touch it
            pos.x = hud.foodMeter.pos.x;
            fade = hydration > 0.05 ||
                   !(hud.rainWorld.processManager.currentMainLoop is RainWorldGame game) ||
                   game?.cameras[0].room == null ||
                   !(game?.cameras[0].room.roomSettings.DangerType !=
                     DangerTypeHeat.Heat)
                ? Mathf.Lerp(fade, Show ? hud.foodMeter.fade : 0.0f,
                    fade < (double)hud.foodMeter.fade ? 0.15f : 0.005f)
                : Mathf.Lerp(fade, 0.0f, 0.1f);
        }

        lastPos = pos;
        lastFade = fade;
        if (hud.HideGeneralHud)
            fade = 0.0f;
        float a = 1f / circles.Length;

        for (int index = 0; index < circles.Length; ++index)
        {
            bool isFull = hydration > index;
            circles[index].Update();
            circles[index].thickness = isFull ? 6 : 1;
            circles[index].fade = Mathf.Lerp(circles[index].fade,
                Mathf.InverseLerp(a * index, a * (index + 1f), fade), 0.1f);
            circles[index].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
            circles[index].snapRad = 0.45f;
            circles[index].snapThickness = 0.45f;
            circles[index].rad = Mathf.Lerp(0.1f, 5f, circles[index].fade);
            circles[index].pos = pos + new Vector2(index * 21.6f, 0);
        }
    }

    public Vector2 DrawPos(float timeStacker) => Vector2.Lerp(lastPos, pos, timeStacker);

    public override void Draw(float timeStacker)
    {
        foreach (var circle in circles)
            circle.Draw(timeStacker);
    }
}