using GameModel;
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
        
        public MilitaryUnitInstanceViewModel(IMilitaryUnit militaryUnit) : base (militaryUnit)
        {
            _militaryUnitInstance = (MilitaryUnitInstance)militaryUnit;
            EditTerrainMovements = false;
            EditEdgeMovements = false;
            EditBaseCharacteristics = false;
            EditCombatModifiers = false;
        }

        public Brush FactionColour
        {
            get
            {
                return new SolidColorBrush(GameRenderingWpf.ArgbColourToColor(_militaryUnitInstance.FactionColour));
            }
        }

        
    }
}