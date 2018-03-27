using GameModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class StructureViewModel : BaseViewModel
    {
        

        Structure _structure;


        public StructureViewModel(Structure structure)
        {
            _structure = structure;
        }

        public string Name
        {
            get
            {
                return _structure.Name;
            }

            set
            {
                if (value == _structure.Name)
                    return;

                _structure.Name = value;
                RaisePropertyChanged();
            }
        }

        public string StructureType
        {
            get { return _structure.StructureType.ToString(); }
        }


    }
}
