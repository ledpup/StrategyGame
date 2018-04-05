using GameModel;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class TerrainMovementViewModel : BaseViewModel
    {
        UnitTerrainMovement _unitTerrainMovement;
        public TerrainMovementViewModel(UnitTerrainMovement unitTerrainMovement)
        {
            _unitTerrainMovement = unitTerrainMovement;
        }
        public string TerrainType { get { return _unitTerrainMovement.TerrainType.ToString(); } }
        public bool Traversable
        {
            get
            {
                return _unitTerrainMovement.Traversable;
            }
            set
            {
                if (value == _unitTerrainMovement.Traversable)
                    return;

                _unitTerrainMovement.Traversable = value;
                RaisePropertyChanged();
            }
        }

        public int MovementCost
        {
            get
            {
                return _unitTerrainMovement.MovementCost;
            }
            set
            {
                if (value == _unitTerrainMovement.MovementCost)
                    return;

                _unitTerrainMovement.MovementCost = value;
                RaisePropertyChanged();
            }
        }

        public bool CanStopOn
        {
            get
            {
                return _unitTerrainMovement.CanStopOn;
            }
            set
            {
                if (value == _unitTerrainMovement.CanStopOn)
                    return;

                _unitTerrainMovement.CanStopOn = value;
                RaisePropertyChanged();
            }
        }
    }
}
