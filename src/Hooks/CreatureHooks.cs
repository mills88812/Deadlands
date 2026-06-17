using Deadlands.Creatures.Buzzard;
using Deadlands.Creatures.CandleMouse;
using Deadlands.Creatures.Iguana;
using Deadlands.Creatures.SaltWorm;
using System.Collections.Generic;

namespace Deadlands.Hooks
{
    internal class CreatureHooks
    {
        // Hooks covering various aspects of the game involving creatures as well as individuals.

        // Whenever you add a new creature make sure you add it to all of the AbstractCreature and CreatureTemplate hooks like I've done.
        // self is not necessary if you're using a mod which does self for you such as Fisob or Lizard Customizer, in short, do what you need to do.

        // Make sure your thuroughly review any addidtions you make to prevent conflicts and ensure everything fits and works properly.

        // From a design perspective consider how a creature will adapt and survive in an enviornment with as big of a contrast to Rain World as the Deadlands
        // rather than just how it'll eat the player. Follow the same principles Rain World was built upon where every creature has the same agency as the player.

        // Deadlands Creatures

        // Buzzard: Vulture varient that is faster, slimmer and very aggressive and persistant.
        //          Primarally a scavenger but will persue living food and won't give up without a fight. Might be able to pick up and use spears and rocks too.
        // Iguana: 
        // Brown Lizard: 
        // Glow Lizard: 
        // SpinePlant: Monsterkelp like plant that uses a harpoon similar to a king vulture.
        //          If damaged critically it will eject its spines peircing most creatures nearby and dealing damage.
        //          This creature is not a varient of the MonsterKelp but is pretty similar so it uses its own code for everything.
        public static void Apply()
        {
            // Static World

            On.StaticWorld.InitCustomTemplates += OnInitCustomTemplates;
            On.StaticWorld.InitStaticWorld += OnInitStaticWorld;

            // WorldLoader

            On.WorldLoader.CreatureTypeFromString += OnCreatureTypeFromString;

            // AbstractCreature

            On.AbstractCreature.InitiateAI += OnInitiateAI;
            On.AbstractCreature.ctor += OnAbstractCreatureCtor;
            On.AbstractCreature.Update += OnAbstractCreatureUpdate;

            //IL.AbstractCreature.ctor += ILAbstractCreatureCtor;
            IL.AbstractCreature.Realize += ILRealize;

            // CreatureTemplate

            On.CreatureTemplate.ctor_Type_CreatureTemplate_List1_List1_Relationship += OnCreatureTemplateCtor;

            // CreatureSymbol

            On.CreatureSymbol.SpriteNameOfCreature += OnCreatureSymbolSpriteNameOfCreature;
            On.CreatureSymbol.ColorOfCreature += OnCreatureSymbolColorOfCreature;

            // Additional Debug

            // Creature Specific Hooks
            IguanaHook.Apply();
            BrownLizardHook.Apply();
            GlowLizardHook.Apply();
            BuzzardHooks.Apply();
            SaltWormHooks.Apply();
            //SpinePlantHooks.Apply();
        }

        #region StaticWorld
        private static void OnInitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
        {
            orig();

            List<TileTypeResistance> list2 = new();
            List<TileConnectionResistance> list3 = new();

            // Populate with CreatureTemplates for any creatures you're adding manually
            #region Buzzard

            CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture);

            if (creatureTemplate == null)
            {
                Debug.Log("Ancestor not found!");
            }

            list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
            list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 10f, PathCost.Legality.Allowed));
            CreatureTemplate template = (new CreatureTemplate(DLCreature.Buzzard, creatureTemplate, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
            template.baseDamageResistance = 9.5f;
            template.baseStunResistance = 7f;
            template.abstractedLaziness = 9;
            template.AI = true;
            template.requireAImap = true;
            template.canSwim = false;
            template.canFly = true;
            template.doPreBakedPathing = false;
            template.preBakedPathingAncestor = creatureTemplate;
            template.offScreenSpeed = 2.5f;
            template.bodySize = 5f;
            template.grasps = 1;
            template.stowFoodInDen = true;
            template.shortcutSegments = 5;
            template.visualRadius = 7000f;
            template.movementBasedVision = 0.4f;
            template.waterVision = 0.8f;
            template.throughSurfaceVision = 0.8f;
            template.hibernateOffScreen = true;
            template.dangerousToPlayer = 0.7f;
            template.meatPoints = 13;
            template.communityInfluence = 0.25f;
            template.socialMemory = true;
            template.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            template.jumpAction = "Fly";
            template.pickupAction = "Ensnare";
            template.throwAction = "Release";
            list2.Clear();
            list3.Clear();

            StaticWorld.creatureTemplates[DLCreature.Buzzard.Index] = template;
            #endregion

            #region Iguana
            CreatureTemplate Iguana = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate);

            if (Iguana == null)
            {
                Debug.Log("Ancestor not found!");
            }
            list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 0f, PathCost.Legality.Allowed));
            list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 0f, PathCost.Legality.IllegalTile));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 0f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 0f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 0f, PathCost.Legality.IllegalTile));
            CreatureTemplate creatureTemplate3 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.PinkLizard, Iguana, null, null, null);
            CreatureTemplate template2 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.PinkLizard, Iguana, creatureTemplate3, null, null);
            LizardBreedParams breedParams = ((LizardBreedParams)template2.breedParameters)!;
            template2.type = DLCreature.Iguana;
            breedParams.template = DLCreature.Iguana;
            breedParams.toughness = 1f;
            breedParams.stunToughness = 1f;
            breedParams.tamingDifficulty = 1f;
            template2.baseDamageResistance = breedParams.toughness * 2f;
            template2.baseStunResistance = breedParams.stunToughness;
            template2.abstractedLaziness = 8;
            template2.AI = true;
            template2.usesCreatureHoles = true;
            template2.usesNPCTransportation = true;
            template2.usesRegionTransportation = true;
            template2.doPreBakedPathing = false;
            template2.requireAImap = true;
            template2.preBakedPathingAncestor = Iguana;
            template2.stowFoodInDen = true;
            template.shortcutSegments = 3;
            template2.visualRadius = 1953.13f;
            breedParams.perfectVisionAngle = 1f;
            breedParams.periferalVisionAngle = 0.3f;
            breedParams.framesBetweenLookFocusChange = 80;
            template2.movementBasedVision = 0.55f;
            template2.waterVision = 0.4f;
            template2.throughSurfaceVision = 0.85f;
            template2.dangerousToPlayer = 0.45f;
            template2.meatPoints = 6;
            template2.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            template2.throwAction = "Cut Tail";
            breedParams.baseSpeed = 4.1f;
            breedParams.terrainSpeeds[1] = new(1.85f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(1.20f, 1f, 0.8f, 1f);
            breedParams.terrainSpeeds[4] = new(0.5f, 1f, 0.75f, 1f);
            breedParams.standardColor = new(.21f, .53f, .09f);
            breedParams.headSize = 1f;
            breedParams.headShieldAngle = 100f;
            breedParams.neckStiffness = 0.15f;
            breedParams.headGraphics = new int[]
            {
                       0,
                       0,
                       0,
                       6,
                       3
            };
            breedParams.jawOpenAngle = 90f;
            breedParams.jawOpenLowerJawFac = .67f;
            breedParams.jawOpenMoveJawsApart = 23f;
            breedParams.biteDominance = .6f;
            breedParams.getFreeBiteChance = .65f;
            breedParams.biteHomingSpeed = 1.15f;
            breedParams.biteRadBonus = 0f;
            breedParams.biteInFront = 25f;
            breedParams.biteDelay = 10;
            breedParams.attemptBiteRadius = 80f;
            breedParams.biteChance = .55f;
            breedParams.biteDamageChance = .43f;
            breedParams.biteDamage = 1.97f;
            breedParams.tailSegments = 10; // 4
            breedParams.tailStiffness = 100f;
            breedParams.tailStiffnessDecline = .2f;
            breedParams.tailLengthFactor = 1.40f;
            breedParams.tailColorationStart = .1f;
            breedParams.tailColorationExponent = 2f;
            //breedParams.bodyLengthFac = 1.2f;  1.37f
            //breedParams.bodyRadFac = 1.2f;   1f 0.28f
            breedParams.bodySizeFac = 1.2f; //1.1f
            breedParams.bodyMass = 4.4f; // 4.49f
            breedParams.bodyStiffnes = .15f;
            breedParams.swimSpeed = .2f;
            breedParams.wiggleDelay = 15;
            breedParams.wiggleSpeed = .5f;
            breedParams.maxMusclePower = 5f;
            breedParams.floorLeverage = .5f;
            breedParams.walkBob = 4f;
            breedParams.regainFootingCounter = 10;
            breedParams.legPairDisplacement = .2f;
            breedParams.limbGripDelay = 1;
            breedParams.limbQuickness = .5f;
            breedParams.limbSpeed = 5f;
            breedParams.noGripSpeed = .1f;
            breedParams.feetDown = .5f;
            breedParams.liftFeet = .3f;
            breedParams.stepLength = .5f;
            breedParams.limbQuickness = 1f;
            breedParams.limbSize = 1f;
            breedParams.canExitLounge = false;
            breedParams.canExitLoungeWarmUp = false;
            breedParams.findLoungeDirection = .5f;
            breedParams.preLoungeCrouch = 25;
            breedParams.preLoungeCrouchMovement = -.2f;
            breedParams.loungePropulsionFrames = 10;
            breedParams.loungeMaximumFrames = 20;
            breedParams.loungeJumpyness = .5f;
            breedParams.loungeDelay = 90;
            breedParams.riskOfDoubleLoungeDelay = .1f;
            breedParams.postLoungeStun = 20;
            breedParams.loungeDistance = 100f;
            breedParams.loungeTendensy = .1f;// 0f
            breedParams.loungeSpeed = 1.9f;
            list2.Clear();
            list3.Clear();
            StaticWorld.creatureTemplates[DLCreature.Iguana.Index] = template2;
            #endregion

            #region BrownLizard
            CreatureTemplate Brown = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate);

            if (Brown == null)
            {
                Debug.Log("Ancestor not found!");
            }
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 40f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.LizardTurn, 60f, PathCost.Legality.Allowed));
            CreatureTemplate creatureTemplate4 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.GreenLizard, Brown, null, null, null);
            CreatureTemplate template3 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.GreenLizard, Brown, creatureTemplate4, null, null);
            LizardBreedParams breedParams2 = ((LizardBreedParams)template3.breedParameters)!;
            template3.type = DLCreature.BrownLizard;
            breedParams2.template = DLCreature.BrownLizard;
            breedParams2.toughness = 2.8f;
            breedParams2.stunToughness = 2.8f;
            breedParams2.tamingDifficulty = 2.5f; //3.37f
            template3.baseDamageResistance = breedParams.toughness * 2f;
            template3.baseStunResistance = breedParams.stunToughness;
            template3.abstractedLaziness = 3;
            template3.AI = true;
            template3.usesCreatureHoles = true;
            template3.usesNPCTransportation = true;
            template3.usesRegionTransportation = true;
            template3.doPreBakedPathing = false;
            template3.requireAImap = true;
            template3.preBakedPathingAncestor = Brown;
            template3.stowFoodInDen = true;
            template.shortcutSegments = 3;
            template3.visualRadius = 2653.13f;
            breedParams2.perfectVisionAngle = 1f;
            breedParams2.periferalVisionAngle = 0.58f;
            breedParams2.framesBetweenLookFocusChange = 160;
            template3.movementBasedVision = 0.70f;
            template3.waterVision = 0.3f;
            template3.throughSurfaceVision = 0.85f;
            template3.dangerousToPlayer = 0.8f;
            template3.meatPoints = 8;
            template3.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            breedParams2.baseSpeed = 6f; // 4.97f
            breedParams2.terrainSpeeds[1] = new(1.43f, 1.77f, 1.41f, 2.07f); // new(2f, 2f, 1.7f, 2.5f);
            breedParams2.terrainSpeeds[3] = new(1f, 1.14f, 1f, 1.6f); //new(1.13f, 1.47f, 1.33f, 1.93f);
            breedParams2.terrainSpeeds[4] = new(1f, 1.42f, 1f, 1.78f);//new(1f, 1.57f, 1.15f, 1.93f);
            breedParams2.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            breedParams2.terrainSpeeds[6] = new(.1f, 1f, 1f, 1f);
            breedParams2.standardColor = new(.59f, .29f, .0f);
            breedParams2.headSize = 1.14f;
            breedParams2.headShieldAngle = 100f;
            breedParams2.neckStiffness = 0.10f;
            breedParams2.headGraphics = new int[]
            {
                       1,
                       1,
                       1,
                       1,
                       1
            };
            breedParams2.jawOpenAngle = 73.28f;
            breedParams2.jawOpenLowerJawFac = .5f;
            breedParams2.jawOpenMoveJawsApart = 14f;
            breedParams2.biteDominance = .75f;
            breedParams2.getFreeBiteChance = .45f;
            breedParams2.biteHomingSpeed = 2.08f;
            breedParams2.biteRadBonus = 17.66f;
            breedParams2.biteInFront = 40f;
            breedParams2.biteDelay = 31;
            breedParams2.attemptBiteRadius = 100f;
            breedParams2.biteChance = .53f;
            breedParams2.biteDamageChance = .75f;
            breedParams2.biteDamage = 2.3f; //5.37f
            breedParams2.tailSegments = 6;
            breedParams2.tailStiffness = 200f;
            breedParams2.tailStiffnessDecline = .72f;
            breedParams2.tailLengthFactor = .9f;
            breedParams2.tailColorationStart = 0.3f; //0f
            breedParams2.tailColorationExponent = 5f;
            breedParams2.bodyLengthFac = 1.25f;
            breedParams2.bodyRadFac = 1.5f;
            breedParams2.bodySizeFac = 1.25f; //1.25f
            breedParams2.bodyMass = 6.2f; // 7.05f
            breedParams2.bodyStiffnes = .08f;
            breedParams2.swimSpeed = 0f; //0.6f
            breedParams2.wiggleDelay = 58;
            breedParams2.wiggleSpeed = .47f;
            breedParams2.maxMusclePower = 16.75f;
            breedParams2.floorLeverage = 7f;
            breedParams2.smoothenLegMovement = false;
            breedParams2.walkBob = 2.15f;
            breedParams2.regainFootingCounter = 10;
            breedParams2.legPairDisplacement = .62f;
            breedParams2.limbGripDelay = 1;
            breedParams2.limbQuickness = .55f;
            breedParams2.limbSpeed = 5.77f;
            breedParams2.noGripSpeed = .05f;
            breedParams2.feetDown = 1f;
            breedParams2.liftFeet = .5f;
            breedParams2.stepLength = .9f;
            breedParams2.limbQuickness = 1f;
            breedParams2.limbSize = 1.19f; //1.11f
            breedParams2.canExitLounge = true;
            breedParams2.canExitLoungeWarmUp = false;
            breedParams2.findLoungeDirection = .25f; //.45f
            breedParams2.preLoungeCrouch = 50;
            breedParams2.preLoungeCrouchMovement = -.38f; // -.24f
            breedParams2.loungePropulsionFrames = 10; // 100
            breedParams2.loungeMaximumFrames = 60; //100
            breedParams2.loungeJumpyness = 0.25f; // 1.17f
            breedParams2.loungeDelay = 70; // 77
            breedParams2.riskOfDoubleLoungeDelay = .3f; //.21f
            breedParams2.postLoungeStun = 29;
            breedParams2.loungeDistance = 280f; //400
            breedParams2.loungeTendensy = 0.4f; //1f
            breedParams2.loungeSpeed = 5f;
            list2.Clear();
            list3.Clear();
            StaticWorld.creatureTemplates[DLCreature.BrownLizard.Index] = template3;
            #endregion

            #region GlowLizard
            CreatureTemplate Glow = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate);

            if (Glow == null)
            {
                Debug.Log("Ancestor not found!");
            }
            list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 0f, PathCost.Legality.Allowed));
            list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 0f, PathCost.Legality.IllegalTile));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 0f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 0f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 0f, PathCost.Legality.IllegalTile));
            CreatureTemplate creatureTemplate5 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.BlueLizard, Glow, null, null, null);
            CreatureTemplate template4 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.BlueLizard, Glow, creatureTemplate5, null, null);
            LizardBreedParams breedParams3 = ((LizardBreedParams)template4.breedParameters)!;
            template4.type = DLCreature.GlowLizard;
            breedParams3.template = DLCreature.GlowLizard;
            breedParams3.toughness = .5f;
            breedParams3.stunToughness = .1f;
            breedParams3.tamingDifficulty = 0.01f; //0f
            template4.baseDamageResistance = breedParams.toughness * 2f;
            template4.baseStunResistance = breedParams.stunToughness;
            template4.AI = true;
            template4.usesCreatureHoles = true;
            template4.usesNPCTransportation = true;
            template4.usesRegionTransportation = true;
            template4.doPreBakedPathing = false;
            template4.requireAImap = true;
            template4.preBakedPathingAncestor = Glow;
            template4.stowFoodInDen = true;
            template.shortcutSegments = 3;
            template4.visualRadius = 1400f;
            breedParams3.perfectVisionAngle = 0.83f;
            breedParams3.periferalVisionAngle = -0.6f;
            breedParams3.framesBetweenLookFocusChange = 130;
            template4.movementBasedVision = 0.3f;
            template4.waterVision = 0.25f;
            template4.throughSurfaceVision = 0.55f;
            template4.dangerousToPlayer = 0.05f;
            breedParams3.aggressionCurveExponent = 0.4f;
            template4.meatPoints = 2;
            template4.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            template4.throwAction = "Spit";
            breedParams3.baseSpeed = 4.1f; //5.1f
            breedParams3.terrainSpeeds[1] = new(1f, 1f, 1f, 1f); // Floor
            breedParams3.terrainSpeeds[3] = new(1.20f, 1f, 0.8f, 1f); // Corridor
            breedParams3.terrainSpeeds[4] = new(1.1f, 1f, 0.75f, 1f); // Climb (poles)
            breedParams3.terrainSpeeds[5] = new(.91f, 1f, 1f, 1f); // Wall
            breedParams3.terrainSpeeds[6] = new(0f, 1f, 1f, 1f); // Ceiling 
            breedParams3.standardColor = new(1f, .29f, .11f);
            breedParams3.headSize = 0.9f;
            breedParams3.headShieldAngle = 100f;
            breedParams3.neckStiffness = 0.2f;
            breedParams3.headGraphics = new int[]
            {
                       0,
                       0,
                       0,
                       0,
                       1
            };
            breedParams3.jawOpenAngle = 80f;
            breedParams3.jawOpenLowerJawFac = .67f;
            breedParams3.jawOpenMoveJawsApart = 23f;
            breedParams3.biteDominance = .15f;
            breedParams3.getFreeBiteChance = .85f;
            breedParams3.biteHomingSpeed = 4.6f;
            breedParams3.biteRadBonus = 0f;
            breedParams3.biteInFront = 15f;
            breedParams3.biteDelay = 18;
            breedParams3.attemptBiteRadius = 80f;
            breedParams3.biteChance = .31f;
            breedParams3.biteDamageChance = .25f; //.65f
            breedParams3.biteDamage = 0.18f;
            breedParams3.tailSegments = 5; //4
            breedParams3.tailStiffness = 20f;
            breedParams3.tailStiffnessDecline = .2f;
            breedParams3.tailLengthFactor = 1.80f;
            breedParams3.tailColorationStart = .8f;
            breedParams3.tailColorationExponent = 2f;
            //breedParams3.bodyLengthFac = 1f;
            //breedParams3.bodyRadFac = 1f;
            breedParams3.bodySizeFac = .8f; //0.74f
            breedParams3.bodyMass = 1f; //1.4f
            breedParams3.bodyStiffnes = .32f;
            breedParams3.swimSpeed = .3f;
            breedParams3.wiggleDelay = 7;
            breedParams3.wiggleSpeed = .8f;
            breedParams3.maxMusclePower = 9f;
            breedParams3.floorLeverage = .25f;
            breedParams3.walkBob = 4f; //9f
            breedParams3.regainFootingCounter = 7; //70
            breedParams3.legPairDisplacement = .8f;
            breedParams3.limbGripDelay = 2; //3
            breedParams3.limbQuickness = .2f;
            breedParams3.limbSpeed = 9f;
            breedParams3.noGripSpeed = .1f;
            breedParams3.feetDown = .2f;
            breedParams3.liftFeet = .6f;
            breedParams3.stepLength = .8f; //.8f
            breedParams3.limbThickness = 1f;
            breedParams3.limbSize = .7f; //.9f
            breedParams3.canExitLounge = true;
            breedParams3.canExitLoungeWarmUp = false;
            breedParams3.findLoungeDirection = .5f;
            breedParams3.preLoungeCrouch = 25;
            breedParams3.preLoungeCrouchMovement = -.7f;
            breedParams3.loungePropulsionFrames = 5;
            breedParams3.loungeMaximumFrames = 24;
            breedParams3.loungeJumpyness = .5f;
            breedParams3.loungeDelay = 90;
            breedParams3.riskOfDoubleLoungeDelay = .4f;
            breedParams3.postLoungeStun = 20;
            breedParams3.loungeDistance = 60f;
            breedParams3.loungeTendensy = .1f;
            breedParams3.loungeSpeed = 3.1f;
            template4.wormGrassImmune = true;
            list2.Clear();
            list3.Clear();
            StaticWorld.creatureTemplates[DLCreature.GlowLizard.Index] = template4;
            #endregion

            #region CandleMouse
            /*
            creatureTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature);
            bool flag2 = creatureTemplate == null;
            if (flag2)
            {
                Debug.Log("CandleMouse Ancestor not found!");
            }
            list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
            list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
            list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
            list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
            template = new CreatureTemplate(Type.CandleMouse, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
            template.baseDamageResistance = 0.4f;
            template.baseStunResistance = 0.3f;
            template.instantDeathDamageLimit = 1f;
            template.offScreenSpeed = 0.3f;
            template.abstractedLaziness = 50;
            template.AI = true;
            template.requireAImap = true;
            template.bodySize = 0.6f;
            template.stowFoodInDen = true;
            template.shortcutSegments = 2;
            template.preBakedPathingAncestor = creatureTemplate;
            template.grasps = 1;
            template.visualRadius = 800f;
            template.communityInfluence = 1f;
            template.dangerousToPlayer = 0.1f;
            template.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            template.meatPoints = 2;
            template.canSwim = true;
            template.BlizzardWanderer = false;
            template.BlizzardAdapted = false;
            template.jumpAction = "Sit/Sleep";
            template.pickupAction = "Flashbang";
            template.throwAction = "Squeak";
            template.usesNPCTransportation = true;
            list2.Clear();
            list3.Clear();
            StaticWorld.creatureTemplates[Type.CandleMouse.Index] = template;
            Debug.Log(StaticWorld.creatureTemplates[Type.CandleMouse.Index]);
            */
            #endregion

            #region SpinePlant
            /*
            CreatureTemplate spinePlant = new CreatureTemplate(Type.SpinePlant, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            spinePlant.baseDamageResistance = 3.5f;
            spinePlant.baseStunResistance = 1f;
            spinePlant.AI = true;
            spinePlant.requireAImap = false;
            spinePlant.doPreBakedPathing = false;
            spinePlant.stowFoodInDen = true;
            spinePlant.offScreenSpeed = 0f;
            spinePlant.bodySize = 2f;
            spinePlant.grasps = 1;
            spinePlant.visualRadius = 400f;
            spinePlant.movementBasedVision = 1f;
            spinePlant.waterVision = 0.7f;
            spinePlant.throughSurfaceVision = 0f;
            spinePlant.dangerousToPlayer = 0.5f;
            spinePlant.communityInfluence = 0.3f;
            spinePlant.wormGrassImmune = true;
            spinePlant.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            spinePlant.BlizzardWanderer = true;
            spinePlant.pickupAction = "Snatch";
            spinePlant.throwAction = "Harpoon";
            list2.Clear();
            list3.Clear();

            StaticWorld.creatureTemplates[Type.SpinePlant.Index] = spinePlant;
            */
            #endregion

            #region SaltWorm
            
            CreatureTemplate creatureTemplate15 = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centipede);

            if (creatureTemplate == null)
            {
                Debug.Log("Ancestor not found!");
            }

            CreatureTemplate saltWorm = new CreatureTemplate(DLCreature.SaltWorm, creatureTemplate15, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            saltWorm.baseDamageResistance = 1f;
            saltWorm.visualRadius = 1100f;
            saltWorm.communityInfluence = 0.25f;
            saltWorm.lungCapacity = 9900f;
            saltWorm.meatPoints = 1;
            saltWorm.pathingPreferencesTiles[9] = new PathCost(1f, PathCost.Legality.Allowed);
            saltWorm.shortcutColor = Color.white;
            saltWorm.shortcutSegments = 5;
            saltWorm.bodySize = 8.5f;
            saltWorm.dangerousToPlayer = 0.7f;
            saltWorm.BlizzardWanderer = false;
            saltWorm.BlizzardAdapted = false;
            saltWorm.jumpAction = "Swap Heads";
            saltWorm.pickupAction = "Grab/Smother";
            saltWorm.throwAction = "Release";
            list2.Clear();
            list3.Clear();

            StaticWorld.creatureTemplates[DLCreature.SaltWorm.Index] = saltWorm;
            #endregion


        }
        private static void OnInitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
        {

            orig();

            // Establish Relationships here
            #region Buzzard

            StaticWorld.EstablishRelationship(DLCreature.Buzzard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.55f));
            StaticWorld.EstablishRelationship(DLCreature.Buzzard, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.55f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.RedLizard, DLCreature.Buzzard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.4f));

            #endregion

            #region Iguana

            StaticWorld.EstablishRelationship(DLCreature.Iguana, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 0.5f));
            StaticWorld.EstablishRelationship(DLCreature.Iguana, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
            StaticWorld.EstablishRelationship(DLCreature.Iguana, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.1f));
            StaticWorld.EstablishRelationship(DLCreature.Iguana, DLCreature.Buzzard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.75f));
            StaticWorld.EstablishRelationship(DLCreature.Iguana, DLCreature.BrownLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.15f));
            StaticWorld.EstablishRelationship(DLCreature.Iguana, DLCreature.Iguana, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            #endregion

            #region BrownLizard

            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 0.5f));
            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.2f));
            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, DLCreature.Iguana, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.2f));
            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, DLCreature.Buzzard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.75f));
            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, DLCreature.BrownLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.85f));
            StaticWorld.EstablishRelationship(DLCreature.BrownLizard, DLCreature.GlowLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));

            #endregion

            #region GlowLizard

            StaticWorld.EstablishRelationship(DLCreature.GlowLizard, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 0.5f));
            StaticWorld.EstablishRelationship(DLCreature.GlowLizard, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.1f));
            StaticWorld.EstablishRelationship(DLCreature.GlowLizard, DLCreature.Iguana, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.35f));
            StaticWorld.EstablishRelationship(DLCreature.GlowLizard, DLCreature.BrownLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));

            #endregion

            #region CandleMouse

            StaticWorld.EstablishRelationship(DLCreature.Buzzard, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Fly, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.65f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.LizardTemplate, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Scavenger, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.SpitterSpider, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.LanternMouse, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.01f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.01f));

            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, DLCreature.CandleMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.01f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.01f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(DLCreature.CandleMouse, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));

            #endregion

            #region SaltWorm

            /*StaticWorld.EstablishRelationship(DLCreature.SaltWorm, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(DLCreature.SaltWorm, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));*/

            #endregion
            /*
            Plugin.Logger.LogFatal("OnInitStaticWorld SpinePlant");
            #region SpinePlant

            StaticWorld.EstablishRelationship(Type.SpinePlant, Type.SpinePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
            StaticWorld.EstablishRelationship(Type.SpinePlant, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Slugcat, Type.SpinePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            #endregion*/
            Plugin.Logger.LogFatal("OnInitStaticWorld SaltWorm");
            #region SaltWorm
            // Todo: Relationships
            #endregion
            
        }
        #endregion

        #region WorldLoader
        private static CreatureTemplate.Type OnCreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
        {
            // String Conversion Here

            if (s.Equals("buzzard", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.Buzzard;
            }
            if (s.Equals("iguana", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.Iguana;
            }
            if (s.Equals("brownlizard", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.BrownLizard;
            }
            if (s.Equals("glowlizard", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.GlowLizard;
            }
            if (s.Equals("spineplant", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.SpinePlant;
            }
            if (s.Equals("saltworm", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.SaltWorm;
            }
            if (s.Equals("candlemouse", StringComparison.OrdinalIgnoreCase))
            {
                return DLCreature.CandleMouse;
            }
            return orig(s);
        }
        #endregion

        #region AbstractCreature

        private static void OnAbstractCreatureUpdate(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);
            if (ModManager.MMF && !self.InDen && self.state.alive && (self.creatureTemplate.type == DLCreature.SpinePlant) && self.pos.abstractNode != -1 && self.GetNodeType != AbstractRoomNode.Type.Den)
            {
                self.destroyOnAbstraction = true;
                self.Die();
            }
        }

        private static void OnAbstractCreatureCtor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);

            // Creature States

            if (creatureTemplate.type == DLCreature.Buzzard && creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture)
            {
                self.state = new Vulture.VultureState(self);
            }
            if (creatureTemplate.type == DLCreature.Iguana && creatureTemplate.TopAncestor().type == CreatureTemplate.Type.PinkLizard)
            {
                self.state = new LizardState(self);
            }
            if (creatureTemplate.type == DLCreature.BrownLizard && creatureTemplate.TopAncestor().type == CreatureTemplate.Type.GreenLizard)
            {
                self.state = new LizardState(self);
            }
            if (creatureTemplate.type == DLCreature.GlowLizard && creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BlueLizard)
            {
                self.state = new LizardState(self);
            }
            if (creatureTemplate.type == DLCreature.CandleMouse)
            {
                self.state = new CandleState(self);
            }
            if (creatureTemplate.type == DLCreature.SaltWorm && creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede)
            {
                self.state = new Centipede.CentipedeState(self);
            }
            // Creature Abstract AI

            if (creatureTemplate.AI)
            {
                if (creatureTemplate.type == DLCreature.Buzzard)
                {
                    self.abstractAI = new VultureAbstractAI(world, self);
                }

                bool setDenPos = pos.abstractNode > -1 && pos.abstractNode < self.Room.nodes.Length
                    && self.Room.nodes[pos.abstractNode].type == AbstractRoomNode.Type.Den && !pos.TileDefined;

                if (setDenPos)
                {
                    self.abstractAI.denPosition = pos;
                }
            }

            // Extra Stuff

            if (creatureTemplate.type == DLCreature.SpinePlant)
            {
                self.remainInDenCounter = 0;
            }
        }

        private static void ILAbstractCreatureCtor(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = null;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(AbstractWorldEntity).GetMethod("get_Room")),
                x => x.MatchBrtrue(out postCheck),
                x => x.MatchLdarg(4)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<AbstractCreature>>((abstractCreature) =>
                {
                    // More Abstract AI if needed
                    if (abstractCreature.creatureTemplate.AI && abstractCreature.creatureTemplate.type == DLCreature.Buzzard)
                    {
                        abstractCreature.abstractAI = new VultureAbstractAI(abstractCreature.world, abstractCreature);
                    }
                });
            }
        }

        private static void ILRealize(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(AbstractCreature).GetMethod("InitiateAI"))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<AbstractCreature>>((abstractCreature) =>
                {
                    // Creature Realization
                    if (abstractCreature.creatureTemplate.TopAncestor().type == DLCreature.Buzzard)
                    {
                        abstractCreature.realizedCreature = new Vulture(abstractCreature, abstractCreature.world);
                    }
                    if (abstractCreature.creatureTemplate.TopAncestor().type == DLCreature.Iguana)
                    {
                        abstractCreature.realizedCreature = new Lizard(abstractCreature, abstractCreature.world);
                    }
                    if (abstractCreature.creatureTemplate.TopAncestor().type == DLCreature.GlowLizard)
                    {
                        abstractCreature.realizedCreature = new Lizard(abstractCreature, abstractCreature.world);
                    }
                    if (abstractCreature.creatureTemplate.TopAncestor().type == DLCreature.BrownLizard)
                    {
                        abstractCreature.realizedCreature = new Lizard(abstractCreature, abstractCreature.world);
                    }
                    if (abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TentaclePlant)
                    {
                        if (abstractCreature.creatureTemplate.type == DLCreature.SpinePlant)
                        {
                            abstractCreature.realizedCreature = new TentaclePlant(abstractCreature, abstractCreature.world);
                        }
                    }
                    if (abstractCreature.creatureTemplate.TopAncestor().type == DLCreature.SaltWorm)
                    {
                        abstractCreature.realizedCreature = new Centipede(abstractCreature, abstractCreature.world);
                    }
                    /*
                    if (abstractCreature.creatureTemplate.TopAncestor().type == Type.CandleMouse)
                    {
                        abstractCreature.realizedCreature = new CandleMouse(abstractCreature, abstractCreature.world);
                    }
                    */
                });
            }

        }

        private static void OnInitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
        {
            orig(self);
            if (self.creatureTemplate.type == DLCreature.Buzzard)
            {
                self.abstractAI.RealAI = new BuzzardAI(self, self.world);
            }
            if (self.creatureTemplate.type == DLCreature.Iguana)
            {
                self.abstractAI.RealAI = new IguanaAI(self, self.world);
            }
            if (self.creatureTemplate.type == DLCreature.BrownLizard)
            {
                self.abstractAI.RealAI = new LizardAI(self, self.world);
            }
            if (self.creatureTemplate.type == DLCreature.GlowLizard)
            {
                self.abstractAI.RealAI = new LizardAI(self, self.world);
            }
            if (self.creatureTemplate.type == DLCreature.SaltWorm)
            {
                self.abstractAI.RealAI = new CentipedeAI(self, self.world);
            }
            /*
            if (self.creatureTemplate.type == Type.CandleMouse)
            {
                self.abstractAI.RealAI = new CandleAI(self, self.world);
            }
            */
        }

        #endregion

        #region CreatureTemplate

        private static void OnCreatureTemplateCtor(On.CreatureTemplate.orig_ctor_Type_CreatureTemplate_List1_List1_Relationship orig, CreatureTemplate self, CreatureTemplate.Type type, CreatureTemplate ancestor, List<TileTypeResistance> tileResistances, List<TileConnectionResistance> connectionResistances, CreatureTemplate.Relationship defaultRelationship)
        {
            // Creature Templates
            orig(self, type, ancestor, tileResistances, connectionResistances, defaultRelationship);
            if (type == DLCreature.Buzzard)
            {
                self.name = "Buzzard";
            }
            if (type == DLCreature.Iguana)
            {
                self.name = "Iguana";
            }
            if (type == DLCreature.BrownLizard)
            {
                self.name = "BrownLizard";
            }
            if (type == DLCreature.GlowLizard)
            {
                self.name = "GlowLizard";
            }
            if (type == DLCreature.SpinePlant)
            {
                self.name = "SpinePlant";
            }
            if (type == DLCreature.SaltWorm)
            {
                self.name = "SaltWorm";
            }
            if (type == DLCreature.CandleMouse)
            {
                self.name = "CandleMouse";
            }
        }

        #endregion

        #region CreatureSymbol

        private static string OnCreatureSymbolSpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
        {

            if (iconData.critType == DLCreature.Buzzard)
            {
                return "Kill_Buzzard";
            }
            if (iconData.critType == DLCreature.Iguana)
            {
                return "Kill_Iguana";
            }
            if (iconData.critType == DLCreature.BrownLizard)
            {
                return "Kill_Brown";
            }
            if (iconData.critType == DLCreature.GlowLizard)
            {
                return "Kill_Standard_Lizard";
            }
            if (iconData.critType == DLCreature.SpinePlant)
            {
                return "Kill_PoleMimic";
            }
            if (iconData.critType == DLCreature.SaltWorm)
            {
                return "Kill_Centipede3";
            }
            if (iconData.critType == DLCreature.CandleMouse)
            {
                return "Kill_Mouse";
            }
            return orig(iconData);
        }

        private static Color OnCreatureSymbolColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (iconData.critType == DLCreature.Buzzard)
            {
                return new Color(0.897f, 0.385f, 0.464f);
            }
            if (iconData.critType == DLCreature.Iguana)
            {
                return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
            }
            if (iconData.critType == DLCreature.BrownLizard)
            {
                return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
            }
            if (iconData.critType == DLCreature.GlowLizard)
            {
                return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
            }
            return orig(iconData);
        }

        #endregion
    }
}
