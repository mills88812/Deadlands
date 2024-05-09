namespace Deadlands;

/// <summary>
/// Stores the graphics data for the Wings of a Nomad slugcat.
/// </summary>
/// <param name="owner">The main body of the slugcat, used for determining head/body/tail positioning.</param>
/// <param name="startSprite">The index into the <see cref="RoomCamera.SpriteLeaser"/>'s array where the Wings store their sprites.</param>
internal sealed class Wings(PlayerGraphics owner, int startSprite, float size = 0.1f, float pointiness = 0.7f)
{
    private readonly PlayerGraphics _pGraphics = owner;

    private readonly int _startSprite = startSprite;

    private readonly float _size = size;
    private readonly float _pointiness = pointiness; // Ranges from 0 - 1, one being max, zero being no pointiness

    private float _wind;

    public const int RequiredSprites = 2;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Debug.Log(sLeaser.sprites.Length);

        for (int i = 0; i < 2; i++)
        {
            Debug.Log(_startSprite + i);
            sLeaser.sprites[_startSprite + i] = Utils.CreateSimpleMesh();
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos, NomadData nomadData)
    {
        _wind = nomadData.GlideSpeed;

        for (int i = 0; i < 2; i++)
        {
            PlaceWingVerts(sLeaser, timeStacker, camPos, _startSprite + i, i);
        }
    }

    private void PlaceWingVerts(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos, int startSprite, int wingIndex)
    {
        sLeaser.sprites[_startSprite].color = Color.cyan;
        sLeaser.sprites[_startSprite + 1].color = Color.red;
        /////////////////////////////////////
        // Base vertices
        /////////////////////////////////////

        Vector2 shoulder = Vector2.Lerp(_pGraphics.owner.bodyChunks[0].lastPos, _pGraphics.owner.bodyChunks[0].pos,
            timeStacker);
        Vector2 hip = Vector2.Lerp(_pGraphics.owner.bodyChunks[1].lastPos, _pGraphics.owner.bodyChunks[1].pos,
            timeStacker);

        // All Vector2s initalized with the keyword "var" in this context are not vertices
        var shoulderHipDiff = (shoulder - hip).normalized;

        Vector2 hand = Vector2.Lerp(_pGraphics.hands[wingIndex].lastPos, _pGraphics.hands[wingIndex].pos, timeStacker)
                       + 2 * shoulderHipDiff;

        /////////////////////////////////////
        // Inbetween vertices
        /////////////////////////////////////

        var handShoulderDist = 0.1f * Vector2.Distance(hand, shoulder);
        handShoulderDist = Mathf.Min(handShoulderDist, 50); // Should never go above this (happens when throwing)

        Vector2 handShoulderInbetween = Vector2.Lerp(shoulder, hand, 0.5f); // Halfway between hand and shoulder

        shoulder += 3 * Vector2.up; // Move up by 3 ...pixels?

        var handHipsDiff = wingIndex == 0 ? hip - hand : hand - hip; // Should be swapped for left and right wings
        float handHipsDiffMag = handHipsDiff.magnitude;

        handHipsDiff = handHipsDiff.normalized;

        var handHipsInbetween =
            Vector2.Lerp(hip, hand, 0.5f) + // Halfway between hips and hand
            _size * Mathf.Max(handShoulderDist * handHipsDiffMag, 65) * // Calculate magnitude, should never go above 30 (happens when throwing)
            new Vector2(handHipsDiff.y, -handHipsDiff.x); // rotate 90 degrees

        Vector2 handToInbetween =
            Vector2.Lerp(hand, handHipsInbetween, _pointiness); // Inbetween (From hand_shoulder_inbetween -- to hand
        Vector2 hipsToInbetween =
            Vector2.Lerp(hip, handHipsInbetween, _pointiness); // Inbetween (From hand_shoulder_inbetween -- to hips

        /////////////////////////////////////
        // Vertex assignment
        /////////////////////////////////////

        if (_wind >= 0.02f)
        {
            Vector2 windPos1 = SampleWindPos(_wind, wingIndex);
            Vector2 windPos2 = SampleWindPos(_wind, wingIndex + 0.6f);

            ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(1, 0.8f * windPos1 + handToInbetween - camPos);
            ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(3, 0.8f * windPos2 + hipsToInbetween - camPos);
        }
        else
        {
            ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(1, handToInbetween - camPos);
            ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(3, hipsToInbetween - camPos);
        }

        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(0, hand - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(2, handShoulderInbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(4, shoulder - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(5, hip - camPos);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < 2; i++)
            sLeaser.sprites[_startSprite + i].color = sLeaser.sprites[0].color;
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        for (int i = 0; i < 2; i++)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[_startSprite + i]);

            sLeaser.sprites[_startSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
        }
    }

    private Vector2 SampleWindPos(float s, float t)
    {
        float amplitude = Mathf.Sin(6) + Mathf.Sin(11.4f);

        float sos = Mathf.Cos(48 * Time.time - t) * amplitude * s * 1.5f;
        float cos = Mathf.Cos(sos + 15 * Time.time + t);
        float sin = Mathf.Sin(sos - 12 * Time.time - t);

        return 1.5f * amplitude * new Vector2(sin, cos);
    }
}