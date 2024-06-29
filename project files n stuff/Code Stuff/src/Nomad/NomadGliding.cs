namespace Deadlands;

internal static class NomadGliding
{
    private const float MinGlideForce = 0.075f;
    private const float MaxGlideForce = 0.25f;

    public static readonly ConditionalWeakTable<Player, NomadData> NomadData = new();

    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;

        On.Player.ctor += (orig, self, creature, world) =>
        {
            orig(self, creature, world);

            NomadData.Add(self, new NomadData(self));

            if (!self.IsNomad()) return;

            if (self.room.world.game.IsArenaSession) return;

            ((PlayerState)self.State).slugcatCharacter = DeadlandsEnums.Nomad;
            self.SlugCatClass = DeadlandsEnums.Nomad;
        };
    }

    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        // Custom player Data
        if (!self.IsNomad()) return;
        if (!NomadData.TryGetValue(self, out var nomadData)) return;

        nomadData.SuperJumpDecay = Mathf.Max(nomadData.SuperJumpDecay - 1, 0);
        if (self.standing) nomadData.SuperJumpDecay = 0;
        nomadData.SuperJumpDecay = Mathf.Max(nomadData.SuperJumpDecay, self.superLaunchJump * 2);

        BodyChunk rootChunk = self.bodyChunks[1];

        do
        {
            var startHeight = nomadData.StartHeight;

            nomadData.GlideSpeed = 0;
            nomadData.StartHeight = rootChunk.pos.y;

            if (!self.input[0].jmp) break;

            if (self.animation != Player.AnimationIndex.None &&
                self.animation != Player.AnimationIndex.RocketJump) break; // Glide should only take effect during these two animations

            if (self.grasps.Any(g => g?.grabbed is Creature and not Fly and not SmallNeedleWorm)) break;

            nomadData.GlideSpeed = 0.01f; // Nomad can do wing splay anytime

            if (self.bodyMode != Player.BodyModeIndex.Default || // Glide should only take effect when airborne
                rootChunk.lastPos.y - rootChunk.pos.y < -1) break; // Glide should only take effect when falling

            nomadData.GlideSpeed = Mathf.Clamp((rootChunk.lastPos - rootChunk.pos).magnitude * 0.1f, 0.01f, 1);

            GlidePhysics(self);

            // Only accelerate if you've fallen far enough this glide
            nomadData.StartHeight = startHeight;
            if (nomadData.StartHeight - rootChunk.pos.y > 60f) GlideAcceleration(self);

            // Debug.Log($"Fallen: {nomadData.StartHeight - rootChunk.pos.y}");

            // Auto air-orient if you recently charge-pounced
            if (nomadData.SuperJumpDecay > 0)
                self.standing = true;
        }
        while (false);

        float vol = nomadData.windSound.Volume;

        if (nomadData.GlideSpeed > 0 && self.bodyMode == Player.BodyModeIndex.Default && rootChunk.lastPos.y - rootChunk.pos.y > -1)
            nomadData.windSound.Volume = Mathf.Min(vol + 0.3f, 1f * nomadData.GlideSpeed);
        else
            nomadData.windSound.Volume = Mathf.Max(vol - 0.5f, 0f);

        //Deprecated, used to update the sound but actually the sound is updated with the Mathf Min and Max, maybe should be changed to a lerp instead
        //nomadData.windSound.Update();
    }

    private static void GlidePhysics(Player player)
    {
        BodyChunk rootChunk = player.bodyChunks[1];

        rootChunk.vel.y = Mathf.Lerp(
            a: rootChunk.vel.y,
            b: 0,
            t: Mathf.Lerp(
                MinGlideForce,
                MaxGlideForce,
                (Mathf.Abs(rootChunk.vel.x) - 2.5f) * 0.2f) // Mapping the (2.5 - 7.5) range to (0.0 - 1.0)
        );

        // Player rotation
        rootChunk.vel.x -= 2 * player.input[0].x;
        player.firstChunk.vel.x += 2 * player.input[0].x;
    }

    private static void GlideAcceleration(Player player)
    {
        BodyChunk rootChunk = player.bodyChunks[1];

        // Acceleration
        if (Mathf.Abs(rootChunk.vel.x) < 7f)
            rootChunk.vel.x += player.input[0].x * 0.15f;
    }
}