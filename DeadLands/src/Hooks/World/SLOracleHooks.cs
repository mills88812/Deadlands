using Deadlands.Enums;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Deadlands.Hooks.World
{
    internal class SLOracleHooks
    {
        // None of this dialoge is final, just what I thought up at 3 AM, feel free to make changes
        // Depending on the direction this project goes lore wise Moon may or may not be capable of giving the mark of communication
        // This dialogue was written between Rivulet and Saint

        // --------------
        // Spoilers ahead
        // --------------

        public static void Apply()
        {
            // SLOracleBehaviorHasMark
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
        }
        
        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            Debug.Log("Current Save: " + self.currentSaveFile);
            Debug.Log("Nomad: " + Nomad.Nomad.Name);
            if (self.currentSaveFile != Nomad.Nomad.Name)
            {
                Debug.Log("Other Encounter SL AI");
                orig(self);
                return;
            }

            // methods by forthbridge

            var t = self.myBehavior.oracle.room.game.rainWorld.inGameTranslator;

            void SayNoLinger(string text) => self.events.Add(new Conversation.TextEvent(self, 0, t.Translate(text), 0));

            void Say(string text, int initialWait, int textLinger) => self.events.Add(new Conversation.TextEvent(self, initialWait, t.Translate(text), textLinger));

            void Wait(int initialWait) => self.events.Add(new Conversation.WaitEvent(self, initialWait));

            Debug.Log("Nomad Encounter SL AI");

            // To add new dialogue simply check the conversation ID and add your dialogue
            // Make sure you add a return statement afterward or the original dialogue will play right after

            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 5:
                        SayNoLinger("Hello little creature.");

                        SayNoLinger("I haven't had a visitor in quite a long time.");

                        SayNoLinger("I've seen your kind before but you are much different.");

                        SayNoLinger("You seem to be well adapted to Arid Climates, I didn't even think those existed anymore with how much the world changed...<LINE>I have no memory of what those enviornments were even like, all I know is the rainfall was more of a drizzle...");

                        SayNoLinger("Where do you come from little one?");

                        SayNoLinger("...");

                        SayNoLinger("If you do plan to stick around just be careful, please...");

                        SayNoLinger("The rainfall here will most certainly kill you if you don't find shelter.");

                        SayNoLinger("You're welcome to stay as long as you'd like.");
                        return;
                    default:
                        orig(self);
                        return;
                }
            }

            if (self.id == DeadlandsEnums.Moon_Pearl_UPGoodbye)
            {
                self.PearlIntro();

                Say(" . . . ", 30, 0);

                Say("<PlayerName>... Where did you find this?", 30, 0);

                Say(" . . . ", 30, 0);

                SayNoLinger("I'm sorry, I am just in complete disbelief that one of his pearls lasted this long...<LINE>The contents of it are even more unbelievable...");

                SayNoLinger("I guess it doesn't really matter at this point since the both of them are long gone.<LINE>The number of us has steadly declined with the passage of time.");

                Say(" . . . ", 30, 0);

                SayNoLinger("Ok... I just had to prepare myself a bit to read this.");

                SayNoLinger("\"I know you've forgotten me, but I could never forget you.<LINE>I've worked so hard to give this to you, I never could've guessed you wouldn't be here to see it.\"");

                SayNoLinger("\"It... It doesn't matter, I just needed it all to be said...<LINE>I love you, Sliver of Straw, goodbye.\"");

                Say("\"- Unbroken Promise\"", 30, 0);

                Say("... I can't even begin to describe how I feel after reading that.", 30, 0);

                if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                {
                    SayNoLinger("Thank you <PlayerName>, truely. I hope you've found peace after all you've gone through.");

                    SayNoLinger("You remind me of an old friend I had a while ago who was much like yourself...");

                    SayNoLinger("... The passage of time took them away, of course...<LINE>I miss them a lot...");

                    SayNoLinger("I shouldn't feel this way, but I do... Wish he could've gotten the same treatment as I did.");

                    Say(" . . . ", 30, 0);
                }

                SayNoLinger("May I please ask something of you?");

                SayNoLinger("Can you deliver this pearl to my neighbor Five Pebbles?");

                SayNoLinger("... Do you know Sliver of Straw? He was OBSESSED with Sliver Of Straw.<LINE>She was the only iterator to ever broadcast a signal that the solution to the Big Problem, the one that we were made to solve, had been found.");

                SayNoLinger("The triple affirmative - affirmative that a solution has been found, affirmative that the solution is portable, and affirmative that a technical implementation is possible and generally applicable.");

                SayNoLinger("Of course... She was also one of the first to be confirmed completely dead. First of what would eventually turn to most of us.<LINE>We do not die easily but the slow current of time takes us all eventually...");

                SayNoLinger("Anyways, Five Pebbles would love to see this. At this point he's in a considerably worse state than I am, left to rot from his tragic mistake...");

                SayNoLinger("He's completely alone and afraid, any cycle now could be his last. Give him my best wishes please.<LINE>Please be careful, you're not well adapted to the climate here.");

                return;
            }

            orig(self);
        }
    }
}
