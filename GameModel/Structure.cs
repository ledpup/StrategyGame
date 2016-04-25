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

        public int Id;
        public Point Location;
        public int OwnerId;
        public float Supply;

        public Structure(int id, Point location, StructureType structureType, int ownerId = 1, int supply = 10)
        {
            Id = id;
            Location = location;
            StructureType = structureType;
            OwnerId = ownerId;
            Supply = supply;
        }

        public ArgbColour Colour { get { return Player.Colour(OwnerId); } }
        public StructureType StructureType { get; set;}
    }
}
