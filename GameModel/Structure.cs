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
        Fortress,
        Town,
        City
    }

    
    public class Structure
    {
        public static Dictionary<StructureType, double> StructureDefenceModifiers = new Dictionary<StructureType, double>
            {
                { StructureType.City, .4 },
                { StructureType.Town, .6 },
                { StructureType.Fortress, .8 },
            };

        public int Index;
        public StructureType StructureType { get; set; }
        public string Name { get; set; }
        public Tile Location;
        public int OwnerIndex;
        public float Supply;
        

        public Structure(int index, StructureType structureType, Tile tile, int ownerIndex = 0, int supply = 10, string name = "Structure")
        {
            Index = index;
            StructureType = structureType;
            Location = tile;
            OwnerIndex = ownerIndex;
            Supply = supply;
            Name = name == "Structure" ? $"Structure {index}" : name;

            if (Location != null)
            {
                Location.Structure = this;
            }            
        }

        
    }
}
