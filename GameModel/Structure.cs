using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum StructureType
    {
        Outpost,
        Fortress,
        City
    }
    public class Structure
    {
        public int Id;
        public Point Location;
        public int OwnerId;
        public float Supply;

        public Structure(int id, Point location, string structureType, int ownerId, int supply)
        {
            Id = id;
            Location = location;
            StructureType = (StructureType)Enum.Parse(typeof(StructureType), structureType);
            OwnerId = ownerId;
            Supply = supply;
        }

        public StructureType StructureType { get; set;}
    }
}
