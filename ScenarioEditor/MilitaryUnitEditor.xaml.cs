using GameModel;
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
using System.Windows.Shapes;

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for MilitaryUnitEditor.xaml
    /// </summary>
    public partial class MilitaryUnitEditor : Window
    {
        public MilitaryUnitEditor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (MovementType movementType in Enum.GetValues(typeof(MovementType)))
            {
                var radioButton = new RadioButton()
                {
                    Content = movementType.ToString(),
                    Tag = (int)movementType,
                    GroupName = "MovementType",
                };
                //radioButton.Click += RadioButton_Click;

                MovementTypeSelector.Children.Add(radioButton);
            }

            foreach (CombatType combatType in Enum.GetValues(typeof(CombatType)))
            {
                var radioButton = new RadioButton()
                {
                    Content = combatType.ToString(),
                    Tag = (int)combatType,
                    GroupName = "CombatType",
                };
                //radioButton.Click += RadioButton_Click;

                CombatTypeSelector.Children.Add(radioButton);
            }


            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
            {
                var label = new Label
                {
                    Content = terrainType.ToString(),
                    Margin = new Thickness(5),
                    Width = 70,
                };

                var textBox = new TextBox
                {
                    Text = "1",
                    Margin = new Thickness(5),
                    Width = 20,
                };

                var checkBox = new CheckBox
                {
                    Name = terrainType.ToString(),
                    Content = "Can stop on",
                    Margin = new Thickness(5, 10, 5, 0),
                    IsChecked = true,
                };

                TerrainTypeMovementCosts.Children.Add(label);
                TerrainTypeMovementCosts.Children.Add(textBox);
                TerrainTypeMovementCosts.Children.Add(checkBox);
            }

            foreach (EdgeType edgeType in Enum.GetValues(typeof(EdgeType)))
            {
                var label = new Label
                {
                    Content = edgeType.ToString(),
                    Margin = new Thickness(5),
                    Width = 70,
                };

                var textBox = new TextBox
                {
                    Text = "1",
                    Margin = new Thickness(5),
                    Width = 20,
                };

                EdgeMovementCosts.Children.Add(label);
                EdgeMovementCosts.Children.Add(textBox);
            }
        }
    }
}
