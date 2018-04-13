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
using Manager;

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

            _scenario = new Scenario("Scenario_01");

            var militaryUnitTemplateViewModels = _scenario.MilitaryUnitTemplates.Select(x => new MilitaryUnitTemplateViewModel(x)).ToList();
            MilitaryUnitTemplateViewModels = new ObservableCollection<MilitaryUnitTemplateViewModel>(militaryUnitTemplateViewModels);

            var militaryUnitInstanceViewModels = _scenario.MilitaryUnitInstances.Select(x => new MilitaryUnitInstanceViewModel(x)).ToList();
            MilitaryUnitInstanceViewModels = new ObservableCollection<MilitaryUnitInstanceViewModel>(militaryUnitInstanceViewModels);

            FactionViewModels = new ObservableCollection<FactionViewModel>(_scenario.Factions.Select(x => new FactionViewModel(x)));

            UnitsAtSelectedLocation = new ObservableCollection<MilitaryUnitInstanceViewModel>();
        }

        Scenario _scenario;

        TerrainType _selectedTerrainType;
        
        Activity _activity;
        StructureType _selectedStructureType;
        MilitaryUnitTemplate _selectedUnitTemplate;
        MilitaryUnitInstance _selectedUnitInstance;
        public ObservableCollection<MilitaryUnitTemplateViewModel> MilitaryUnitTemplateViewModels { get; set; }
        public ObservableCollection<MilitaryUnitInstanceViewModel> MilitaryUnitInstanceViewModels { get; set; }
        public ObservableCollection<MilitaryUnitInstanceViewModel> UnitsAtSelectedLocation { get; set; }
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

            RenderMap();

            //var drawing2d = new GameRenderingGdiPlus(_scenario.Board.Width, _scenario.Board.Height);
            //GameRenderer.Render(drawing2d, RenderPipeline.Board, RenderPipeline.Labels, _scenario.Board.Width, _scenario.Board.Height, _scenario.Board.Tiles, _scenario.Board.Edges, _scenario.Board.Structures, units: _scenario.Board.Units);

            //var bitmap = (Bitmap)drawing2d.GetBitmap();
            //Map.Source = BitmapToBitmapImage(bitmap);
        }

        private void RenderMap()
        {
            _gameRenderingWpf = new GameRenderingWpf(_scenario.Board.Width, _scenario.Board.Height);
            var gameRenderer = GameRenderer.Render(_gameRenderingWpf, RenderPipeline.Board, RenderPipeline.Units, _scenario.Board.Width, _scenario.Board.Height, _scenario.Board.Tiles, _scenario.Board.Edges, _scenario.Board.Structures, units: _scenario.Board.Units);
            var canvas = (Canvas)gameRenderer.GetBitmap();
            canvas.Margin = new Thickness(10, 0, 0, 0);
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
            var x = (int)((HexItem)sender).GetValue(Grid.ColumnProperty);
            var y = (int)((HexItem)sender).GetValue(Grid.RowProperty);

            switch (_activity)
            {
                case Activity.Terrain:
                    SetTerrain(x, y, (HexItem)sender);
                    break;
                case Activity.Structure:
                    PlaceStructure(x, y);
                    break;
                case Activity.Unit:
                    PlaceUnit(x, y);
                    break;
            }

            UnitsAtSelectedLocation.Clear();
            _scenario.Board[x, y].Units.Select(u => new MilitaryUnitInstanceViewModel(new MilitaryUnitInstance(u.Id, u.Name, _scenario.Factions.Single(f => f.Id == u.FactionId), _scenario.MilitaryUnitTemplates.Single(t => t.Id == u.TemplateId))))
                .ToList()
                .ForEach(u => UnitsAtSelectedLocation.Add(u));
        }

        private void PlaceUnit(int x, int y)
        {
            var terrainMovement = _selectedUnitTemplate.TerrainMovements.Single(t => t.TerrainType == _scenario.Board[x, y].TerrainType.ToString());

            if (terrainMovement.Traversable && terrainMovement.CanStopOn)
            {
                var militaryUnit = _scenario.PlaceUnit(_selectedUnitTemplate, _selectedFaction, x, y);

                switch (militaryUnit.MovementType)
                {
                    case MovementType.Airborne:
                        _gameRenderingWpf.DrawTriangle(_scenario.Board[x, y].Hex, _scenario.Board[x, y].Units.Count, _scenario.Factions[_selectedFaction].Colour);
                        break;
                    case MovementType.Land:
                        _gameRenderingWpf.DrawCircle(_scenario.Board[x, y].Hex, _scenario.Board[x, y].Units.Count, _scenario.Factions[_selectedFaction].Colour);
                        break;
                    case MovementType.Water:
                        _gameRenderingWpf.DrawTrapezium(_scenario.Board[x, y].Hex, _scenario.Board[x, y].Units.Count, _scenario.Factions[_selectedFaction].Colour);
                        break;
                }

                _gameRenderingWpf.RepositionUnits(_scenario.Board[x, y].Hex);
            }
        }

        private void PlaceStructure(int x, int y)
        {
            _gameRenderingWpf.RemoveRectangle(_scenario.Board[x, y].Hex);

            _scenario.PlaceStructure(_selectedStructureType, _selectedFaction, x, y);

            if (_selectedStructureType == StructureType.None)
            {
                Structure.DataContext = null;
            }
            else if (_scenario.Board[x, y].Structure != null)
            {
                Structure.DataContext = new StructureViewModel(_scenario.Board[x, y].Structure);
                _gameRenderingWpf.DrawRectangle(_scenario.Board[x, y].Hex, GameRenderer.StructureColour(_scenario.Board[x, y].Structure));
            }
        }

        private void HexItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var x = (int)((HexItem)sender).GetValue(Grid.ColumnProperty);
            var y = (int)((HexItem)sender).GetValue(Grid.RowProperty);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetTerrain(x, y, (HexItem)sender);
            }
        }

        private void SetTerrain(int x, int y, HexItem hex)
        {
            hex.Background = TerrainType.Background;

            _scenario.Board[x, y].TerrainType = _selectedTerrainType;
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

        private void UnitTemplateSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mue = new MilitaryUnitEditor(MilitaryUnitTemplateViewModels[((ListBox)sender).SelectedIndex]);
            mue.Show();
        }

        private void FactionSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mue = new FactionEditor(FactionViewModels[((ListBox)sender).SelectedIndex]);
            mue.Show();
        }

        private void FactionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectFaction(((FactionViewModel)e.AddedItems[0]).Id);
        }

        private void UnitTemplateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = ((MilitaryUnitTemplateViewModel)e.AddedItems[0]).Id;
            _activity = Activity.Unit;
            _selectedUnitTemplate = _scenario.MilitaryUnitTemplates.Single(x => x.Id == id);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _scenario.Save();
        }

        private void UnitSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mue = new MilitaryUnitEditor(UnitsAtSelectedLocation[((ListBox)sender).SelectedIndex]);
            mue.Show();
        }

        private void UnitSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = ((MilitaryUnitInstanceViewModel)e.AddedItems[0]).Id;
            _activity = Activity.Unit;
            _selectedUnitInstance = _scenario.MilitaryUnitInstances.Single(x => x.Id == id);
        }

        private void RemoveUnitInstance_Click(object sender, RoutedEventArgs e)
        {
            _scenario.MilitaryUnitInstances.Remove(_selectedUnitInstance);
            MilitaryUnitInstanceViewModels.Remove(MilitaryUnitInstanceViewModels.Single(x => x.Id == _selectedUnitInstance.Id));
        }
    }
}
