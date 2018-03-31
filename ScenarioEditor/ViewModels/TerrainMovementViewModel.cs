using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class TerrainMovementViewModel : BaseViewModel
    {
        public TerrainMovementViewModel(TerrainType terrainType)
        {
            TerrainType = TerrainType;
            Traversable = true;
            MovementCost = 1;
            CanStopOn = true;
        }
        public TerrainType TerrainType { get; private set; }
        public bool Traversable { get; set; }
        public int MovementCost { get; set; }
        public bool CanStopOn { get; set; }
    }
}
