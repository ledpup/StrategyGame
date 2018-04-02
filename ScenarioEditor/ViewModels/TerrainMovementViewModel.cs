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
            TerrainType = terrainType.ToString();
            Traversable = true;
            MovementCost = 1;
            CanStopOn = true;
        }
        public string TerrainType { get; private set; }
        public bool Traversable
        {
            get
            {
                return _traversable;
            }
            set
            {
                if (value == _traversable)
                    return;

                _traversable = value;
                RaisePropertyChanged();
            }
        }
        bool _traversable;
        public int MovementCost { get; set; }
        public bool CanStopOn { get; set; }
    }
}
