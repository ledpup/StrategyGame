using GameModel;
using GameModel.Rendering;
using GameRendering2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using HexGridControl;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ScenarioEditor.ViewModels;

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var militaryUnitTemplates = new List<MilitaryUnitTemplate>
            {
                new MilitaryUnitTemplate("Dwarf Infantry", usesRoads: true),
                new MilitaryUnitTemplate("Human Cavalry", movementPoints: 3, usesRoads: true),
                new MilitaryUnitTemplate("Grifon", MovementType.Airborne, 3, false, true),
            };

            var militaryUnitTemplateViewModels = militaryUnitTemplates.Select(x => new MilitaryUnitTemplateViewModel(x)).ToList();
            MilitaryUnitTemplateViewModels = new ObservableCollection<MilitaryUnitTemplateViewModel>(militaryUnitTemplateViewModels);

            FactionViewModels = new ObservableCollection<FactionViewModel>();
            for (var i = 0; i < 4; i++)
            {
                var faction = new Faction(i, $"Faction {i}", GameRenderer.PlayerColour(i));
                FactionViewModels.Add(new FactionViewModel(faction));
            }
        }

        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        TerrainType _selectedTerrainType;
        Board _board;
        Activity _activity;
        StructureType _selectedStructureType;
        public ObservableCollection<MilitaryUnitTemplateViewModel> MilitaryUnitTemplateViewModels { get; set; }
        public ObservableCollection<FactionViewModel> FactionViewModels { get; set; }


        //public StructureViewModel _selectedStructure;

        int _selectedFaction;
        GameRenderingWpf _gameRenderingWpf;
        enum Activity
        {
            Terrain,
            Edge,
            Structure,
            Unit
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            _board = new Board(GameBoard, TileEdges, Structures);

            _board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0) { Location = _board[1, 1] },
                new MilitaryUnit(1) { Location = _board[1, 1] },
                new MilitaryUnit(2) { Location = _board[1, 1] },
                new MilitaryUnit(3) { Location = _board[1, 1], OwnerIndex = 1 },
                new MilitaryUnit(4, "1st Fleet", 0, _board[0, 0], MovementType.Water),
                new MilitaryUnit(4, "1st Airborne", 0, _board[3, 2], MovementType.Airborne),
            };

            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                var color = GameRenderingWpf.ArgbColourToColor(GameRenderer.TerrainTypeColour(terrainType));

                var button = new Button
                {
                    Content = terrainType.ToString(),
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(PerceivedBrightness(color) > 130 ? Colors.Black : Colors.White),
                    Margin = new Thickness(5),
                    Width = 60,
                    FontWeight = FontWeights.Bold,
                    Tag = terrainType,
                };

                button.Click += TerrainTypeSelected_Click;

                TerrainTypeSelector.Children.Add(button);
            }

            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                var color = GameRenderingWpf.ArgbColourToColor(GameRenderer.EdgeTypeColour(edgeType));

                var button = new Button
                {
                    Content = edgeType.ToString(),
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(PerceivedBrightness(color) > 130 ? Colors.Black : Colors.White),
                    Margin = new Thickness(5),
                    Width = 60,
                    FontWeight = FontWeights.Bold,
                    Tag = edgeType,
                };

                EdgeTypeSelector.Children.Add(button);
            }

            foreach (StructureType structureType in Enum.GetValues(typeof(StructureType)))
            {
                //var color = GameRenderingWpf.ArgbColourToColor(GameRenderer.EdgeTypeColour(structureType));

                var button = new Button
                {
                    Content = structureType.ToString(),

                    Margin = new Thickness(5),
                    Width = 60,
                    FontWeight = FontWeights.Bold,
                    Tag = structureType,
                };

                button.Click += Button_Click;

                StructureTypeSelector.Children.Add(button);
            }

            //for (var i = 0; i < 4; i++)
            //{
            //    var color = GameRenderingWpf.ArgbColourToColor(GameRenderer.PlayerColour(i));
            //    var radioButton = new RadioButton()
            //    {
            //        Content = i,
            //        Tag = i,
            //        Foreground = new SolidColorBrush(color),
            //        GroupName = "Faction",
            //        Width = 50,
            //    };
            //    radioButton.Click += RadioButton_Click;
            //    if (i == 0)
            //    {
            //        radioButton.IsChecked = true;
            //        SelectFaction(i);
            //    }

            //    FactionSelector.Children.Add(radioButton);
            //}

            RenderMap();
        }

        private void RenderMap()
        {
            _gameRenderingWpf = new GameRenderingWpf(_board.Width, _board.Height);
            var gameRenderer = GameRenderer.Render(_gameRenderingWpf, RenderPipeline.Board, RenderPipeline.Units, _board.Width, _board.Height, _board.Tiles, _board.Edges, _board.Structures, units: _board.Units);
            var canvas = (Canvas)gameRenderer.GetBitmap();
            HexGrid hexGrid = null;

            foreach (UIElement uiElement in canvas.Children)
            {
                if (uiElement is HexGrid)
                {
                    hexGrid = uiElement as HexGrid;
                    break;
                }
            }

            foreach (UIElement uiElement in hexGrid.Children)
            {
                if (uiElement is HexItem hexItem)
                {
                    hexItem.MouseEnter += HexItem_MouseEnter;
                    hexItem.PreviewMouseLeftButtonDown += HexItem_PreviewMouseLeftButtonDown;
                }
            }

            MainGrid.Children.Add(canvas);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFaction((int)((RadioButton)sender).Tag);
        }

        private void SelectFaction(int faction)
        {
            _selectedFaction = faction;
            foreach (Button structureTypeUiElement in StructureTypeSelector.Children)
            {
                var color = GameRenderingWpf.ArgbColourToColor(GameRenderer.PlayerColour(_selectedFaction));

                if ((StructureType)structureTypeUiElement.Tag != StructureType.None)
                {
                    structureTypeUiElement.Background = new SolidColorBrush(color);
                    structureTypeUiElement.Foreground = new SolidColorBrush(PerceivedBrightness(color) > 130 ? Colors.Black : Colors.White);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = ((Button)e.Source);
            _activity = Activity.Structure;
            _selectedStructureType = (StructureType)button.Tag;
        }

        private void HexItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (_activity)
            {
                case Activity.Terrain:
                    SetTerrainType(sender);
                    break;
                case Activity.Structure:
                    SetStructureType(sender);
                    break;
            }
        }

        private void SetStructureType(object sender)
        {
            var x = (int)((HexItem)sender).GetValue(Grid.ColumnProperty);
            var y = (int)((HexItem)sender).GetValue(Grid.RowProperty);

            _gameRenderingWpf.RemoveRectangle(_board[x, y].Hex);

            if (_selectedStructureType == StructureType.None)
            {
                _board.Structures.Remove(_board[x, y].Structure);
                _board[x, y].Structure = null;
                Structure.DataContext = null;
            }
            else if (_board[x, y].Structure != null)
            {
                _board[x, y].Structure.OwnerIndex = _selectedFaction;
                _board[x, y].Structure.StructureType = _selectedStructureType;
            }
            else
            {
                _board.Structures.Remove(_board[x, y].Structure);
                var newStructure = new Structure(_board[x, y].Index, _selectedStructureType, _board[x, y], _selectedFaction);
                
                _board[x, y].Structure = newStructure;
                _board.Structures.Add(newStructure);
                
            }

            if (_board[x, y].Structure != null)
            {
                Structure.DataContext = new StructureViewModel(_board[x, y].Structure);
                _gameRenderingWpf.DrawRectangle(_board[x, y].Hex, GameRenderer.StructureColour(_board[x, y].Structure));
            }
            
        }

        private void HexItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetTerrainType(sender);
            }
        }

        private void SetTerrainType(object sender)
        {
            ((HexItem)sender).Background = TerrainType.Background;
            var x = (int)((HexItem)sender).GetValue(Grid.ColumnProperty);
            var y = (int)((HexItem)sender).GetValue(Grid.RowProperty);
            _board[x, y].TerrainType = _selectedTerrainType;
        }

        private void TerrainTypeSelected_Click(object sender, RoutedEventArgs e)
        {
            _activity = Activity.Terrain;

            var button = ((Button)e.Source);
            _selectedTerrainType = (TerrainType)button.Tag;
            TerrainType.Content = _selectedTerrainType;
            TerrainType.Background = button.Background;
        }



        private int PerceivedBrightness(System.Windows.Media.Color c)
        {
            return (int)Math.Sqrt(
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
        }

        BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        private void UnitSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mue = new MilitaryUnitTemplateEditor(MilitaryUnitTemplateViewModels[((ListBox)sender).SelectedIndex]);
            mue.Show();
        }

        private void FactionSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mue = new FactionEditor(FactionViewModels[((ListBox)sender).SelectedIndex]);
            mue.Show();
        }
    }
}
