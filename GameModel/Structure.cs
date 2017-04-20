using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum StructureType
    {
        None,
        Outpost,
        Fortress,
        City
    }

    
    public class Structure
    {
        public static Dictionary<StructureType, double> StructureDefenceModifiers = new Dictionary<StructureType, double>
            {
                { StructureType.City, .4 },
                { StructureType.Fortress, .6 },
                { StructureType.Outpost, .8 },
            };

        public int Index;
        public Tile Location;
        public int OwnerIndex;
        public float Supply;

        public Structure(int index, StructureType structureType, Tile tile, int ownerIndex = 0, int supply = 10)
        {
            Index = index;
            StructureType = structureType;
            Location = tile;
            OwnerIndex = ownerIndex;
            Supply = supply;

            tile.Structure = this;
        }

        public ArgbColour Colour { get { return Player.Colour(OwnerIndex); } }
        public StructureType StructureType { get; set;}
    }
}
