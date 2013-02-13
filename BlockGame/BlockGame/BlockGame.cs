using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGame
{
    class BlockProgram
    {

        public static void Main()
        {
            using (MainGame game = new MainGame())
            {
                game.Run();
            }
        }
    }
}
