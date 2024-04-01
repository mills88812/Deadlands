namespace Deadlands;

/// <summary>
/// Aiming for a combination between DustWave and BlizzardGraphics
/// </summary>
public class Sandstorm(RoomCamera rCam, float angle) : CosmeticSprite, IDrawable
{
    public RoomCamera rCam = rCam;

    //public List<RoofTopView.DustWave> dustWaves;

    public float windStrength;
    public float oldWindStrength;

    //public float intensity;
    //public float oldIntensity;
    public float windAngle = angle;

    public float oldWindAngle;

    public float lastmPos;
    public float mPos;

    public Material mat = new(rCam.game.rainWorld.Shaders["DustFlowRenderer"].shader);
    public Texture2D tex;
    private RenderTexture render;
    private RenderTexture render2;

    public float Progress
    {
        get
        {
            if (rCam.room == null)
            {
                return 1f;
            }
            return rCam.room.world.rainCycle.TimeUntilRain * rCam.room.roomSettings.RainIntensity;
        }
    }

    public float Intensity
    {
        get
        {
            if (rCam.room == null)
            {
                return 1f;
            }
            if (Progress < 0f)
            {
                return Mathf.InverseLerp(-300f, -1500f, Progress);
            }
            return Mathf.InverseLerp(300f, 1500f, Progress);
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
        rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[1]);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = rCam.pos - room.cameraPositions[this.rCam.currentCameraPosition];
        Vector2 vector2 = room.game.rainWorld.options.ScreenSize * 0.5f;
        sLeaser.sprites[0].x = vector2.x - vector.x;
        sLeaser.sprites[0].y = vector2.y - vector.y;
        sLeaser.sprites[0].color = new Color(Mathf.Lerp(lastmPos, mPos, timeStacker), 0f, 0f);
        sLeaser.sprites[0].isVisible = mPos < 1f;
        sLeaser.sprites[1].x = vector2.x - vector.x + 100f;
        sLeaser.sprites[1].y = vector2.y - vector.y + 100f;
        sLeaser.sprites[1].color = new Color(Mathf.Lerp(lastmPos, mPos, timeStacker), 0f, 0f);
        sLeaser.sprites[1].isVisible = mPos < 1f;
        sLeaser.sprites[1].alpha = 1f - Mathf.Lerp(lastmPos, mPos, timeStacker);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("Futile_White", true)
        {
            anchorX = 0.5f,
            anchorY = 0.5f,
            scaleX = 100f,
            scaleY = 100f,
            shader = room.game.rainWorld.Shaders[room.game.rainWorld.options.quality == Options.Quality.LOW ? "DustWaveLevelLow" : "DustWaveLevel"],
            isVisible = mPos > 0f
        };

        sLeaser.sprites[1] = new FSprite("Futile_White", true)
        {
            anchorX = 0.5f,
            anchorY = 0.5f,
            scaleX = 150f,
            scaleY = 150f,
            shader = room.game.rainWorld.Shaders[room.game.rainWorld.options.quality == Options.Quality.LOW ? "DustWaveLevelLow" : "DustWaveLevel"],
            isVisible = mPos > 0f
        };

        RenderFlowMap();
        AddToContainer(sLeaser, rCam, null);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        Vector4 value = new(rCam.sSize.x / ((rCam.room.TileWidth + 40f) * 20f) * (1366f / rCam.sSize.x) * 1.02f, rCam.sSize.y / ((rCam.room.TileHeight + 40f) * 20f) * 1.04f, (rCam.room.cameraPositions[rCam.currentCameraPosition].x + 400f) / ((rCam.room.TileWidth + 40f) * 20f), (rCam.room.cameraPositions[rCam.currentCameraPosition].y + 400f) / ((rCam.room.TileHeight + 40f) * 20f));
        Shader.SetGlobalVector("_tileCorrection", value);
        lastmPos = mPos;
        mPos = Intensity;
    }

    private void RenderFlowMap()
    {
        tex = new Texture2D(rCam.room.TileWidth, rCam.room.TileHeight)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int i = 0; i < rCam.room.TileWidth; i++)
        {
            for (int j = rCam.room.TileHeight - 1; j >= 0; j--)
            {
                bool flag = true;
                if (rCam.room.GetTile(i, j).Solid)
                {
                    flag = false;
                }
                tex.SetPixel(i, j, !flag ? new Color(0f, 0f, 0f) : new Color(1f, 0f, 0f));
            }
        }
        tex.Apply();
        mat.SetFloat("_topBottom", windAngle);
        render = new RenderTexture(tex.width + 40, tex.height + 40, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        render2 = new RenderTexture(tex.width + 40, tex.height + 40, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        mat.SetTexture("_MainTex", render);
        Shader.SetGlobalTexture("_Original", tex);
        mat.SetFloat("_firstPass", 1f);
        mat.SetFloat("_step", 0f);
        Graphics.Blit(tex, render, mat);
        mat.SetFloat("_firstPass", 0.6f);
        for (int k = 0; k < 127; k++)
        {
            mat.SetFloat("_step", k * 2f / 256f);
            Graphics.Blit(render, render2, mat);
            mat.SetFloat("_firstPass", 0.4f);
            mat.SetFloat("_step", (k * 2f + 1f) / 256f);
            Graphics.Blit(render2, render, mat);
        }
        mat.SetFloat("_firstPass", 0f);
        Graphics.Blit(render, render2, mat);
        render.filterMode = FilterMode.Bilinear;
        render2.filterMode = FilterMode.Bilinear;
        Shader.SetGlobalTexture("_DustFlowTex", render2);
        tex = new Texture2D(render2.width, render2.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0f, 0f, render2.width, render2.height), 0, 0);
        tex.Apply();
    }

    public float GetWindPixel(Vector2 pos)
    {
        if (tex != null)
        {
            pos.x /= 20f;
            pos.y /= 20f;
            return tex.GetPixel((int)pos.x + 20, (int)pos.y + 20).g - 0.05f;
        }
        return 0f;
    }
}