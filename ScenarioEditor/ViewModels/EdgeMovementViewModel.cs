using GameModel;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class EdgeMovementViewModel : BaseViewModel
    {
        UnitEdgeMovement _unitEdgeMovement;
        public EdgeMovementViewModel(UnitEdgeMovement unitEdgeMovement)
        {
            _unitEdgeMovement = unitEdgeMovement;
        }

        public string EdgeType { get { return _unitEdgeMovement.EdgeType.ToString(); } }
        public bool Traversable
        {
            get
            {
                return _unitEdgeMovement.Traversable;
            }
            set
            {
                if (value == _unitEdgeMovement.Traversable)
                    return;

                _unitEdgeMovement.Traversable = value;
                RaisePropertyChanged();
            }
        }

        public int MovementCost
        {
            get
            {
                return _unitEdgeMovement.MovementCost;
            }
            set
            {
                if (value == _unitEdgeMovement.MovementCost)
                    return;

                _unitEdgeMovement.MovementCost = value;
                RaisePropertyChanged();
            }
        }
    }
}
