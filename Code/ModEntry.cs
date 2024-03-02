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

using HopeToRiseMod.Monsters;

namespace HopeToRiseMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private int staminaThreshold = 3; // Adjust this value as needed

        private bool bossSpawned = false;
        private bool poisonTilesSpawned = false;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameLocation.RegisterTouchAction("poison", GiveBuff);
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }
        

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            #region // Warp Logic
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
            #endregion

            PlayerLocation();

            #region // Boss Logic
            // Spawn in a boss if the player is in the boss arena and there is no boss spawned
            if (Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldBoss" && !bossSpawned)
            {
                bossSpawned = true;
                
                Monster somnia = new DreamLord(new Vector2(15f, 15f) * 64f);
                Game1.currentLocation.characters.Add(somnia);
            }
            #endregion

            #region // Tile Logic
            if (bossSpawned && Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldBoss" && !poisonTilesSpawned)
            {
                // Access the current game location
                GameLocation currentLocation = Game1.currentLocation;

                // Define the location and size of the target area
                int targetX = 15;
                int targetY = 12;
                int areaSize = 10;

                // Define the number of tiles to replace
                int numTilesToReplace = 75;

                // Randomly select tiles within the target area and replace them
                Random random = new Random();
                for (int i = 0; i < numTilesToReplace; i++)
                {
                    int randomX = targetX - areaSize + random.Next(2 * areaSize + 1);
                    int randomY = targetY - areaSize + random.Next(2 * areaSize + 1);

                    // Check if the randomly selected tile is within the target area
                    if (randomX >= targetX - areaSize && randomX <= targetX + areaSize && randomY >= targetY - areaSize && randomY <= targetY + areaSize)
                    {
                        // Change the tile at the randomly selected position
                        currentLocation.setMapTileIndex(randomX, randomY, 622, "Back");
                    }
                }

                poisonTilesSpawned = true;
            }
            #endregion
        }

        #region // Tile Methods
        private void GiveBuff(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            Buff buff = new Buff(
                id: "poison",
                displayName: "Poison",
                iconTexture: this.Helper.ModContent.Load<Texture2D>("../[CP] Hope to Rise/assets/PoisonBuff.png"),
                iconSheetIndex: 0,
                duration: 5_000,
                effects: new BuffEffects()
                {
                    Speed = { -5 },
                    Defense = { -3 }
                }
            );

            player.applyBuff(buff);
        }
        #endregion

        #region // Warp Methods
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
        #endregion

        private void PlayerLocation()
        {
            try
            {
                if (Game1.player == null || Game1.currentGameTime.TotalGameTime.TotalSeconds < 5)
                {
                    return;
                }

                GameLocation playerLocation = Game1.currentLocation;

                if (playerLocation == null)
                {
                    //Monitor.Log("Player location is null.", LogLevel.Warn);
                    return;
                }

                Vector2 playerTileCoordinates = Game1.player.getLocalPosition(Game1.viewport);

                if (playerTileCoordinates.X < 0 || playerTileCoordinates.Y < 0)
                {
                    Monitor.Log("Invalid player tile coordinates.", LogLevel.Warn);
                    return;
                }

                int tileSize = 64;
                Vector2 playerTile = new Vector2((int)(playerTileCoordinates.X / tileSize), (int)(playerTileCoordinates.Y / tileSize));

                string locationName = playerLocation.Name;
                //Monitor.Log($"Player is in {locationName}/{playerTileCoordinates}/{playerTile}/{Game1.player.position}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Exception in PlayerLocation: {ex}", LogLevel.Error);
            }
        }
    }

    // WATERING CAN INFO
    // VolcanoDungeon
    // OnLightningStrike
    // Ladders in mines
}
