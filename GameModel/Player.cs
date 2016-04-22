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

        internal static ArgbColour Colour(int ownerId)
        {
            return ownerId == 1 ? Colours.Red : Colours.Blue;
        }
    }
}
