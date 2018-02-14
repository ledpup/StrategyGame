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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var board = new Board(GameBoard, TileEdges, Structures);

            var units = new List<MilitaryUnit>
            {
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1] },
                new MilitaryUnit() { Location = board[1, 1], OwnerIndex = 2 }
            };

            var drawing2d = new GameRenderingEngine2D(board.Width, board.Height);
            GameRenderer.Render(drawing2d, RenderPipeline.Board, RenderPipeline.Labels, board.Width, board.Height, board.Tiles, board.Edges, board.Structures, units: units);

            var bitmap = (Bitmap)drawing2d.GetBitmap();
            Map.Source = BitmapToBitmapImage(bitmap);

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

            HexGrid.RowCount = board.Height;
            HexGrid.ColumnCount = board.Width;

            for (var x = 0; x < HexGrid.RowCount; x++)
            { 
                for (var y = 0; y < HexGrid.ColumnCount; y++)
                {
                    var hexItem = new HexItem();
                    hexItem.SetValue(Grid.ColumnProperty, x);
                    hexItem.SetValue(Grid.RowProperty, y);
                    hexItem.BorderThickness = new Thickness(.5);
                    hexItem.Background = System.Windows.Media.Brushes.White;
                    hexItem.MouseEnter += Hi_MouseEnter;
                    hexItem.MouseLeftButtonUp += Hi_MouseLeftButtonDown;
                    HexGrid.Children.Add(hexItem);
                }
            }
        }

        private void Hi_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ((HexItem)sender).Background = TerrainType.Background;
            }
        }

        private void Hi_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((HexItem)sender).Background = TerrainType.Background;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = ((Button)e.Source);
            _selectedTerrainType = (TerrainType)button.Tag;
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
