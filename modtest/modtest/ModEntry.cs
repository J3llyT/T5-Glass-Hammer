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
            GameLocation.RegisterTouchAction("poison", GiveBuff);
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
    }
}

