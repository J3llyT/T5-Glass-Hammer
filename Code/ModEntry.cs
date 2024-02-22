using System;
using Microsoft.Xna.Framework;
using QuestFramework.Api;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;



namespace HopeToRiseMod
{
    public class ModEntry : Mod
    {
        /*
         * We can override these two methods later. Or I can create a testing folder and give this class access to it sow
         * we can get rid of them here. I'll put it on the back burner for now though.
         */
        private HTRQuest questItems;
        

        /*********
      ** Public methods
      *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameStarted;
            questItems = new HTRQuest();

        }

        /// <summary>
        //  Loads Quest when the game launches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameStarted(object sender, GameLaunchedEventArgs e)
        {
        }
    }
}
