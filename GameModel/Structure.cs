using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public enum StructureType
    {
        Fortress,
        City
    }
    public class Structure
    {
        public int Id;
        public Point Location;

        public Structure(int id, Point location, string structureType)
        {
            Id = id;
            Location = location;
            StructureType = (StructureType)Enum.Parse(typeof(StructureType), structureType);

        }

        public StructureType StructureType { get; set;}
    }
}
