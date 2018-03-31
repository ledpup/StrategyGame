using GameModel;
using ScenarioEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MilitaryUnitTemplateEditor.xaml
    /// </summary>
    public partial class MilitaryUnitTemplateEditor : Window
    {
        public MilitaryUnitTemplateEditor()
        {
            InitializeComponent();
        }

        MilitaryUnitTemplateViewModel _militaryUnitTemplateViewModel;
        
        public MilitaryUnitTemplateEditor(MilitaryUnitTemplateViewModel militaryUnitTemplateViewModel) : this ()
        {
            DataContext = _militaryUnitTemplateViewModel = militaryUnitTemplateViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                    Text = "0",
                    Margin = new Thickness(5),
                    Width = 20,
                };

                EdgeMovementCosts.Children.Add(label);
                EdgeMovementCosts.Children.Add(textBox);
            }
        }

        private void MovementType_Click(object sender, RoutedEventArgs e)
        {
            _militaryUnitTemplateViewModel.MovementType = (MovementType)((RadioButton)sender).Tag;
        }

        private void MovementTypesTransportableBy_Click(object sender, RoutedEventArgs e)
        {
            var movementTypeTransportableBy = (CheckBox)sender;
            if ((bool)movementTypeTransportableBy.IsChecked)
            {
                _militaryUnitTemplateViewModel.MovementTypesTransportableBy.Add((MovementType)Enum.Parse(typeof(MovementType), movementTypeTransportableBy.Content.ToString()));
            }
            else
            {
                _militaryUnitTemplateViewModel.MovementTypesTransportableBy.Remove((MovementType)Enum.Parse(typeof(MovementType), movementTypeTransportableBy.Content.ToString()));
            }
        }
    }
}
