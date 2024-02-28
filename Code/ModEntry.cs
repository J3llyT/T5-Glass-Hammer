using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Buffs;


using StardewValley.GameData.LocationContexts;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace HopeToRiseMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private int staminaThreshold = 3; // Adjust this value as needed

        private bool bossSpawned = false;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameLocation.RegisterTouchAction("poison", GiveBuff);
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }
        private void GiveBuff(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            Buff buff = new Buff(
                id: "poison",
                displayName: "poison",
                iconTexture: this.Helper.ModContent.Load<Texture2D>("assets/poison.png"),
                iconSheetIndex: 0,
                duration: 5_000,
                effects: new BuffEffects()
                {
                    Speed = { -10 }
                }
            );

            player.applyBuff(buff);

            Monitor.Log("asdhsahedajskhdejkawedhjkawehdkwa");

        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (Game1.player.stamina <= staminaThreshold && Game1.currentLocation != null
                && Game1.currentLocation.Name == "DreamWorldSpawn")
            {
                Monitor.Log("Player Passed Out!", LogLevel.Info);

                // Display a blackout screen
                Game1.showGlobalMessage("You're waking up...");

                // Teleport the player to their last slept-in bed location (NEED TO FIND BED LOCATION)
                Game1.warpHome();
                // WarpPlayerToNewLocation("FarmHouse", (int)Game1.player.mostRecentBed.X, (int)Game1.player.mostRecentBed.Y);
                // Game1.player.setTileLocation(Game1.player.mostRecentBed);


                // Reset the player's stamina to prevent further passouts
                Game1.player.stamina = Game1.player.MaxStamina;
            }

            // Spawn in a boss if the player is in the boss arena and there is no boss spawned
            if (Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldBoss" && !bossSpawned)
            {
                bossSpawned = true;
                
                Monster somnia = new SquidKid(new Vector2(15f, 15f) * 64f);
                Game1.currentLocation.characters.Add(somnia);
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Handles sleep warp logic. (CHANGE ITEM ID TO TEDDY BEAR)
            if (Game1.player.isInBed.Value && Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.lastQuestionKey.StartsWith("Sleep")
                && Game1.player.ActiveObject != null && Game1.player.ActiveObject.ParentSheetIndex.Equals(69))
            {
                Game1.currentLocation.afterQuestion += SleepWarp;
            }
        }

        // Warps player to new location.
        private void WarpPlayerToNewLocation(string locationName, int xCoord, int yCoord)
        {
            Game1.warpFarmer(locationName, xCoord, yCoord, false);
        }

        // Checks to see if player clicked 'Yes' to sleep.
        public void SleepWarp(Farmer who, string whichAnswer)
        {
            if (whichAnswer is "Yes")
            {
                WarpPlayerToNewLocation("dreamworldspawn", 4, 4);
            }
        }
    }
}
