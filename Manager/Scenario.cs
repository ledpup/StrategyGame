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
        public List<MilitaryUnitInstance> MilitaryUnitInstances { get; set; }
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
        public MilitaryUnitInstance PlaceUnit(MilitaryUnitTemplate unitTemplate, int selectedFactionId, int x, int y)
        {
            var unitNumber = MilitaryUnitInstances.Count(u => u.FactionId == selectedFactionId && u.UnitTemplateId == unitTemplate.Id) + 1;

            var militaryUnitInstance = new MilitaryUnitInstance(MilitaryUnitInstances.Count + 1, unitNumber, unitTemplate.Name, selectedFactionId, Board[x, y].Index, unitTemplate);
            MilitaryUnitInstances.Add(militaryUnitInstance);

            return militaryUnitInstance;
        }



        public void Load()
        {
            var militaryUnitTemplates = File.ReadAllText($"{Folder}\\MilitaryUnitTemplates.json");
            MilitaryUnitTemplates = JsonConvert.DeserializeObject<List<MilitaryUnitTemplate>>(militaryUnitTemplates);

            MilitaryUnitInstances = new List<MilitaryUnitInstance>();
            if (File.Exists($"{Folder}\\MilitaryUnitInstances.json"))
            {
                var militaryUnitInstances = File.ReadAllText($"{Folder}\\MilitaryUnitInstances.json");
                MilitaryUnitInstances = JsonConvert.DeserializeObject<List<MilitaryUnitInstance>>(militaryUnitInstances);
            }
        }

        public void Save()
        {
            var militaryUnitTemplates = JsonConvert.SerializeObject(MilitaryUnitTemplates, Formatting.Indented);
            File.WriteAllText($"{Folder}\\MilitaryUnitTemplates.json", militaryUnitTemplates);

            var militaryUnitInstances = JsonConvert.SerializeObject(MilitaryUnitInstances, Formatting.Indented);
            File.WriteAllText($"{Folder}\\MilitaryUnitInstances.json", militaryUnitInstances);
        }


    }
}
