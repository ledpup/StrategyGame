using GameModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScenarioEditor.ViewModels
{
    public class MilitaryUnitTemplateViewModel : BaseViewModel
    {
        private MilitaryUnitTemplate _militaryUnitTemplate;
        
        public MilitaryUnitTemplateViewModel(MilitaryUnitTemplate militaryUnitTemplate)
        {
            _militaryUnitTemplate = militaryUnitTemplate;

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
            foreach (CombatType ct in Enum.GetValues(typeof(CombatType)))
            {
                CombatTypesSelection.Add(new CombatTypeSelector(ct, ct == militaryUnitTemplate.CombatType));
            }

            TerrainMovementViewModels = new List<TerrainMovementViewModel>();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                var traversable = true;
                var movementCost = 1;
                var canStopOn = true;
                switch (_militaryUnitTemplate.MovementType)
                {
                    case MovementType.Airborne:
                        if (Terrain.All_Water.HasFlag(terrainType))
                        {
                            canStopOn = false;
                        }
                        break;
                    case MovementType.Land:
                        if (Terrain.All_Water.HasFlag(terrainType))
                        {
                            traversable = false;
                            canStopOn = false;
                        }
                        if (Terrain.Rough_Land.HasFlag(terrainType))
                        {
                            movementCost = 2;
                        }
                        break;
                    case MovementType.Water:
                        if (Terrain.All_Land.HasFlag(terrainType))
                        {
                            traversable = false;
                            canStopOn = true;
                        }
                        break;
                }
                if (terrainType == TerrainType.Mountain)
                {
                    canStopOn = false;
                }

                TerrainMovementViewModels.Add(new TerrainMovementViewModel(terrainType, traversable, movementCost, canStopOn));
            }

            EdgeMovementCosts = new List<EdgeMovementViewModel>();
            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                var traversable = true;
                var movementCost = 0;
                switch (_militaryUnitTemplate.MovementType)
                {
                    case MovementType.Land:
                        if (Edge.All_Water.HasFlag(edgeType) && edgeType != EdgeType.None)
                        {
                            traversable = false;
                        }
                        if (Edge.All_Land.HasFlag(edgeType))
                        {
                            movementCost = 1;
                        }
                        break;
                    case MovementType.Water:
                        if (Edge.All_Land.HasFlag(edgeType))
                        {
                            traversable = false;
                        }
                        break;
                }
                EdgeMovementCosts.Add(new EdgeMovementViewModel(edgeType, traversable, movementCost));
            }
        }

        public List<TerrainMovementViewModel> TerrainMovementViewModels { get; private set; }
        public List<EdgeMovementViewModel> EdgeMovementCosts { get; private set; }
        public string Name
        {
            get { return _militaryUnitTemplate.Name; }
            set
            {
                if (value == _militaryUnitTemplate.Name)
                    return;

                _militaryUnitTemplate.Name = value;
                RaisePropertyChanged();
            }
        }

        public List<MovementTypeSelector> MovementTypesSelection { get; set; }
        public MovementTypeSelector MovementTypeSelectedItem
        {
            get { return MovementTypesSelection.Single(x => x.IsSelected); }
            set
            {
                if (value.Name == _militaryUnitTemplate.MovementType.ToString())
                    return;

                _militaryUnitTemplate.MovementType = (MovementType)Enum.Parse(typeof(MovementType), value.Name);
                RaisePropertyChanged();
            }
        }

        public MovementType MovementType
        {
            get { return _militaryUnitTemplate.MovementType; }
            set
            {
                if (value == _militaryUnitTemplate.MovementType)
                    return;

                _militaryUnitTemplate.MovementType = value;
                RaisePropertyChanged();
            }
        }

        public bool UsesRoads
        {
            get { return _militaryUnitTemplate.UsesRoads; }
            set
            {
                if (value == _militaryUnitTemplate.UsesRoads)
                    return;

                _militaryUnitTemplate.UsesRoads = value;
                RaisePropertyChanged();
            }
        }

        public int MovementPoints
        {
            get { return _militaryUnitTemplate.MovementPoints; }
            set
            {
                if (value == _militaryUnitTemplate.MovementPoints)
                    return;

                _militaryUnitTemplate.MovementPoints = value;
                RaisePropertyChanged();
            }
        }

        public bool CanTransport
        {
            get { return _militaryUnitTemplate.CanTransport; }
            set
            {
                if (value == _militaryUnitTemplate.CanTransport)
                    return;

                _militaryUnitTemplate.CanTransport = value;
                RaisePropertyChanged();
            }
        }


        
        public List<MovementTypeSelector> MovementTypesTransportableBySelection { get; set; }

        public List<MovementType> MovementTypesTransportableBy
        {
            get { return _militaryUnitTemplate.MovementTypesTransportableBy; }
            set
            {
                if (value == _militaryUnitTemplate.MovementTypesTransportableBy)
                    return;

                _militaryUnitTemplate.MovementTypesTransportableBy = value;
                RaisePropertyChanged();
            }
        }

        public int Members
        {
            get { return _militaryUnitTemplate.Members; }
            set
            {
                if (value == _militaryUnitTemplate.Members)
                    return;

                _militaryUnitTemplate.Members = value;
                RaisePropertyChanged();
            }
        }

        public float Size
        {
            get { return _militaryUnitTemplate.Size; }
            set
            {
                if (value == _militaryUnitTemplate.Size)
                    return;

                _militaryUnitTemplate.Size = value;
                RaisePropertyChanged();
            }
        }

        public List<CombatTypeSelector> CombatTypesSelection { get; set; }
        public CombatTypeSelector CombatTypeSelectedItem
        {
            get { return CombatTypesSelection.Single(x => x.IsSelected); }
            set
            {
                if (value.Name == _militaryUnitTemplate.CombatType.ToString())
                    return;

                _militaryUnitTemplate.CombatType = (CombatType)Enum.Parse(typeof(CombatType), value.Name);
                RaisePropertyChanged();
            }
        }

        public int CombatQuality
        {
            get { return _militaryUnitTemplate.CombatAbility; }
            set
            {
                if (value == _militaryUnitTemplate.CombatAbility)
                    return;

                _militaryUnitTemplate.CombatAbility = value;
                RaisePropertyChanged();
            }
        }

        public int DepletionOrder
        {
            get { return _militaryUnitTemplate.DepletionOrder; }
            set
            {
                if (value == _militaryUnitTemplate.DepletionOrder)
                    return;

                _militaryUnitTemplate.DepletionOrder = value;
                RaisePropertyChanged();
            }
        }

        public int Morale
        {
            get { return _militaryUnitTemplate.Morale; }
            set
            {
                if (value == _militaryUnitTemplate.Morale)
                    return;

                _militaryUnitTemplate.Morale = value;
                RaisePropertyChanged();
            }
        }
    }
}