using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AccessibleTiles.Modules.GridMovement {
    internal class GridMovement {

        private readonly ModEntry Mod;

        public Boolean is_warping = false;
        public Boolean is_moving = false;

        //stop player from moving too fast
        int minMillisecondsBetweenSteps = 150;
        Timer timer = new Timer();

        public GridMovement(ModEntry mod) {
            this.Mod = mod;

            //set is_moving after x time to allow the next grid movement
            timer.Interval = minMillisecondsBetweenSteps;
            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            is_moving = false;
            timer.Stop();
        }

        public void HandleGridMovement(int direction, InputButton pressedButton) {

            if (this.is_warping == true || is_moving || Game1.IsChatting) return;

            is_moving = true;
            timer.Start();

            this.Mod.LastGridMovementButtonPressed = pressedButton;
            this.Mod.LastGridMovementDirection = direction;

            Mod.Output($"Move Direction: {direction}");

            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;

            if (Game1.player.FacingDirection != direction) {
                Game1.player.faceDirection(direction);
                Game1.playSound("dwop");
                return;
            }

            Vector2 tileLocation = player.getTileLocation();

            switch(direction) {
                case 0:
                    tileLocation = Vector2.Add(tileLocation, new(0, -1));
                    break;
                case 1:
                    tileLocation = Vector2.Add(tileLocation, new(1, 0));
                    break;
                case 2:
                    tileLocation = Vector2.Add(tileLocation, new(0, 1));
                    break;
                case 3:
                    tileLocation = Vector2.Add(tileLocation, new(-1, 0));
                    break;
            }

            Mod.Output($"Move To: {tileLocation.ToString()}");
            player.CanMove = false;

            Warp warp = location.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize));
            if (warp != null) {

                Mod.Output("Collides with Warp or Door");

                if (location.checkAction(new Location((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize), Game1.viewport, Game1.player)) {
                    this.is_warping = true;
                } else {
                    Game1.playSound("doorClose");
                    Game1.player.warpFarmer(warp);
                    this.is_warping = true;
                }

                player.CanMove = true;
                return;

            }

            PathFindController pathfinder = new PathFindController(player, location, tileLocation.ToPoint(), direction);
            if(pathfinder.pathToEndPoint != null) {
                //valid point
                player.Position = tileLocation * Game1.tileSize;
                player.CanMove = true;
                location.playTerrainSound(tileLocation);
                CenterPlayer();

            }

        }

        internal void PlayerWarped(object sender, WarpedEventArgs e) {
            Game1.player.canMove = true;
            this.is_warping = false;
        }

        private void CenterPlayer() {
            Game1.player.Position = Vector2.Divide(Game1.player.Position, Game1.tileSize) * Game1.tileSize;
        }

    }
}
