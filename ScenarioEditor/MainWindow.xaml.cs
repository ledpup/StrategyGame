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
        }

        static string[] GameBoard = File.ReadAllLines("BasicBoard.txt");
        static string[] TileEdges = File.ReadAllLines("BasicBoardEdges.txt");
        static string[] Structures = File.ReadAllLines("BasicBoardStructures.txt");

        TerrainType _selectedTerrainType;
        Board _board;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _board = new Board(GameBoard, TileEdges, Structures);

            _board.Units = new List<MilitaryUnit>
            {
                new MilitaryUnit(0) { Location = _board[1, 1] },
                new MilitaryUnit(1) { Location = _board[1, 1] },
                new MilitaryUnit(2) { Location = _board[1, 1] },
                new MilitaryUnit(3) { Location = _board[1, 1], OwnerIndex = 2 },
                new MilitaryUnit(4, "1st Fleet", 0, _board[0, 0], MovementType.Water),
                new MilitaryUnit(4, "1st Airborne", 0, _board[3, 2], MovementType.Airborne),
            };

            RenderGdiPlusBoard();

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

                button.Click += Button_Click;

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

                button.Click += Button_Click;

                EdgeTypeSelector.Children.Add(button);
            }

            var wpf = new GameRenderingWpf(_board.Width, _board.Height);
            var gameRenderer = GameRenderer.Render(wpf, RenderPipeline.Board, RenderPipeline.Units, _board.Width, _board.Height, _board.Tiles, _board.Edges, _board.Structures, units: _board.Units);
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

        private void HexItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetTerrainType(sender);
        }

        private void RenderGdiPlusBoard()
        {
            var drawing2d = new GameRenderingGdiPlus(_board.Width, _board.Height);
            GameRenderer.Render(drawing2d, RenderPipeline.Board, RenderPipeline.Labels, _board.Width, _board.Height, _board.Tiles, _board.Edges, _board.Structures, units: _board.Units);

            var bitmap = (Bitmap)drawing2d.GetBitmap();
            Map.Source = BitmapToBitmapImage(bitmap);
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
            RenderGdiPlusBoard();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
