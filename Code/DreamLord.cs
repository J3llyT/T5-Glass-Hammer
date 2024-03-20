using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HopeToRiseMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace HopeToRiseMod.Monsters
{
    public enum Behavior
    {
        Idle = 0,
        SquidKid = 1,
        SpawnEnemies = 2,
        Warp = 3
    }

    public class DreamLord : Monster
    {
        // Fields
        private float lastFireball;

        private new int yOffset;

        private readonly NetEvent0 fireballEvent = new NetEvent0();

        private readonly NetEvent0 hurtAnimationEvent = new NetEvent0();

        private int numFireballsLeft;

        private float firingTimer;

        // Fields related to stagger mechanic
        public int numHitsToStagger;

        private int nextMaxStagger = 5;

        private int numTimesStaggered = 0;

        private float staggerTimer;

        private HealthBarElement healthBar;
        
        // Fields related to behavior and monsters summoned
        private List<Monster> monsterList = new List<Monster>();

        public Behavior behavior;

        private float behaviorTimer;

        // Fields related to warping
        private List<Vector2> warpPoints = new List<Vector2>();

        private int warpIndex;

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

            // Set behavior to Idle
            behavior = Behavior.Idle;
            behaviorTimer = 1000;

            // Set the initial numHitsToStagger
            numHitsToStagger = nextMaxStagger;
            // Now set a new max stagger based on how many times boss has been staggered
            nextMaxStagger = nextMaxStagger + (4 + numTimesStaggered) / 2;

            this.healthBar = new HealthBarElement(this);
            this.healthBar.width = 200; // Adjust the width of the health bar as needed
            this.healthBar.height = 30; // Adjust the height of the health bar as needed

            // Add warp positions to list
            warpPoints.Add(new Vector2(10, 5) * 64f);
            warpPoints.Add(new Vector2(20, 5) * 64f);
            warpPoints.Add(new Vector2(15, 12) * 64f);
            warpPoints.Add(new Vector2(10, 19) * 64f);
            warpPoints.Add(new Vector2(20, 19) * 64f);
            warpPoints.Add(new Vector2(5, 12) * 64f);
            warpPoints.Add(new Vector2(26, 12) * 64f);
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
            // If the player is not attacking with the Risen Blade, then deal no damage
            if (Game1.player.CurrentTool.ItemId != "RisenBlade")
            {
                return 0;
            }

            // Decrease stagger counter if not already staggered
            if (staggerTimer <= 0)
            {
                numHitsToStagger--;
            }
            // If there are no hits left to stagger, stagger boss
            if (numHitsToStagger <= 0)
            {
                staggerTimer = Game1.random.Next(6000, 9500);

                // Increase numTimesStaggered
                numTimesStaggered++;

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
            base.drawAboveAllLayers(b);

            if (this.healthBar != null)
            {
                this.healthBar.draw(Game1.spriteBatch);
            }

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
            if (staggerTimer >= 0)
            {
                // Emote on 'em
                base.doEmote(sleepEmote, true);

                // Subtract from staggerTimer
                staggerTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;

                // if stagger timer is now below 0, wake up boss
                if (staggerTimer <= 0)
                {
                    base.CurrentEmote = exclamationEmote;
                    base.emoteInterval = 4f;
                    base.doEmote(exclamationEmote, true);
                    base.updateEmote(time);

                    // After a wakeup, have the boss always teleport
                    behavior = Behavior.Warp;
                    behaviorTimer = 1000;
                    warpIndex = Game1.random.Next(0, warpPoints.Count);
                }
                else
                {
                    // Otherwise, do nothing
                    return;
                }
            }

            base.behaviorAtGameTick(time);
            base.faceGeneralDirection(base.Player.Position);

            // Loop through list of monsters
            for (int i = 0; i < monsterList.Count; i++)
            {
                // If monster is dead, remove it from list so more can spawn
                if (monsterList[i].Health <= 0)
                {
                    monsterList.RemoveAt(i--);
                }
            }

            // Decrement behavior timer to next behavior
            behaviorTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
            if (behaviorTimer <= 0)
            {
                // Randomize behavior (Next generates up to but not including the max)
                Behavior prevBehavior = behavior;
                behavior = (Behavior) (Game1.random.Next(0, 4));

                // Loop through and make sure the behavior is different from last time
                while (behavior == prevBehavior)
                {
                    behavior = (Behavior)(Game1.random.Next(0, 4));
                }

                switch (behavior)
                {
                    // Idle
                    case Behavior.Idle:
                        // Do nothing and set behavior timer for 1000 millisecond
                        base.doEmote(angryEmote, true);
                        behaviorTimer = 1000;
                        break;
                    // SquidKid 
                    case Behavior.SquidKid:
                        // Set timer for a decent amount of time and let it play out
                        behaviorTimer = Game1.random.Next(8000, 12000);
                        break;
                    // Spawn Enemies
                    case Behavior.SpawnEnemies:
                        // Wait for a few moments, then summon bats
                        behaviorTimer = 3000;
                        break;
                    case Behavior.Warp:
                        // Set time for warp to occur
                        behaviorTimer = 1000;
                        // Choose a point to warp to
                        warpIndex = Game1.random.Next(0, warpPoints.Count);
                        break;
                }
            }

            // Actually do the behavior
            switch (behavior)
            {
                // Idle
                case Behavior.Idle:
                    // Do nothing and set behavior timer for 1000 millisecond
                    break;
                // SquidKid 
                case Behavior.SquidKid:
                    // Set timer for a decent amount of time and let it play out
                    SquidKidBehavior(time);
                    break;
                // Spawn Enemies
                case Behavior.SpawnEnemies:
                    // Wait for a few moments, then summon bats
                    SpawnMonsterBehavior(time);
                    break;
                case Behavior.Warp:
                    WarpBehavior(time);
                    break;
            }
        }

        private void WarpBehavior(GameTime time)
        {
            // Wait for half the time to warp
            if (behaviorTimer <= 800)
            {
                // Warp (move really quickly to) the determined warp point
                //base.position.Set(warpPoints[warpIndex]);
                Point standingTile = base.StandingPixel;
                base.setTrajectory(
                        (int)Utility.getVelocityTowardPoint(standingTile, warpPoints[warpIndex], 40f).X,
                        (int)(0f - Utility.getVelocityTowardPoint(standingTile, warpPoints[warpIndex], 40f).Y)
                    );

                if (base.Position.X - warpPoints[warpIndex].X < 1 && base.Position.Y - warpPoints[warpIndex].Y < 1)
                {
                    behaviorTimer = 0;
                }
            }
        }

        private void SpawnMonsterBehavior(GameTime time)
        {
            // Wait until half the time to spawn enemy
            if (behaviorTimer <= 1500 && monsterList.Count <= 2)
            {
                // Get the boss's position
                Vector2 bossPosition = this.Position;
                // Spawn the monsters around the boss
                monsterList.Add(new Bat(new Vector2(bossPosition.X + 3 * 64f, bossPosition.Y)));
                monsterList.Add(new Bat(new Vector2(bossPosition.X - 3 * 64f, bossPosition.Y)));
                monsterList.Add(new Bat(new Vector2(bossPosition.X, bossPosition.Y + 3 * 64f)));
                monsterList.Add(new Bat(new Vector2(bossPosition.X, bossPosition.Y - 3 * 64f)));

                // Add the monsters to the list of monsters in the fight
                Game1.currentLocation.characters.Add(monsterList[0]);
                Game1.currentLocation.characters.Add(monsterList[1]);
                Game1.currentLocation.characters.Add(monsterList[2]);
                Game1.currentLocation.characters.Add(monsterList[3]);
            }
            else
            {
                // Have the dream lord back away from the player
                base.Slipperiness = 8;
                Point standingTile = base.StandingPixel;
                base.setTrajectory(
                        -(int)Utility.getVelocityTowardPlayer(standingTile, 2f, base.Player).X,
                        -(int)(0f - Utility.getVelocityTowardPlayer(standingTile, 2f, base.Player).Y)
                    );
            }
        }

        private void SquidKidBehavior (GameTime time) 
        {
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
                    this.firingTimer = 200f;
                    // If there are no fireballs left, set a delay till next one can be fired
                    if (this.numFireballsLeft <= 0)
                    {
                        this.lastFireball = Game1.random.Next(1500, 3250);
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
                this.lastFireball = Game1.random.Next(600, 1750);
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
                    base.setTrajectory(
                        (int)Utility.getVelocityTowardPlayer(standingTile, 4f, base.Player).X, 
                        (int)(0f - Utility.getVelocityTowardPlayer(standingTile, 4f, base.Player).Y)
                    );
                }
            }
        }
    }
}
