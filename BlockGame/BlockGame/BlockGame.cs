using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockGame : GameComponent
    {
        private BlockCreationPlayer blockCreator;
        //private BlockPlacerPlayer blockPlacer;


        public BlockGame(Game game) : base(game)
        {
            blockCreator = new BlockCreationHumanPlayer();
        }

        public override void Initialize()
        {
            
            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SkeletonStreamManager skeletonManager = (SkeletonStreamManager)this.Game.Services.GetService(typeof(SkeletonStreamManager));
            if(skeletonManager.currentSkeleton!=null)
            {
                PoseStatus poseStatus = blockCreator.GetBlock(skeletonManager.currentSkeleton);
                System.Diagnostics.Debug.WriteLine(poseStatus);
            }
        }
    }
}
