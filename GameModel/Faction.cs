using GameModel.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameModel
{
    public class Faction
    {
        public Faction(int id, string name, ArgbColour colour)
        {
            Id = id;
            Name = name;
            Colour = colour;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public ArgbColour Colour { get; set; }
    }
}
