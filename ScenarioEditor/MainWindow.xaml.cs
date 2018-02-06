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
                TerrainTypeSelector.Children.Add(new Button
                {
                    Content = terrainType.ToString(),
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(PerceivedBrightness(color) > 130 ? Colors.Black : Colors.White),
                    Margin = new Thickness(5),
                    Width = 60,
                    FontWeight = FontWeights.Bold,
                }
                );
            }
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
