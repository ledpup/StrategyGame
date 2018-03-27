using ScenarioEditor.ViewModels;
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
    /// Interaction logic for FactionEditor.xaml
    /// </summary>
    public partial class FactionEditor : Window
    {
        public FactionEditor()
        {
            InitializeComponent();
        }

        FactionViewModel _factionViewModel;
        public FactionEditor(FactionViewModel factionViewModel) : this()
        {
            DataContext = _factionViewModel = factionViewModel;
        }
    }
}
