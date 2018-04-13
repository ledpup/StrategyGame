using GameModel;
using Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScenarioEditor.ViewModels
{
    public class MilitaryUnitTemplateViewModel : BaseViewModel
    {
        private IMilitaryUnit _militaryUnit;
        
        public MilitaryUnitTemplateViewModel(IMilitaryUnit militaryUnitTemplate)
        {
            _militaryUnit = militaryUnitTemplate;

            MovementTypesSelection = new List<MovementTypeSelector>();
            foreach (MovementType mt in Enum.GetValues(typeof(MovementType)))
            {
                MovementTypesSelection.Add(new MovementTypeSelector(mt, mt == militaryUnitTemplate.MovementType));
            }

            MovementTypesTransportableBySelection = new List<MovementTypeSelector>();
            foreach (MovementType movementType in Enum.GetValues(typeof(MovementType)))
            {
                MovementTypesTransportableBySelection.Add(new MovementTypeSelector(movementType, militaryUnitTemplate.MovementTypesTransportableBy.Contains(movementType)));
            }

            CombatTypesSelection = new List<CombatTypeSelector>();
            EnemyCombatTypeCombatModifiers = new List<KeyValueViewModel<CombatType>>();
            foreach (CombatType ct in Enum.GetValues(typeof(CombatType)))
            {
                CombatTypesSelection.Add(new CombatTypeSelector(ct, ct == militaryUnitTemplate.CombatType));
                EnemyCombatTypeCombatModifiers.Add(new KeyValueViewModel<CombatType>(ct));
            }

            TerrainMovementViewModels = _militaryUnit.TerrainMovements.Select(x => new TerrainMovementViewModel(x)).ToList();

            TerrainCombatModifiers = new List<KeyValueViewModel<TerrainType>>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                TerrainCombatModifiers.Add(new KeyValueViewModel<TerrainType>(terrainType));
            }

            EdgeMovementCosts = _militaryUnit.EdgeMovements.Select(x => new EdgeMovementViewModel(x)).ToList();

            WeatherCombatModifiers = new List<KeyValueViewModel<Weather>>();
            foreach (Weather weather in Enum.GetValues(typeof(Weather)))
            {
                WeatherCombatModifiers.Add(new KeyValueViewModel<Weather>(weather));
            }

            StructureCombatModifiers = new List<KeyValueViewModel<StructureType>>();
            foreach (StructureType structureType in Enum.GetValues(typeof(StructureType)))
            {
                StructureCombatModifiers.Add(new KeyValueViewModel<StructureType>(structureType));
            }
        }

        public List<KeyValueViewModel<StructureType>> StructureCombatModifiers { get; set; }
        public List<KeyValueViewModel<CombatType>> EnemyCombatTypeCombatModifiers { get; set; }
        public List<KeyValueViewModel<Weather>> WeatherCombatModifiers { get; set; }
        public List<KeyValueViewModel<TerrainType>> TerrainCombatModifiers { get; set; }
        public List<TerrainMovementViewModel> TerrainMovementViewModels { get; private set; }
        public List<EdgeMovementViewModel> EdgeMovementCosts { get; private set; }

        public int Id { get { return _militaryUnit.Id; } }
        public string Name
        {
            get { return _militaryUnit.Name; }
            set
            {
                if (value == _militaryUnit.Name)
                    return;

                _militaryUnit.Name = value;
                RaisePropertyChanged();
            }
        }

        public List<MovementTypeSelector> MovementTypesSelection { get; set; }
        public MovementTypeSelector MovementTypeSelectedItem
        {
            get { return MovementTypesSelection.Single(x => x.IsSelected); }
            set
            {
                if (value.Name == _militaryUnit.MovementType.ToString())
                    return;

                _militaryUnit.MovementType = (MovementType)Enum.Parse(typeof(MovementType), value.Name);
                RaisePropertyChanged();
            }
        }

        public MovementType MovementType
        {
            get { return _militaryUnit.MovementType; }
            set
            {
                if (value == _militaryUnit.MovementType)
                    return;

                _militaryUnit.MovementType = value;
                RaisePropertyChanged();
            }
        }

        public bool UsesRoads
        {
            get { return _militaryUnit.UsesRoads; }
            set
            {
                if (value == _militaryUnit.UsesRoads)
                    return;

                _militaryUnit.UsesRoads = value;
                RaisePropertyChanged();
            }
        }

        public int MovementPoints
        {
            get { return _militaryUnit.MovementPoints; }
            set
            {
                if (value == _militaryUnit.MovementPoints)
                    return;

                _militaryUnit.MovementPoints = value;
                RaisePropertyChanged();
            }
        }

        public bool CanTransport
        {
            get { return _militaryUnit.CanTransport; }
            set
            {
                if (value == _militaryUnit.CanTransport)
                    return;

                _militaryUnit.CanTransport = value;
                RaisePropertyChanged();
            }
        }


        
        public List<MovementTypeSelector> MovementTypesTransportableBySelection { get; set; }

        public List<MovementType> MovementTypesTransportableBy
        {
            get { return _militaryUnit.MovementTypesTransportableBy; }
            set
            {
                if (value == _militaryUnit.MovementTypesTransportableBy)
                    return;

                _militaryUnit.MovementTypesTransportableBy = value;
                RaisePropertyChanged();
            }
        }

        public int Members
        {
            get { return _militaryUnit.Members; }
            set
            {
                if (value == _militaryUnit.Members)
                    return;

                _militaryUnit.Members = value;
                RaisePropertyChanged();
            }
        }

        public float Size
        {
            get { return _militaryUnit.Size; }
            set
            {
                if (value == _militaryUnit.Size)
                    return;

                _militaryUnit.Size = value;
                RaisePropertyChanged();
            }
        }

        public List<CombatTypeSelector> CombatTypesSelection { get; set; }
        public CombatTypeSelector CombatTypeSelectedItem
        {
            get { return CombatTypesSelection.Single(x => x.IsSelected); }
            set
            {
                if (value.Name == _militaryUnit.CombatType.ToString())
                    return;

                _militaryUnit.CombatType = (CombatType)Enum.Parse(typeof(CombatType), value.Name);
                RaisePropertyChanged();
            }
        }

        public int CombatQuality
        {
            get { return _militaryUnit.CombatAbility; }
            set
            {
                if (value == _militaryUnit.CombatAbility)
                    return;

                _militaryUnit.CombatAbility = value;
                RaisePropertyChanged();
            }
        }

        public int DepletionOrder
        {
            get { return _militaryUnit.DepletionOrder; }
            set
            {
                if (value == _militaryUnit.DepletionOrder)
                    return;

                _militaryUnit.DepletionOrder = value;
                RaisePropertyChanged();
            }
        }

        public int Morale
        {
            get { return _militaryUnit.Morale; }
            set
            {
                if (value == _militaryUnit.Morale)
                    return;

                _militaryUnit.Morale = value;
                RaisePropertyChanged();
            }
        }
        public bool EditTerrainMovements { get; set; } = true;
        public bool EditEdgeMovements { get; set; } = true;
        public bool EditBaseCharacteristics { get; set; } = true;

        public bool EditRoadMovementBonusPoints { get { return EditBaseCharacteristics && UsesRoads; } }

        public bool EditCombatModifiers { get; set; } = true;
    }
}