using GameModel;
using GameModel.Rendering;
using Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace ScenarioEditor.ViewModels
{
    public class MilitaryUnitInstanceViewModel : MilitaryUnitTemplateViewModel
    {
        private MilitaryUnitInstance _militaryUnitInstance;
        ArgbColour _factionColour;
        public MilitaryUnitInstanceViewModel(MilitaryUnitInstance militaryUnit, ArgbColour factionColour) : base (militaryUnit)
        {
            _militaryUnitInstance = militaryUnit;
            EditTerrainMovements = false;
            EditEdgeMovements = false;
            EditBaseCharacteristics = false;
            EditCombatModifiers = false;
            _factionColour = factionColour;
        }

        public Brush FactionColour
        {
            get
            {
                return new SolidColorBrush(GameRenderingWpf.ArgbColourToColor(_factionColour));
            }
        }

        public int LocationIndex { get { return _militaryUnitInstance.LocationIndex; } }
    }
}