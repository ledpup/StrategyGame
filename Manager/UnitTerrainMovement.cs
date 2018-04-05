using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class UnitTerrainMovement
    {
        public UnitTerrainMovement(TerrainType terrainType)
        {
            TerrainType = terrainType.ToString();
            Traversable = true;
            MovementCost = 1;
            CanStopOn = true;
        }
        public string TerrainType { get; private set; }
        public bool Traversable { get; set; }
        public int MovementCost { get; set; }
        public bool CanStopOn { get; set; }
    }
}
