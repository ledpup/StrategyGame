using GameModel;
using GameModel.Rendering;
using System;
using System.Collections.Generic;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private Color ColourToColor(ArgbColour colour)
        {
            return Color.FromArgb((byte)colour.Alpha, (byte)colour.Red, (byte)colour.Green, (byte)colour.Blue);
        }

        private int PerceivedBrightness(Color c)
        {
            return (int)Math.Sqrt(
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
        }
    }
}
