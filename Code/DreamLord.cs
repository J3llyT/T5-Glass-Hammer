using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace HopeToRiseMod.Monsters
{
    public class DreamLord : Monster
    {
        // Fields
        private float lastFireball;

        private new int yOffset;

        private readonly NetEvent0 fireballEvent = new NetEvent0();

        private readonly NetEvent0 hurtAnimationEvent = new NetEvent0();

        private int numFireballsLeft;

        private float firingTimer;

        public int numHitsToStagger;

        private int nextMaxStagger = 3;

        private int numTimesStaggered = 0;

        private float staggerTimer;

        // Constructors
        public DreamLord()
        {
        }

        public DreamLord(Vector2 position)
            : base("Dream Lord", position)
        {
            this.Sprite = new AnimatedSprite("Characters\\DreamLord");
            this.Sprite.SpriteHeight = 32;
            base.IsWalkingTowardPlayer = false;
            this.Sprite.UpdateSourceRect();
            //base.HideShadow = true;
            base.shouldShadowBeOffset = true;

            // Set the initial numHitsToStagger
            numHitsToStagger = nextMaxStagger;
            // Now set a new max stagger based on how many times boss has been staggered
            nextMaxStagger = nextMaxStagger + (4 + numTimesStaggered) / 2;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddField(this.fireballEvent, "fireballEvent").AddField(this.hurtAnimationEvent, "hurtAnimationEvent");
            this.fireballEvent.onEvent += delegate
            {
                if (!Game1.IsMasterGame)
                {
                    this.fireballFired();
                }
            };
            this.hurtAnimationEvent.onEvent += delegate
            {
                this.Sprite.currentFrame = this.Sprite.currentFrame - this.Sprite.currentFrame % 4 + 3;
            };
        }

        /// <inheritdoc />
        public override void reloadSprite(bool onlyAppearance = false)
        {
            this.Sprite = new AnimatedSprite("Characters\\DreamLord");
            this.Sprite.SpriteHeight = 32;
            this.Sprite.UpdateSourceRect();
        }


        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            // Decrease stagger counter if not already staggered
            if (staggerTimer <= 0)
            {
                numHitsToStagger--;
            }
            // If there are no hits left to stagger, stagger boss
            if (numHitsToStagger <= 0)
            {
                staggerTimer = Game1.random.Next(6000, 9500);

                // Set next number of hits to stagger and next max stagger threshold
                numHitsToStagger = nextMaxStagger;
                nextMaxStagger = nextMaxStagger + (4 + numTimesStaggered) / 2;
            }

            int actualDamage = Math.Max(1, damage - (int)base.resilience);
            if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
            {
                actualDamage = -1;
            }
            else
            {
                base.Health -= actualDamage;
                base.setTrajectory(xTrajectory/ 2, yTrajectory / 2);
                if (staggerTimer > 0)
                {
                    base.setTrajectory(xTrajectory / 8, yTrajectory / 8);
                }
                base.currentLocation.playSound("hitEnemy");
                this.hurtAnimationEvent.Fire();
                if (base.Health <= 0)
                {
                    base.deathAnimation();
                }
            }

            return actualDamage;
        }

        protected override void sharedDeathAnimation()
        {
        }

        protected override void localDeathAnimation()
        {
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName, new Rectangle(0, 64, 16, 16), 70f, 7, 0, base.Position + new Vector2(0f, -32f), flicker: false, flipped: false)
            {
                scale = 4f
            });
            base.currentLocation.localSound("fireball");
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
            {
                delayBeforeAnimationStart = 100
            });
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
            {
                delayBeforeAnimationStart = 200
            });
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
            {
                delayBeforeAnimationStart = 300
            });
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
            {
                delayBeforeAnimationStart = 400
            });
        }

        public override void drawAboveAllLayers(SpriteBatch b)
        {
            int standingY = base.StandingPixel.Y;
            //b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.yOffset), this.Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
            b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
        }

        protected override void updateAnimation(GameTime time)
        {
            if (this.isMoving())
		    {
			    switch (this.FacingDirection)
			    {
			    case 0:
				    this.Sprite.AnimateUp(time);
				    break;
			    case 3:
				    this.Sprite.AnimateLeft(time);
				    break;
			    case 1:
				    this.Sprite.AnimateRight(time);
				    break;
			    case 2:
				    this.Sprite.AnimateDown(time);
				    break;
			    }
		    }
        }

        protected override void updateMonsterSlaveAnimation(GameTime time)
        {
            if (this.isMoving())
            {
                switch (this.FacingDirection)
                {
                    case 0:
                        this.Sprite.AnimateUp(time);
                        break;
                    case 3:
                        this.Sprite.AnimateLeft(time);
                        break;
                    case 1:
                        this.Sprite.AnimateRight(time);
                        break;
                    case 2:
                        this.Sprite.AnimateDown(time);
                        break;
                }
            }
            base.faceGeneralDirection(base.Player.Position);
        }

        private Vector2 fireballFired()
        {
            switch (this.FacingDirection)
            {
                case 0:
                    this.Sprite.currentFrame = 3;
                    return Vector2.Zero;
                case 1:
                    this.Sprite.currentFrame = 7;
                    return new Vector2(64f, 0f);
                case 2:
                    this.Sprite.currentFrame = 11;
                    return new Vector2(0f, 32f);
                case 3:
                    this.Sprite.currentFrame = 15;
                    return new Vector2(-32f, 0f);
                default:
                    return Vector2.Zero;
            }
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            this.fireballEvent.Poll();
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            // If the boss is staggered, emote and then do nothing
            if (staggerTimer > 0)
            {
                // Emote on 'em
                base.doEmote(sleepEmote, true);

                // Subtract from staggerTimer
                staggerTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;

                // if stagger timer is now below 0, wake up boss
                if (staggerTimer <= 0)
                {
                    base.CurrentEmote = angryEmote;
                    base.emoteInterval = 4f;
                    base.doEmote(angryEmote, true);
                }
                else
                {
                    // Otherwise, do nothing
                    return;
                }
            }

            base.behaviorAtGameTick(time);
            base.faceGeneralDirection(base.Player.Position);

            // Subtract time from last fireball shot
            this.lastFireball = Math.Max(0f, this.lastFireball - (float)time.ElapsedGameTime.Milliseconds);

            // Hard mode behavior
            if ((bool)base.isHardModeMonster)
            {
                // If there are no fireballs left and the player is not nearby
                if ((this.numFireballsLeft <= 0 && !this.withinPlayerThreshold()) || !(this.lastFireball <= 0f))
                {
                    return;
                }
                // If the time since last fireball has expire but there are no fireballs, add some
                if (this.lastFireball <= 0f && this.numFireballsLeft <= 0)
                {
                    this.numFireballsLeft = 4;
                    this.firingTimer = 0f;
                }
                // lower the firing timer
                this.firingTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
                // If it is time to fire and monster has fireballs
                if (this.firingTimer <= 0f && this.numFireballsLeft > 0)
                {
                    // Get ready to fire
                    Rectangle playerBounds = base.Player.GetBoundingBox();
                    this.numFireballsLeft--;
                    // Stop walking
                    base.IsWalkingTowardPlayer = false;
                    this.Halt();
                    // Trigger fireball event
                    this.fireballEvent.Fire();
                    this.fireballFired();
                    this.Sprite.UpdateSourceRect();
                    // Create a projectile and launch it
                    Vector2 standingPixel2 = base.getStandingPosition();
                    Vector2 trajectory = Utility.getVelocityTowardPoint(standingPixel2, new Vector2(playerBounds.X, playerBounds.Y) + new Vector2(Game1.random.Next(-128, 128)), 8f);
                    BasicProjectile projectile = new BasicProjectile(10, 10, 2, 4, 0f, trajectory.X, trajectory.Y, standingPixel2 - new Vector2(32f, 0f), null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
                    projectile.height.Value = 48f;
                    base.currentLocation.projectiles.Add(projectile);
                    base.currentLocation.playSound("fireball");
                    this.firingTimer = 400f;
                    // If there are no fireballs left, set a delay till next one can be fired
                    if (this.numFireballsLeft <= 0)
                    {
                        this.lastFireball = Game1.random.Next(3000, 6500);
                    }
                }
            }
            // Non hard mode behavior
            // If player is nearby and it is time to shoot a fireball and rng allows for it
            else if (this.withinPlayerThreshold() && this.lastFireball == 0f && Game1.random.NextDouble() < 0.01)
            {
                // Stop walking
                base.IsWalkingTowardPlayer = false;
                this.Halt();
                // Trigger fireball event
                this.fireballEvent.Fire();
                this.fireballFired();
                // Create a projectile and launch it
                this.Sprite.UpdateSourceRect();
                Point standingPixel = base.StandingPixel;
                Vector2 trajectory2 = Utility.getVelocityTowardPlayer(standingPixel, 8f, base.Player);
                BasicProjectile projectile2 = new BasicProjectile(5, 10, 3, 4, 0f, trajectory2.X, trajectory2.Y, new Vector2(standingPixel.X - 32, standingPixel.Y), null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
                projectile2.height.Value = 48f;
                base.currentLocation.projectiles.Add(projectile2);
                base.currentLocation.playSound("fireball");
                this.lastFireball = Game1.random.Next(1200, 3500);
            }
            // If the player is not nearby, check for movement opportunity and move
            else if (this.lastFireball != 0f)
            {
                this.Halt();
                // If player nearby, move towards them
                if (this.withinPlayerThreshold())
                {
                    base.Slipperiness = 8;
                    Point standingTile = base.StandingPixel;
                    base.setTrajectory((int)Utility.getVelocityTowardPlayer(standingTile, 4f, base.Player).X, (int)(0f - Utility.getVelocityTowardPlayer(standingTile, 4f, base.Player).Y));
                }
            }
        }
    }
}
