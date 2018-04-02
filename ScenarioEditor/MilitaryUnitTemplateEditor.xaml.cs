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

            _militaryUnitTemplateViewModel.TerrainMovementViewModels.ForEach(x => x.PropertyChanged += X_PropertyChanged);
        }

        private void X_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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

        private void TerrainMovement_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var terrainMovementViewModel = (TerrainMovementViewModel)e.Row.Item;
            if (!terrainMovementViewModel.Traversable)
            {
                if (e.Column.DisplayIndex == 2 || e.Column.DisplayIndex == 3)
                    e.Cancel = true;
            }
        }

        private void EdgeMovement_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var edgeMovementViewModel = (EdgeMovementViewModel)e.Row.Item;
            if (!edgeMovementViewModel.Traversable)
            {
                if (e.Column.DisplayIndex == 2)
                    e.Cancel = true;
            }
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
