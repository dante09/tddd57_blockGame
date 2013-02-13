using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlockGame
{
    class BlockGame : GameComponent
    {
        private BlockCreationPlayer bcPlayer;

        public BlockGame(Game game) : base(game)
        {
            bcPlayer = new BlockCreationHumanPlayer(game);
            game.Components.Add(bcPlayer);

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
