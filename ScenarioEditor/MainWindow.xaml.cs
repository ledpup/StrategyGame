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
                new MilitaryUnit() { Location = _board[1, 1] },
                new MilitaryUnit() { Location = _board[1, 1] },
                new MilitaryUnit() { Location = _board[1, 1] },
                new MilitaryUnit() { Location = _board[1, 1], OwnerIndex = 2 }
            };

            RenderGdiPlusBoard();

            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                var color = ColourToColor(GameRenderer.GetColour(terrainType));

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

            HexGrid.RowCount = _board.Height;
            HexGrid.ColumnCount = _board.Width;

            for (var x = 0; x < HexGrid.ColumnCount; x++)
            {
                for (var y = 0; y < HexGrid.RowCount; y++)
                {
                    var hexItem = new HexItem();
                    hexItem.SetValue(Grid.ColumnProperty, x);
                    hexItem.SetValue(Grid.RowProperty, y);
                    hexItem.BorderThickness = new Thickness(.5);
                    hexItem.Background = new SolidColorBrush(ColourToColor(GameRenderer.GetColour(_board[x, y].TerrainType)));
                    hexItem.MouseEnter += HexItem_MouseEnter;
                    hexItem.PreviewMouseLeftButtonDown += HexItem_PreviewMouseLeftButtonDown;
                    HexGrid.Children.Add(hexItem);
                }
            }
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

        private System.Windows.Media.Color ColourToColor(ArgbColour colour)
        {
            return System.Windows.Media.Color.FromArgb((byte)colour.Alpha, (byte)colour.Red, (byte)colour.Green, (byte)colour.Blue);
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
