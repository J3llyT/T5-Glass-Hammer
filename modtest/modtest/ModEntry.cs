using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;
using StardewValley.Buffs;

namespace modtest
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsOneSecond) 
            {
                CheckPlayerOnPoisonTile();
            }
        }
        private void CheckPlayerOnPoisonTile()
        {
            if (Game1.player == null || Game1.currentGameTime.TotalGameTime.TotalSeconds < 5)
            {
                return;
            }

            GameLocation farmLocation = Game1.getLocationFromName("Farm");

            if (farmLocation == null || farmLocation.map == null)
            {
                return;
            }

            Vector2 playerTile = Game1.player.getLocalPosition(Game1.viewport);
            Monitor.Log($"Player Tile Coordinates: ({playerTile.X}, {playerTile.Y})", LogLevel.Info);

            if (playerTile.X < 0 || playerTile.Y < 0)
            {
                return;
            }

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

            // Check if the player is on the poisoned tile
            if (farmLocation.doesTileHaveProperty((int)playerTile.X, (int)playerTile.Y, "Paths", "Poison") != null)
            {
                Game1.player.applyBuff(buff);
                Monitor.Log($"Player Tile Coordinates: ({playerTile.X}, {playerTile.Y})", LogLevel.Info);
                Monitor.Log("Player on poisoned tile.", LogLevel.Info);
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs args)
        {
            GameLocation farmLocation = Game1.getLocationFromName("Farm");
            if (farmLocation == null)
            {
                Monitor.Log("Farm location not found.", LogLevel.Error);
                return;
            }

            int tileX = 63; 
            int tileY = 23; 
            foreach (var layer in farmLocation.map.Layers)
            {
                Monitor.Log($"Layer Name: {layer.Id}", LogLevel.Info);
            }

            Layer pathLayer = farmLocation.map.GetLayer("Paths");
            if (pathLayer == null)
            {
                Monitor.Log("Paths layer not found.", LogLevel.Error);
                return;
            }

            TileSheet pathTileSheet = farmLocation.map.GetTileSheet("Paths");
            if (pathTileSheet == null)
            {
                Monitor.Log("Paths tile sheet not found.", LogLevel.Error);
                return;
            }

            if (pathLayer != null && pathTileSheet != null)
            {
                Texture2D customTileTexture = this.Helper.ModContent.Load<Texture2D>("assets/poison.png");
                //int customTileIndex = pathTileSheet.AddTileSheetImage(customTileTexture);

                for (int x = 0; x < pathLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < pathLayer.LayerHeight; y++)
                    {
                        // Set the custom tile on each tile in the specified layer
                        pathLayer.Tiles[x, y] = new StaticTile(pathLayer, pathTileSheet, BlendMode.Alpha, tileIndex: 100);
                    }
                }

                //pathLayer.Tiles[tileX, tileY] = new StaticTile(pathLayer, pathTileSheet, BlendMode.Alpha, tileIndex: 100);
                ////pathLayer.Tiles[tileX, tileY] = new StaticTile(pathLayer, pathTileSheet, BlendMode.Alpha, tileIndex: customTileIndex);
                //
                //farmLocation.setTileProperty(tileX, tileY, "Paths", "Poison", "T");
                //Monitor.Log("Custom property set for tile (63, 23).", LogLevel.Info);
            }
            else
            {
                Monitor.Log("Layer or TileSheet not found.", LogLevel.Error);
            }
        }
    }
}
