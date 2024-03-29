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
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using xTile.Layers;


namespace HopeToRiseMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private int staminaThreshold = 3; // Adjust this value as needed

        private bool bossSpawned = false;
        private bool poisonTilesSpawned = false;

        private DreamLord somnia;

        private Vector2 lastMouseTile;

        Texture2D PoisonTile;
        Texture2D PoisonTileCooled;

        private bool isMouseLeftButtonDown = false;
        private int clicks = 0;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameLocation.RegisterTouchAction("poison", GiveBuff);
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            //for watering can logic
            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Input.ButtonPressed += LeftClick;
            helper.Events.Input.ButtonReleased += WateringPoisonRelease;

            //loading in textures
            PoisonTile = helper.ModContent.Load<Texture2D>("../[CP] Hope to Rise/assets/PoisonTile.png");
            //PoisonTileCooled = helper.ModContent.Load<Texture2D>("../[CP] Hope to Rise/assets/PoisonTileCooled.png");
        }
        

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            #region // Warp Logic
            // NEED TO UPDATE TO ACCOMADATE FOR ALL OF THE LOCATIONS
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
                
                somnia = new DreamLord(new Vector2(15f, 12f) * 64f);
                Game1.currentLocation.characters.Add(somnia);
            }
            if (Game1.currentLocation != null &&  Game1.currentLocation.characters.Contains(somnia))
            {
                //Monitor.Log(somnia.numHitsToStagger.ToString());
                //Monitor.Log(somnia.behavior.ToString());
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
                        currentLocation.setMapTileIndex(randomX, randomY, 923467, "Back");
                        //add the poison to the tile
                        

                        currentLocation.removeTile(randomX, randomY, "Back");

                        Layer layer = currentLocation.map.GetLayer("Back");
                        TileSheet tilesheet = currentLocation.map.GetTileSheet("z_PoisonTile");
                        layer.Tiles[randomX, randomY] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);

                        currentLocation.setTileProperty(randomX, randomY, "Back", "TouchAction", "poison");
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
            if (args.Length > 1 && args[1] == "deactivatePoison")
            {
                DeactivatePoisonTile((int)tile.X, (int)tile.Y);
            }
        }
        List<Vector2> tilesAffected(Vector2 tileLocation, int power, Farmer who)
        {
            power++;
            List<Vector2> tileLocations = new List<Vector2>();
            tileLocations.Add(tileLocation);
            Vector2 extremePowerPosition = Vector2.Zero;
            switch (who.FacingDirection)
            {
                case 0:
                    if (power >= 6)
                    {
                        extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y - 2f);
                        break;
                    }
                    if (power >= 2)
                    {
                        tileLocations.Add(tileLocation + new Vector2(0f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(0f, -2f));
                    }
                    if (power >= 3)
                    {
                        tileLocations.Add(tileLocation + new Vector2(0f, -3f));
                        tileLocations.Add(tileLocation + new Vector2(0f, -4f));
                    }
                    if (power >= 4)
                    {
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.Add(tileLocation + new Vector2(1f, -2f));
                        tileLocations.Add(tileLocation + new Vector2(1f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(1f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, -2f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
                    }
                    if (power >= 5)
                    {
                        for (int l = tileLocations.Count - 1; l >= 0; l--)
                        {
                            tileLocations.Add(tileLocations[l] + new Vector2(0f, -3f));
                        }
                    }
                    break;
                case 1:
                    if (power >= 6)
                    {
                        extremePowerPosition = new Vector2(tileLocation.X + 2f, tileLocation.Y);
                        break;
                    }
                    if (power >= 2)
                    {
                        tileLocations.Add(tileLocation + new Vector2(1f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(2f, 0f));
                    }
                    if (power >= 3)
                    {
                        tileLocations.Add(tileLocation + new Vector2(3f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(4f, 0f));
                    }
                    if (power >= 4)
                    {
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.Add(tileLocation + new Vector2(0f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(1f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(2f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(0f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(1f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(2f, 1f));
                    }
                    if (power >= 5)
                    {
                        for (int k = tileLocations.Count - 1; k >= 0; k--)
                        {
                            tileLocations.Add(tileLocations[k] + new Vector2(3f, 0f));
                        }
                    }
                    break;
                case 2:
                    if (power >= 6)
                    {
                        extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y + 2f);
                        break;
                    }
                    if (power >= 2)
                    {
                        tileLocations.Add(tileLocation + new Vector2(0f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(0f, 2f));
                    }
                    if (power >= 3)
                    {
                        tileLocations.Add(tileLocation + new Vector2(0f, 3f));
                        tileLocations.Add(tileLocation + new Vector2(0f, 4f));
                    }
                    if (power >= 4)
                    {
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.Add(tileLocation + new Vector2(1f, 2f));
                        tileLocations.Add(tileLocation + new Vector2(1f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(1f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, 2f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
                    }
                    if (power >= 5)
                    {
                        for (int j = tileLocations.Count - 1; j >= 0; j--)
                        {
                            tileLocations.Add(tileLocations[j] + new Vector2(0f, 3f));
                        }
                    }
                    break;
                case 3:
                    if (power >= 6)
                    {
                        extremePowerPosition = new Vector2(tileLocation.X - 2f, tileLocation.Y);
                        break;
                    }
                    if (power >= 2)
                    {
                        tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(-2f, 0f));
                    }
                    if (power >= 3)
                    {
                        tileLocations.Add(tileLocation + new Vector2(-3f, 0f));
                        tileLocations.Add(tileLocation + new Vector2(-4f, 0f));
                    }
                    if (power >= 4)
                    {
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.RemoveAt(tileLocations.Count - 1);
                        tileLocations.Add(tileLocation + new Vector2(0f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(-2f, -1f));
                        tileLocations.Add(tileLocation + new Vector2(0f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
                        tileLocations.Add(tileLocation + new Vector2(-2f, 1f));
                    }
                    if (power >= 5)
                    {
                        for (int i = tileLocations.Count - 1; i >= 0; i--)
                        {
                            tileLocations.Add(tileLocations[i] + new Vector2(-3f, 0f));
                        }
                    }
                    break;
            }
            if (power >= 6)
            {
                tileLocations.Clear();
                for (int x = (int)extremePowerPosition.X - 2; (float)x <= extremePowerPosition.X + 2f; x++)
                {
                    for (int y = (int)extremePowerPosition.Y - 2; (float)y <= extremePowerPosition.Y + 2f; y++)
                    {
                        tileLocations.Add(new Vector2(x, y));
                    }
                }
            }
            return tileLocations;
        }
        private void LeftClick(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                isMouseLeftButtonDown = true;
                if (lastMouseTile != Vector2.Zero)
                {
                    WateringPoison();
                    Vector2 tileCoordinates = Game1.currentCursorTile;
                    //for the tree in northwest
                    if (Game1.player.CurrentTool is Axe && Game1.currentLocation != null)
                    {
                        if (Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild DreamTree 5 5")
                        {
                            if (clicks < 1)
                            {
                                Game1.addHUDMessage(new HUDMessage("Why would you want to cut down such a beautiful tree?", 2));
                            }
                            else if (clicks < 2)
                            {
                                Game1.addHUDMessage(new HUDMessage("Do you really have no conscience??", 2));
                            }
                            else if (clicks < 3)
                            {
                                Game1.addHUDMessage(new HUDMessage("You will regret this...", 2));
                            }
                            else if (clicks < 4)
                            {
                                Game1.addHUDMessage(new HUDMessage("STOPPPPP!!!!!", 2));
                            }
                            else if (clicks < 5)
                            {
                                Game1.addHUDMessage(new HUDMessage("OKAY THAT'S IT! NO MORE AXE FOR MEANIES LIKE YOU!", 2));
                                Game1.player.CurrentTool = new WateringCan();
                            }
                            else if (clicks < 6)
                            {
                                Game1.addHUDMessage(new HUDMessage("HOW DID YOU EVEN GET ANOTHER AXE?!?!?", 2));
                                Game1.addHUDMessage(new HUDMessage("DIEEE!!!!!!!!!", 2));
                                for(int i =0; i < 5;  i++)
                                {
                                    Monster temp = new Skeleton(new Vector2(16+i, 18));
                                    temp.BuffForAdditionalDifficulty(1000);
                                    Game1.currentLocation.characters.Add(temp);
                                }
                            }
                            else
                            {
                                Game1.addHUDMessage(new HUDMessage("DIEEE!!!!!!!!!", 2));
                                for (int i = 0; i < 5; i++)
                                {
                                    Monster temp = new Skeleton(new Vector2(16 + i, 18));
                                    temp.BuffForAdditionalDifficulty(1000);
                                    Game1.currentLocation.characters.Add(temp);
                                }
                            }
                            clicks++;
                        }
                    }
                }
            }
        }
        private void WateringPoison()
        {
            //deactivate the poison with water
            if (Game1.player.CurrentTool is WateringCan watercan && Game1.currentLocation != null)
            {
                Vector2 tileCoordinates = Game1.currentCursorTile;
                int power = Game1.player.toolPower.Value;
                float distance = Vector2.Distance(Game1.player.Tile, tileCoordinates);
                if(distance < 3  && watercan.WaterLeft>0)
                {
                    List<Vector2> tileLocations = tilesAffected(new Vector2((int)tileCoordinates.X, (int)tileCoordinates.Y), power, Game1.player);
                    //Monitor.Log("Tiles Affected: ", LogLevel.Info);
                    foreach (Vector2 tile in tileLocations)
                    {
                        DeactivatePoisonTile((int)tile.X, (int)tile.Y);
                        //Monitor.Log($"{tile}", LogLevel.Info);
                    }
                }
            }
        }
        private void WateringPoisonRelease(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                isMouseLeftButtonDown = false;
                WateringPoison();
            }
        }
        private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            Vector2 mouseTile = Game1.currentCursorTile;
            lastMouseTile = mouseTile;
        }

        private void DeactivatePoisonTile(int x, int y)
        {
            if (Game1.currentLocation != null)
            {
                Game1.currentLocation.removeTileProperty(x, y, "Back", "TouchAction");
                //asset needs to be changed back from the poison tile
                //int originalIndex = 0; 
                //Game1.currentLocation.setMapTileIndex(x, y, originalIndex, "Back");
                Game1.currentLocation.removeTile(x, y, "Back");
            }
        }
        #endregion

        #region // Warp Methods
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            // Handles sleep warp logic. (CHANGE ITEM ID TO TEDDY BEAR)
            if (Game1.player.isInBed.Value && Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.lastQuestionKey.StartsWith("Sleep")
                && Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "TeddyBear")
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
}
