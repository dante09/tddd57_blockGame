using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlockGame
{
    abstract class BlockCreationPlayer : Player
    {
        protected BlockCreationRenderer bcRenderer;

        public BlockCreationPlayer(Game game)
            : base(game)
        {
            bcRenderer = new BlockCreationRenderer(game);
            game.Components.Add(bcRenderer);
        }
    }
}
