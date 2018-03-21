using GameModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor
{
    public class StructureViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
