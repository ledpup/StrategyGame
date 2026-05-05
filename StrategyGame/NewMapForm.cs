using System.Windows.Forms;

namespace StrategyGame
{
    internal class NewMapForm : Form
    {
        readonly NumericUpDown _widthNumeric;
        readonly NumericUpDown _heightNumeric;

        public int MapWidth => (int)_widthNumeric.Value;
        public int MapHeight => (int)_heightNumeric.Value;

        public NewMapForm()
        {
            Text = "New Map";
            Width = 240;
            Height = 150;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var widthLabel = new Label { Left = 20, Top = 20, Width = 80, Text = "Width" };
            var heightLabel = new Label { Left = 20, Top = 50, Width = 80, Text = "Height" };
            _widthNumeric = new NumericUpDown { Left = 110, Top = 18, Width = 80, Minimum = 4, Maximum = 60, Value = 12 };
            _heightNumeric = new NumericUpDown { Left = 110, Top = 48, Width = 80, Minimum = 4, Maximum = 60, Value = 12 };

            var okButton = new Button { Text = "OK", Left = 35, Width = 70, Top = 80, DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "Cancel", Left = 115, Width = 70, Top = 80, DialogResult = DialogResult.Cancel };

            Controls.Add(widthLabel);
            Controls.Add(heightLabel);
            Controls.Add(_widthNumeric);
            Controls.Add(_heightNumeric);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}
