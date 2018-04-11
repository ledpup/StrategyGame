using GameModel;
using GameModel.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class Scenario
    {
        public Scenario(string folder)
        {
            Folder = folder;

            var gameBoard = File.ReadAllLines($"{Folder}\\Board.txt");
            var tileEdges = File.ReadAllLines($"{Folder}\\Edges.txt");
            var structures = File.ReadAllLines($"{Folder}\\Structures.txt");

            Board = new Board(gameBoard, tileEdges, structures);

            Board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0) { Location = Board[1, 1] },
                new MilitaryUnit(1) { Location = Board[1, 1] },
                new MilitaryUnit(2) { Location = Board[1, 1] },
                new MilitaryUnit(3) { Location = Board[1, 1], OwnerIndex = 1 },
                new MilitaryUnit(4, "1st Fleet", 0, Board[0, 0], MovementType.Water),
                new MilitaryUnit(4, "1st Airborne", 0, Board[3, 2], MovementType.Airborne),
            };

            

            Load();

            Factions = new List<Faction>();
            for (var i = 0; i < 4; i++)
            {
                var faction = new Faction(i, $"Faction {i}", GameRenderer.PlayerColour(i));
                Factions.Add(faction);
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public string Folder { get; set; }
        public Board Board { get; set; }

        public List<MilitaryUnitTemplate> MilitaryUnitTemplates { get; set; }
        public List<Faction> Factions { get; set; }
        public void PlaceStructure (StructureType structureType, int faction, int x, int y)
        {
            if (structureType == StructureType.None)
            {
                Board.Structures.Remove(Board[x, y].Structure);
                Board[x, y].Structure = null;
            }
            else if (Board[x, y].Structure != null)
            {
                Board[x, y].Structure.OwnerIndex = faction;
                Board[x, y].Structure.StructureType = structureType;
            }
            else
            {
                Board.Structures.Remove(Board[x, y].Structure);
                var newStructure = new Structure(Board[x, y].Index, structureType, Board[x, y], faction);

                Board[x, y].Structure = newStructure;
                Board.Structures.Add(newStructure);
            }
        }
        public MilitaryUnit PlaceUnit(MilitaryUnitTemplate unitTemplate, int selectedFaction, int x, int y)
        {
            var militaryUnit = new MilitaryUnit(Board.Units.Count + 1, unitTemplate.Name, selectedFaction, Board[x, y], unitTemplate.MovementType, unitTemplate.MovementPoints, unitTemplate.RoadMovementBonusPoints, unitTemplate.CombatType, unitTemplate.CombatAbility, unitTemplate.Members, unitTemplate.Size, unitTemplate.CanTransport, unitTemplate.MovementTypesTransportableBy, unitTemplate.DepletionOrder, unitTemplate.Morale, 0);
            Board.Units.Add(militaryUnit);
            return militaryUnit;
        }

        public void Load()
        {
            var militaryUnitTemplates = File.ReadAllText($"{Folder}\\MilitaryUnitTemplates.json");
            MilitaryUnitTemplates = JsonConvert.DeserializeObject<List<MilitaryUnitTemplate>>(militaryUnitTemplates);
        }

        public void Save()
        {
            var militaryUnitTemplates = JsonConvert.SerializeObject(MilitaryUnitTemplates, Formatting.Indented);
            File.WriteAllText($"{Folder}\\MilitaryUnitTemplates.json", militaryUnitTemplates);
        }


    }
}
