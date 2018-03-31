using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScenarioEditor.ViewModels
{
    public class FactionViewModel : BaseViewModel
    {
        Faction _faction;

        public FactionViewModel(Faction faction)
        {
            _faction = faction;
        }

        public int Id { get { return _faction.Id; } }

        public string Name
        {
            get
            {
                return _faction.Name;
            }

            set
            {
                if (value == _faction.Name)
                    return;

                _faction.Name = value;
                RaisePropertyChanged();
            }
        }

        public short Red
        {
            get
            {
                return _faction.Colour.Red;
            }

            set
            {
                if (value == _faction.Colour.Red)
                    return;

                _faction.Colour = new GameModel.Rendering.ArgbColour(value, _faction.Colour.Green, _faction.Colour.Blue);
                RaisePropertyChanged(nameof(Color));
            }
        }

        public short Green
        {
            get
            {
                return _faction.Colour.Green;
            }

            set
            {
                if (value == _faction.Colour.Green)
                    return;

                _faction.Colour = new GameModel.Rendering.ArgbColour(_faction.Colour.Red, value, _faction.Colour.Blue);
                RaisePropertyChanged(nameof(Color));
            }
        }

        public short Blue
        {
            get
            {
                return _faction.Colour.Blue;
            }

            set
            {
                if (value == _faction.Colour.Blue)
                    return;

                _faction.Colour = new GameModel.Rendering.ArgbColour(_faction.Colour.Red, _faction.Colour.Green, value);
                RaisePropertyChanged(nameof(Color));
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                var color = GameRenderingWpf.ArgbColourToColor(_faction.Colour);
                return new SolidColorBrush(color);
            }
        }
    }
}
