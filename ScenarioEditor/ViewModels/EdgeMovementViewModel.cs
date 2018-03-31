using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class EdgeMovementViewModel : BaseViewModel
    {
        public EdgeMovementViewModel(EdgeType edgeType, bool traversable, int movementCost)
        {
            EdgeType = edgeType.ToString();
            Traversable = traversable;
            MovementCost = movementCost;
        }

        public string EdgeType { get; set; }
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
    }
}
