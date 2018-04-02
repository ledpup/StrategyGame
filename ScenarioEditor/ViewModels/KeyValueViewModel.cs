using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenarioEditor.ViewModels
{
    public class KeyValueViewModel<T> : BaseViewModel
    {
        public KeyValueViewModel(T key)
        {
            Key = key;
        }
        public T Key { get; private set; }

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == _value)
                    return;

                _value = value;
                RaisePropertyChanged();
            }
        }
        double _value;
    }
}
