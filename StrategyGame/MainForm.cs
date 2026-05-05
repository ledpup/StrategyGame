using GameModel;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Visualise;

namespace StrategyGame
{
    internal class MainForm : Form
    {
        readonly PictureBox _canvas;
        readonly ComboBox _toolComboBox;
        readonly ComboBox _terrainComboBox;
        readonly ComboBox _edgeComboBox;
        readonly CheckBox _roadCheckBox;
        readonly ComboBox _unitTypeComboBox;
        readonly ComboBox _movementTypeComboBox;
        readonly ComboBox _structureTypeComboBox;
        readonly NumericUpDown _ownerNumeric;
        readonly TextBox _statusTextBox;

        Board _board;
        string _currentFilePath;
        Tile _selectedTile;
        bool _isPaintingTerrain;
        int? _lastPaintedTileIndex;
        TerrainType _lastPaintedTerrainType;

        public MainForm()
        {
            Text = "StrategyGame Map Editor";
            Width = 1400;
            Height = 900;

            var toolPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                AutoSize = false,
                WrapContents = true,
            };

            var newButton = new Button { Text = "New", Width = 80 };
            var openButton = new Button { Text = "Open", Width = 80 };
            var saveButton = new Button { Text = "Save", Width = 80 };
            var saveAsButton = new Button { Text = "Save As", Width = 80 };
            var simulateButton = new Button { Text = "Simulate", Width = 100 };

            _toolComboBox = CreateComboBox(typeof(EditorTool));
            _terrainComboBox = CreateComboBox(typeof(TerrainType));
            _edgeComboBox = CreateComboBox(typeof(EdgeType));
            _unitTypeComboBox = CreateComboBox(typeof(UnitType));
            _movementTypeComboBox = CreateComboBox(typeof(MovementType));
            _structureTypeComboBox = CreateComboBox(typeof(StructureType));
            _roadCheckBox = new CheckBox { Text = "Road", AutoSize = true };
            _ownerNumeric = new NumericUpDown { Minimum = 0, Maximum = 7, Width = 60 };
            _statusTextBox = new TextBox { Width = 500, ReadOnly = true };

            toolPanel.Controls.Add(newButton);
            toolPanel.Controls.Add(openButton);
            toolPanel.Controls.Add(saveButton);
            toolPanel.Controls.Add(saveAsButton);
            toolPanel.Controls.Add(simulateButton);
            toolPanel.Controls.Add(new Label { Text = "Tool", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_toolComboBox);
            toolPanel.Controls.Add(new Label { Text = "Terrain", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_terrainComboBox);
            toolPanel.Controls.Add(new Label { Text = "Edge", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_edgeComboBox);
            toolPanel.Controls.Add(_roadCheckBox);
            toolPanel.Controls.Add(new Label { Text = "Unit", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_unitTypeComboBox);
            toolPanel.Controls.Add(new Label { Text = "Move", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_movementTypeComboBox);
            toolPanel.Controls.Add(new Label { Text = "Structure", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_structureTypeComboBox);
            toolPanel.Controls.Add(new Label { Text = "Owner", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            toolPanel.Controls.Add(_ownerNumeric);
            toolPanel.Controls.Add(_statusTextBox);

            _canvas = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = Color.White,
            };

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
            };
            scrollPanel.Controls.Add(_canvas);

            Controls.Add(scrollPanel);
            Controls.Add(toolPanel);

            newButton.Click += (_, __) => CreateNewMap();
            openButton.Click += (_, __) => OpenMap();
            saveButton.Click += (_, __) => SaveMap(false);
            saveAsButton.Click += (_, __) => SaveMap(true);
            simulateButton.Click += (_, __) => SimulateGame();
            _canvas.MouseClick += CanvasMouseClick;
            _canvas.MouseDown += CanvasMouseDown;
            _canvas.MouseMove += CanvasMouseMove;
            _canvas.MouseUp += CanvasMouseUp;

            LoadDefaultMap();
        }

        static ComboBox CreateComboBox(Type enumType)
        {
            var comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110,
                DataSource = Enum.GetValues(enumType),
            };
            return comboBox;
        }

        void LoadDefaultMap()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _board = new Board(
                File.ReadAllLines(Path.Combine(basePath, "BasicBoard.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardEdges.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardStructures.txt")));
            _board.Units = new System.Collections.Generic.List<MilitaryUnit>();
            _currentFilePath = null;
            _selectedTile = null;
            RenderBoard();
        }

        void CreateNewMap()
        {
            using (var dialog = new NewMapForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                _board = BoardEditorService.CreateDefaultBoard(dialog.MapWidth, dialog.MapHeight);
                _currentFilePath = null;
                _selectedTile = null;
                RenderBoard();
            }
        }

        void OpenMap()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Strategy Map (*.sgmap)|*.sgmap|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                _board = MapDocument.Load(dialog.FileName).ToBoard();
                _currentFilePath = dialog.FileName;
                _selectedTile = null;
                RenderBoard();
            }
        }

        void SaveMap(bool saveAs)
        {
            var path = _currentFilePath;
            if (saveAs || string.IsNullOrWhiteSpace(path))
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Strategy Map (*.sgmap)|*.sgmap|All files (*.*)|*.*";
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    path = dialog.FileName;
                }
            }

            MapDocument.FromBoard(_board).Save(path);
            _currentFilePath = path;
            SetStatus($"Saved {Path.GetFileName(path)}");
        }

        void SimulateGame()
        {
            var result = BoardEditorService.Simulate(_board);
            _board = result.Board;
            _selectedTile = null;
            RenderBoard();
            var owners = string.Join(", ", result.StructuresByOwner.OrderBy(x => x.Key).Select(x => $"P{x.Key}:{x.Value}"));
            SetStatus($"Simulated {result.TurnsCompleted} turns. Units alive: {result.RemainingUnits}. Structures: {owners}");
        }

        void CanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var selectedTool = _toolComboBox.SelectedItem is EditorTool tool ? tool : EditorTool.Terrain;
            if (selectedTool != EditorTool.Terrain)
                return;

            _isPaintingTerrain = true;
            _lastPaintedTileIndex = null;
            _lastPaintedTerrainType = (TerrainType)_terrainComboBox.SelectedItem;
            PaintTerrainAt(e.X, e.Y);
        }

        void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPaintingTerrain || e.Button != MouseButtons.Left)
                return;

            PaintTerrainAt(e.X, e.Y);
        }

        void CanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (_isPaintingTerrain && _lastPaintedTileIndex.HasValue)
            {
                // Full board rebuild happens once here, after the drag is finished.
                _board = BoardEditorService.RebuildBoard(_board);
            }

            _isPaintingTerrain = false;
            _lastPaintedTileIndex = null;
        }

        void PaintTerrainAt(int x, int y)
        {
            var tile = BoardEditorService.HitTest(_board, x, y);
            if (tile == null || _lastPaintedTileIndex == tile.Index)
                return;

            // Mutate in-place during drag for performance; board is rebuilt once on mouse-up.
            BoardEditorService.SetTerrainDirect(tile, _lastPaintedTerrainType);
            _selectedTile = null;
            _lastPaintedTileIndex = tile.Index;
            SetStatus($"Terrain changed at tile {tile.Index}");
            RenderBoard();
        }

        void CanvasMouseClick(object sender, MouseEventArgs e)
        {
            var tile = BoardEditorService.HitTest(_board, e.X, e.Y);
            if (tile == null)
                return;

            switch ((EditorTool)_toolComboBox.SelectedItem)
            {
                case EditorTool.Terrain:
                    return;
                case EditorTool.Structure:
                    _board = BoardEditorService.SetStructure(_board, tile, (StructureType)_structureTypeComboBox.SelectedItem, (int)_ownerNumeric.Value);
                    _selectedTile = null;
                    SetStatus($"Structure updated at tile {tile.Index}");
                    break;
                case EditorTool.Unit:
                    _board = BoardEditorService.AddUnit(_board, tile, (UnitType)_unitTypeComboBox.SelectedItem, (MovementType)_movementTypeComboBox.SelectedItem, (int)_ownerNumeric.Value);
                    _selectedTile = null;
                    SetStatus($"Unit added at tile {tile.Index}");
                    break;
                case EditorTool.Erase:
                    if (_selectedTile != null && BoardEditorService.AreAdjacent(_board[_selectedTile.Index], _board[tile.Index]))
                    {
                        _board = BoardEditorService.SetEdge(_board, _board[_selectedTile.Index], _board[tile.Index], EdgeType.None, false);
                        SetStatus($"Edge removed between {_selectedTile.Index} and {tile.Index}");
                        _selectedTile = null;
                    }
                    else
                    {
                        _board = BoardEditorService.EraseTileContent(_board, tile);
                        SetStatus($"Tile content removed at {tile.Index}");
                        _selectedTile = null;
                    }
                    break;
                case EditorTool.Edge:
                    if (_selectedTile == null)
                    {
                        _selectedTile = tile;
                        SetStatus($"Selected tile {_selectedTile.Index}. Click adjacent tile for edge.");
                        RenderBoard();
                        return;
                    }

                    if (!BoardEditorService.AreAdjacent(_board[_selectedTile.Index], _board[tile.Index]))
                    {
                        _selectedTile = tile;
                        SetStatus($"Selected tile {_selectedTile.Index}. Click adjacent tile for edge.");
                        RenderBoard();
                        return;
                    }

                    _board = BoardEditorService.SetEdge(_board, _board[_selectedTile.Index], _board[tile.Index], (EdgeType)_edgeComboBox.SelectedItem, _roadCheckBox.Checked);
                    SetStatus($"Edge updated between {_selectedTile.Index} and {tile.Index}");
                    _selectedTile = null;
                    break;
            }

            RenderBoard();
        }

        void RenderBoard()
        {
            var selectedTool = _toolComboBox.SelectedItem is EditorTool tool ? tool : EditorTool.Terrain;
            var showSelectedTile = selectedTool == EditorTool.Edge && _selectedTile != null;
            foreach (var tile in _board.Tiles)
            {
                tile.IsSelected = showSelectedTile && tile.Index == _selectedTile.Index;
            }

            var drawing = GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, _board.Width, _board.Height, _board.Tiles, _board.Edges, _board.Structures, null, null, _board.Units);
            var previous = _canvas.Image;
            _canvas.Image = new Bitmap(drawing.ToBitmap());
            previous?.Dispose();
        }

        void SetStatus(string message)
        {
            _statusTextBox.Text = message;
        }
    }
}
