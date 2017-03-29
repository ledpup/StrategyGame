using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public class Player
    {
        public int Id;

        public override int GetHashCode()
        {
            return Id;
        }

        internal static ArgbColour Colour(int playerIndex)
        {
            return playerIndex == 0 ? Colours.Blue : Colours.Red;
        }
    }
}
