using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
        }
    }
}
