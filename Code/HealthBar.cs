using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using HopeToRiseMod.Monsters;

namespace HopeToRiseMod.UI
{
    public class HealthBarElement : IClickableMenu
    {
        private Rectangle healthBarBounds;
        private Rectangle healthBarFill;

        private readonly DreamLord boss;
        private float healthPercentage;

        private Vector2 shakeOffset;
        private float shakeTimer;
        private const float ShakeDuration = 0.2f;
        private const float ShakeMagnitude = 5f;
        private int previousHealth;

        public HealthBarElement(DreamLord boss) : base(0, 0, 200, 40)
        {
            this.boss = boss;
            this.healthBarBounds = CalculateHealthBarPosition();
            this.healthBarFill = new Rectangle(0, 0, 0, 20);
            this.previousHealth = boss.Health;
        }

        private Rectangle CalculateHealthBarPosition()
        {
            int barWidth = 600;
            int barHeight = 20;
            int barX = (Game1.viewport.Width - barWidth) / 2;
            int barY = 160;
            return new Rectangle(barX, barY, barWidth, barHeight);
        }

        public void UpdateHealthPercentage()
        {
            healthPercentage = (float)boss.Health / boss.MaxHealth;

            // Check if the boss has taken damage
            if (boss.Health < previousHealth)
            {
                shakeTimer = ShakeDuration;
            }

            // Update the shake effect
            if (shakeTimer > 0)
            {
                float offsetX = (float)(Game1.random.NextDouble() - 0.5) * ShakeMagnitude;
                float offsetY = (float)(Game1.random.NextDouble() - 0.5) * ShakeMagnitude;

                shakeOffset = new Vector2(offsetX, offsetY);

                shakeTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                shakeOffset = Vector2.Zero;
            }

            // Update previous health
            previousHealth = boss.Health;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // This method is required by the interface but doesn't need to do anything for the health bar UI.
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            UpdateHealthPercentage();

            healthBarBounds = CalculateHealthBarPosition();
            healthBarFill.Width = (int)(healthBarBounds.Width * healthPercentage);

            // Draw everything else first
            base.draw(spriteBatch);

            // Draw black background rectangle for health bar
            spriteBatch.Draw(Game1.staminaRect, healthBarBounds, null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.99f);

            // Calculate the position for the red fill part of the health bar
            Rectangle redFillBounds = new(
                healthBarBounds.X,
                healthBarBounds.Y + (healthBarBounds.Height - healthBarFill.Height) / 2,
                healthBarFill.Width,
                healthBarFill.Height);

            // Draw red fill rectangle based on health percentage (ON A HIGHER LAYER)
            spriteBatch.Draw(Game1.staminaRect, redFillBounds, null, Color.Red, 0f, Vector2.Zero, SpriteEffects.None, 0.995f);

            // Calculate boss name position above the health bar
            Vector2 nameSize = Game1.dialogueFont.MeasureString(boss.Name);
            Vector2 namePosition = new Vector2(
                healthBarBounds.X + healthBarBounds.Width / 2 - nameSize.X / 2,
                healthBarBounds.Y - nameSize.Y - 5);

            // Draw boss's name centered above the health bar
            spriteBatch.DrawString(Game1.dialogueFont, boss.Name, namePosition + shakeOffset, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.999f);

            // Apply the shake offset to the health bar position
            healthBarBounds.Y += (int)shakeOffset.Y;
        }

        public override void update(GameTime gameTime)
        {
            // Update health bar position (if needed)
            healthBarBounds = CalculateHealthBarPosition();

            base.update(gameTime);
        }
    }
}
