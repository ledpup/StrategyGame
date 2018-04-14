using GameModel;
using GameModel.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class MilitaryUnitInstance : IMilitaryUnit
    {
        
        public MilitaryUnitInstance()
        { }
        public MilitaryUnitInstance(int id, int unitNumber, string name, int factionId, int locationIndex, MilitaryUnitTemplate militaryUnitTemplate)
        {
            Id = id;

            FactionId = factionId;
            LocationIndex = locationIndex;

            MovementType = militaryUnitTemplate.MovementType;
            UsesRoads = militaryUnitTemplate.UsesRoads;
            RoadMovementBonusPoints = militaryUnitTemplate.RoadMovementBonusPoints;
            CanTransport = militaryUnitTemplate.CanTransport;
            MovementTypesTransportableBy = militaryUnitTemplate.MovementTypesTransportableBy;
            MovementPoints = militaryUnitTemplate.MovementPoints;
            CombatType = militaryUnitTemplate.CombatType;
            CombatAbility = militaryUnitTemplate.CombatAbility;
            DepletionOrder = militaryUnitTemplate.DepletionOrder;
            Size = militaryUnitTemplate.Size;
            Members = militaryUnitTemplate.Members;
            Morale = militaryUnitTemplate.Morale;
            TerrainMovements = militaryUnitTemplate.TerrainMovements;
            EdgeMovements = militaryUnitTemplate.EdgeMovements;
            NumberingConvention = militaryUnitTemplate.NumberingConvention;
            UnitTemplateId = militaryUnitTemplate.Id;

            string unitName = name;
            switch (militaryUnitTemplate.NumberingConvention)
            {
                case NumberingConvention.Alphabetic:
                    unitName = $"{name} {unitNumber.ToAlphabetic()}";
                    break;
                case NumberingConvention.Ordinal:
                    unitName = $"{unitNumber.ToOrdinal()} {name}";
                    break;
                case NumberingConvention.RomanNumeral:
                    unitName = $"{unitNumber.ToRoman()} {name}";
                    break;
            }
            Name = unitName;
        }

        public int Id { get; private set; }
        public string Name { get; set; }
        public int FactionId { get; set; }
        public MovementType MovementType { get; set; }
        public bool UsesRoads { get; set; }
        public int RoadMovementBonusPoints { get; set; }
        public bool CanTransport { get; set; }
        public List<MovementType> MovementTypesTransportableBy { get; set; }
        public int MovementPoints { get; set; }
        public CombatType CombatType { get; set; }
        public int CombatAbility { get; set; }
        public int DepletionOrder { get; set; }
        public float Size { get; set; }
        public int Members { get; set; }
        public int Morale { get; set; }

        public List<UnitTerrainMovement> TerrainMovements { get; set; }
        public List<UnitEdgeMovement> EdgeMovements { get; set; }


        public int LocationIndex { get; }
        public int UnitTemplateId { get; set; }
        public NumberingConvention NumberingConvention { get; set; }
    }
}
 