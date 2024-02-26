using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;



namespace HopeToRiseMod
{
    public class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
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

