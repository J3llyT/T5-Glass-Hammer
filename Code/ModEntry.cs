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
using System.Threading;
using Microsoft.Xna.Framework.Input;
using static StardewValley.GameLocation;
using static StardewValley.Minigames.CraneGame;
using StardewValley.GameData.Objects;
using StardewValley.Quests;


namespace HopeToRiseMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private int staminaThreshold = 3; // Adjust this value as needed

        private bool bossSpawned = false;
        private bool poisonTilesSpawnedBoss = false;
        private bool poisonTilesSpawnedWest = false;
        private bool enemiesSpawned = false;

        private DreamLord somnia;

        private Vector2 lastMouseTile;

        Texture2D PoisonTile;
        Texture2D PoisonTileCooled;

        private bool isMouseLeftButtonDown = false;
        private int clicks = 0;
        Random rng = new Random();
        bool bossUnlock = false;
        int timer = 0;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameLocation.RegisterTouchAction("poison", GiveBuff);
            GameLocation.RegisterTouchAction("returnHome", ReturnToBed);
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            //for watering can logic
            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Input.ButtonPressed += LeftClick;
            helper.Events.Input.ButtonReleased += WateringPoisonRelease;
            helper.Events.Input.ButtonPressed += drink;

            //loading in textures
            PoisonTile = helper.ModContent.Load<Texture2D>("../[CP] Hope to Rise/assets/PoisonTile.png");
            //PoisonTileCooled = helper.ModContent.Load<Texture2D>("../[CP] Hope to Rise/assets/PoisonTileCooled.png");
        }

        int haveDrink = 0;
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            #region // Warp Logic
            // NEED TO UPDATE TO ACCOMADATE FOR ALL OF THE LOCATIONS
            if (Game1.player.stamina <= staminaThreshold && Game1.currentLocation != null)
            {
                if (Game1.currentLocation.Name is
                    "DreamWorldSpawn" or
                    "DreamWorldHub" or
                    "DreamWorldEast" or
                    "DreamWorldNorthEast" or
                    "DreamWorldWest" or
                    "DreamWorldNorthWest" or
                    "DreamWorldCore" or
                    "DreamWorldBoss")
                {
                    Monitor.Log("Player Passed Out!", LogLevel.Info);

                    // Display a blackout screen
                    Game1.showGlobalMessage("You're waking up...");

                    // Teleport the player to their last slept-in bed location (NEED TO FIND BED LOCATION)
                    Game1.warpHome();
                    Game1.NewDay(1);
                    // WarpPlayerToNewLocation("FarmHouse", (int)Game1.player.mostRecentBed.X, (int)Game1.player.mostRecentBed.Y);
                    // Game1.player.setTileLocation(Game1.player.mostRecentBed);

                    // Reset the player's stamina to prevent further passouts
                    Game1.player.stamina = Game1.player.MaxStamina;
                }
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
            if (Game1.currentLocation != null && Game1.currentLocation.characters.Contains(somnia))
            {
                //Monitor.Log(somnia.numHitsToStagger.ToString());
                //Monitor.Log(somnia.behavior.ToString());
            }
            DefeatedBoss();
            #endregion

            #region // Tile Logic
            if (bossSpawned && Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldBoss" && !poisonTilesSpawnedBoss)
            {
                // Access the current game location
                GameLocation currentLocation = Game1.currentLocation;

                // Define the location and size of the target area
                int targetX = 15;
                int targetY = 12;
                int areaSize = 10;

                // Define the number of tiles to replace
                int numTilesToReplace = 50;

                // Randomly select tiles within the target area and replace them
                Random random = new Random();
                for (int i = 0; i < numTilesToReplace; i++)
                {
                    int randomX = targetX - areaSize + random.Next(2 * areaSize + 1);
                    int randomY = targetY - areaSize + random.Next(2 * areaSize + 1);

                    // Check if the randomly selected tile is within the target area
                    if (randomX >= targetX - areaSize && randomX <= targetX + areaSize && randomY >= targetY - areaSize && randomY <= targetY + areaSize)
                    {
                        SpawnPoisonTile(randomX, randomY);
                    }
                }

                poisonTilesSpawnedBoss = true;
            }

            if (Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldWest" && !poisonTilesSpawnedWest)
            {
                SpawnPoisonTile(45, 19);

                // Upper path
                SpawnPoisonTile(33, 13);
                SpawnPoisonTile(33, 12);
                SpawnPoisonTile(34, 12);
                SpawnPoisonTile(35, 12);
                SpawnPoisonTile(32, 11);
                SpawnPoisonTile(33, 11);
                SpawnPoisonTile(34, 11);
                SpawnPoisonTile(36, 11);
                SpawnPoisonTile(34, 10);
                SpawnPoisonTile(35, 10);
                SpawnPoisonTile(36, 10);
                SpawnPoisonTile(35, 9);
                SpawnPoisonTile(36, 9);
                SpawnPoisonTile(37, 9);
                SpawnPoisonTile(35, 8);
                SpawnPoisonTile(36, 8);
                SpawnPoisonTile(37, 8);

                // Lower path 
                SpawnPoisonTile(29, 29);
                SpawnPoisonTile(30, 29);
                SpawnPoisonTile(28, 30);
                SpawnPoisonTile(30, 30);
                SpawnPoisonTile(31, 30);
                SpawnPoisonTile(29, 31);
                SpawnPoisonTile(27, 32);
                SpawnPoisonTile(30, 32);
                SpawnPoisonTile(28, 33);
                SpawnPoisonTile(29, 33);
                SpawnPoisonTile(31, 33);
                SpawnPoisonTile(30, 34);
                SpawnPoisonTile(31, 34);

                SpawnPoisonTile(20, 34);
                SpawnPoisonTile(20, 35);
                SpawnPoisonTile(21, 33);

                // Upper left
                SpawnPoisonTile(12, 8);
                SpawnPoisonTile(10, 9);
                SpawnPoisonTile(11, 9);
                SpawnPoisonTile(9, 10);
                SpawnPoisonTile(11, 10);
                SpawnPoisonTile(10, 11);
                SpawnPoisonTile(11, 11);
                SpawnPoisonTile(9, 12);
                SpawnPoisonTile(10, 12);
                SpawnPoisonTile(12, 12);
                SpawnPoisonTile(11, 13);
                SpawnPoisonTile(12, 14);
                SpawnPoisonTile(13, 14);

                poisonTilesSpawnedWest = true;
            }

            #endregion

            #region //Event Logic
            var seenEvents = Game1.eventsSeenSinceLastLocationChange;
            if (seenEvents.Count > 0)
            {
                foreach (var eventId in seenEvents)
                {
                    //Monitor.Log($"Seen event: {eventId}", LogLevel.Info);
                    if (eventId == "HTR.DreamWorldBoss_event") bossUnlock = true;
                }
            }
            if (bossUnlock) BossBlock();

            //if(!seenEvents.Contains("EventID")) PlayFirstBossEvent();
            #endregion

            #region //Enemy Logic
            if(Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldNorthWest" &&!enemiesSpawned)
            {
                Monster temp = new GreenSlime(new Vector2(28 *64, 14*64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                temp = new GreenSlime(new Vector2(28 * 64, 25 * 64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                temp = new GreenSlime(new Vector2(9 * 64, 27 * 64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                temp = new GreenSlime(new Vector2(6 * 64, 20 * 64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                temp = new GreenSlime(new Vector2(9 * 64, 11 * 64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                temp = new GreenSlime(new Vector2(18 * 64, 21 * 64));
                if (temp != null) Game1.currentLocation.characters.Add(temp);
                enemiesSpawned = true;
            }
            #endregion
        }
        #region//Mouse Methods
        private void LeftClick(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                isMouseLeftButtonDown = true;
                if (lastMouseTile != Vector2.Zero)
                {
                    if (isMouseLeftButtonDown) WateringPoison();
                    Vector2 tileCoordinates = Game1.currentCursorTile;
                    float distance = Vector2.Distance(Game1.player.Tile, tileCoordinates);
                    if (Game1.currentLocation != null && Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "BossBlock", "Buildings") == "T" && distance < 3)
                    {
                        Game1.addHUDMessage(new HUDMessage("They look like they're guarding something...", 2));
                    }
                    //for the trees in northwest

                    if (Game1.player.CurrentTool is Axe && Game1.currentLocation != null)
                    {
                        if (Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree1 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree2 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree3 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree4 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree5 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree6 5 5" ||
                           Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild FishTree7 5 5")
                        {
                            int rand = rng.Next(3);
                            if (rand == 0) Game1.addHUDMessage(new HUDMessage("OW!!", 2));
                            if (rand == 1) Game1.addHUDMessage(new HUDMessage("oww...", 2));
                            if (rand == 2) Game1.addHUDMessage(new HUDMessage("meanie...", 2));
                        }
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
                            else if (clicks < 7)
                            {
                                Game1.addHUDMessage(new HUDMessage("BETRAYER TRAITOR LIAR FRAUDDD", 2));
                                Game1.addHUDMessage(new HUDMessage("DIEEE!!!!!!!!!", 2));
                                for (int i = 0; i < 5; i++)
                                {
                                    Monster temp = new Skeleton(new Vector2(Game1.player.Position.X, Game1.player.Position.Y));
                                    temp.BuffForAdditionalDifficulty(1000);
                                    Game1.currentLocation.characters.Add(temp);
                                }
                            }
                            else
                            {
                                Game1.addHUDMessage(new HUDMessage("DIEEE!!!!!!!!!", 2));
                                for (int i = 0; i < 5; i++)
                                {
                                    Monster temp = new Skeleton(new Vector2(Game1.player.Position.X, Game1.player.Position.Y));
                                    temp.BuffForAdditionalDifficulty(1000);
                                    Game1.currentLocation.characters.Add(temp);
                                }
                            }
                            clicks++;
                        }
                    }
                    else if (Game1.player.CurrentTool is WateringCan && Game1.currentLocation != null && Game1.currentLocation.doesTileHaveProperty((int)tileCoordinates.X, (int)tileCoordinates.Y, "SpawnTree", "Paths") == "wild DreamTree 5 5")
                    {
                        Game1.addHUDMessage(new HUDMessage("Oh why thank you :)", 2));
                        if (clicks == 5)
                        {
                            Game1.addHUDMessage(new HUDMessage("I guess you can have your axe back now.", 2));
                            //need to make the game wait a bit to give 
                            timer++;
                            if (timer > 0)
                            {
                                Game1.player.CurrentTool = new Axe();
                                timer = 0;
                            }
                            clicks++;
                        }
                    }
                }
            }
        }
        #endregion

        #region //question dialogue
        private void drink(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight && Game1.player.CurrentItem != null)
            {
                if (Game1.player.CurrentItem.Name == "Wake Up Water")
                {
                    question("Would you like to return to reality?",
                         new Response[2]
                         {
                    new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
                    new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
                         }
                         , delegate (Farmer who, string whichAnswer)
                         {
                             if (whichAnswer == "Yes")
                             {
                                 Game1.showGlobalMessage("You're waking up...");
                                 Game1.warpHome();
                                 Game1.player.stamina = Game1.player.MaxStamina;
                             }
                         });
                    Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                }
            }
        }
        private void ReturnToBed(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            question("Would you like to return to reality?",
                 new Response[2]
                 {
                    new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
                    new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
                 }
                 , delegate (Farmer who, string whichAnswer)
                 {
                     if (whichAnswer == "Yes")
                     {
                         Game1.showGlobalMessage("You're waking up...");
                         Game1.warpHome();
                         Game1.player.stamina = Game1.player.MaxStamina;
                     }
                 });
        }
        public void question(string question, Response[] answerChoices, afterQuestionBehavior afterDialogueBehavior, NPC speaker = null)
        {
            Game1.currentLocation.lastQuestionKey = null;
            Game1.currentLocation.afterQuestion = afterDialogueBehavior;
            Game1.drawObjectQuestionDialogue(question, answerChoices);
            if (speaker != null)
            {
                Game1.objectDialoguePortraitPerson = speaker;
            }
        }
        #endregion

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
        #region //Watering Can Methods
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
        private void WateringPoison()
        {
            // Deactivate the poison with water
            if (Game1.player.CurrentTool is WateringCan watercan && Game1.currentLocation != null)
            {
                // Check if the current location is "DreamWorldBoss"
                if (Game1.currentLocation.Name == "DreamWorldBoss" || Game1.currentLocation.Name == "DreamWorldWest")
                {
                    Vector2 tileCoordinates = Game1.currentCursorTile;
                    float distance = Vector2.Distance(Game1.player.Tile, tileCoordinates);
                    if (distance < 3 && watercan.WaterLeft > 0)
                    {
                        List<Vector2> tileLocations = tilesAffected(new Vector2((int)tileCoordinates.X, (int)tileCoordinates.Y), Game1.player.toolPower, Game1.player);
                        //Monitor.Log("Tiles Affected: ", LogLevel.Info);
                        foreach (Vector2 tile in tileLocations)
                        {
                            DeactivatePoisonTile((int)tile.X, (int)tile.Y);
                            //Monitor.Log($"{tile}", LogLevel.Info);
                        }
                        tileLocations.Clear();
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
                if (Game1.currentLocation.Name == "DreamWorldBoss" && x <= 31 && x >= 0 && y <= 23 && y >= 0 ||
                    Game1.currentLocation.Name == "DreamWorldWest" && x <= 53 && x >= 0 && y <= 41 && y >= 0)
                {
                    Game1.currentLocation.removeTileProperty(x, y, "Back", "TouchAction");
                    Game1.currentLocation.removeTile(x, y, "Back");
                }
            }
        }
        #endregion
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
                WarpPlayerToNewLocation("dreamworldspawn", 14, 7);
            }
        }
        #endregion

        #region//Event Methods
        private void BossBlock()
        {
            if (Game1.currentLocation.Name == "DreamWorldHub")
            {
                //delete block after certain event
                for (int i = 16; i < 19; i++)
                {
                    Game1.currentLocation.removeTileProperty(i, 0, "Buildings", "BossBlock");
                    Game1.currentLocation.removeTileProperty(i, 1, "Buildings", "BossBlock");
                    Game1.currentLocation.removeTile(i, 0, "Buildings");
                    Game1.currentLocation.removeTile(i, 1, "Buildings");
                }
            }
        }

        private void PlayFirstBossEvent()
        {
            if(Quest.getQuestFromId("ID").checkIfComplete() && Quest.getQuestFromId("ID").checkIfComplete())
            {
                Game1.PlayEvent("EventID");
            }
        }
        #endregion

        #region //DreamLord Defeat Methods
        /// <summary>
        /// Handles Game behavior for when the player
        /// defeats the Lord Somnia
        /// </summary>
        void DefeatedBoss()
        {
            if (Game1.currentLocation != null && Game1.currentLocation.Name == "DreamWorldBoss")
            {
                if (somnia.Health == 0 && !Quest.getQuestFromId("QuestID").checkIfComplete())
                {
                    Game1.player.completeQuest("QuestID");
                    Game1.PlayEvent("eventID");
                }
            }
        }
        #endregion

        private void SpawnPoisonTile(int x, int y)
        {
            // Access the current game location
            GameLocation currentLocation = Game1.currentLocation;

            // Change the tile at the randomly selected position
            currentLocation.setMapTileIndex(x, y, 923467, "Back");
            //add the poison to the tile


            currentLocation.removeTile(x, y, "Back");

            Layer layer = currentLocation.map.GetLayer("Back");
            TileSheet tilesheet = currentLocation.map.GetTileSheet("z_PoisonTile");
            layer.Tiles[x, y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: 0);

            currentLocation.setTileProperty(x, y, "Back", "TouchAction", "poison");
        }

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
                    //Monitor.Log("Invalid player tile coordinates.", LogLevel.Warn);
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
