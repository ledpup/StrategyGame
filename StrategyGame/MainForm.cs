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
        readonly PictureBox canvas;
        readonly ComboBox toolComboBox;
        readonly TerrainPalette terrainPalette;
        readonly ComboBox edgeComboBox;
        readonly CheckBox roadCheckBox;
        readonly ComboBox unitTypeComboBox;
        readonly ComboBox movementTypeComboBox;
        readonly ComboBox structureTypeComboBox;
        readonly NumericUpDown ownerNumeric;
        readonly TextBox statusTextBox;

        // Per-tool panels – shown/hidden when the active tool changes
        readonly Panel _terrainPanel;
        readonly Panel _edgePanel;
        readonly Panel _unitPanel;
        readonly Panel _structurePanel;
        readonly Panel _ownerPanel;

        Board _board;
        string _currentFilePath;
        Tile _selectedTile;
        bool _isPaintingTerrain;
        int? _lastPaintedTileIndex;
        TerrainType _lastPaintedTerrainType;
        readonly MapHistory _history = new();
        Button _undoButton;
        Button _redoButton;

        // Simulation mode
        SimulationSession _session;
        readonly Panel _simPanel;
        readonly Button _simPrevButton;
        readonly Button _simNextButton;
        readonly Button _simRestartButton;
        readonly Button _simExitButton;

        public MainForm()
        {
            Text = "StrategyGame Map Editor";
            Width = 1400;
            Height = 900;

            // ── toolbar ──────────────────────────────────────────────────
            var toolPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 70,
                AutoSize = false,
                WrapContents = false,
                Padding = new Padding(4, 4, 0, 0),
            };

            var newButton      = new Button { Text = "New",      Width = 70, Height = 28 };
            var openButton     = new Button { Text = "Open",     Width = 70, Height = 28 };
            var saveButton     = new Button { Text = "Save",     Width = 70, Height = 28 };
            var saveAsButton   = new Button { Text = "Save As",  Width = 70, Height = 28 };
            var simulateButton = new Button { Text = "Simulate", Width = 80, Height = 28 };
            _undoButton        = new Button { Text = "Undo",     Width = 70, Height = 28, Enabled = false };
            _redoButton        = new Button { Text = "Redo",     Width = 70, Height = 28, Enabled = false };

            // ── simulation controls (hidden until simulation mode) ────────
            _simPrevButton     = new Button { Text = "◀ Prev",    Width = 80, Height = 28 };
            _simNextButton     = new Button { Text = "Next ▶",    Width = 80, Height = 28 };
            _simRestartButton  = new Button { Text = "↺ Restart", Width = 85, Height = 28 };
            _simExitButton     = new Button { Text = "✕ Exit",    Width = 80, Height = 28 };
            _simPanel = MakePanel(
                Label("Simulation:", leftPad: 6),
                _simPrevButton,
                _simNextButton,
                _simRestartButton,
                _simExitButton);
            _simPanel.Visible = false;

            toolComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                Height = 28,
                DataSource = Enum.GetValues(typeof(EditorTool)),
            };

            // ── terrain palette panel ────────────────────────────────────
            terrainPalette = new TerrainPalette { Margin = new Padding(6, 4, 0, 0) };
            _terrainPanel = new Panel { AutoSize = true, Margin = new Padding(0) };
            _terrainPanel.Controls.Add(terrainPalette);

            // ── edge panel ───────────────────────────────────────────────
            edgeComboBox = CreateComboBox(typeof(EdgeType));
            roadCheckBox = new CheckBox { Text = "Road", AutoSize = true, Margin = new Padding(4, 8, 0, 0) };
            _edgePanel = MakePanel(
                Label("Edge"),
                edgeComboBox,
                roadCheckBox);

            // ── unit panel ───────────────────────────────────────────────
            unitTypeComboBox     = CreateComboBox(typeof(UnitType));
            movementTypeComboBox = CreateComboBox(typeof(MovementType));
            _unitPanel = MakePanel(
                Label("Unit"),
                unitTypeComboBox,
                Label("Move"),
                movementTypeComboBox);

            // ── structure panel ──────────────────────────────────────────
            structureTypeComboBox = CreateComboBox(typeof(StructureType));
            _structurePanel = MakePanel(
                Label("Structure"),
                structureTypeComboBox);

            // ── owner panel (unit + structure) ───────────────────────────
            ownerNumeric = new NumericUpDown { Minimum = 0, Maximum = 7, Width = 55 };
            _ownerPanel = MakePanel(Label("Owner"), ownerNumeric);

            // ── status ───────────────────────────────────────────────────
            statusTextBox = new TextBox { Width = 380, ReadOnly = true, Margin = new Padding(8, 8, 0, 0) };

            toolPanel.Controls.Add(newButton);
            toolPanel.Controls.Add(openButton);
            toolPanel.Controls.Add(saveButton);
            toolPanel.Controls.Add(saveAsButton);
            toolPanel.Controls.Add(simulateButton);
            toolPanel.Controls.Add(_undoButton);
            toolPanel.Controls.Add(_redoButton);
            toolPanel.Controls.Add(_simPanel);
            toolPanel.Controls.Add(Label("Tool", leftPad: 10));
            toolPanel.Controls.Add(toolComboBox);
            toolPanel.Controls.Add(_terrainPanel);
            toolPanel.Controls.Add(_edgePanel);
            toolPanel.Controls.Add(_unitPanel);
            toolPanel.Controls.Add(_structurePanel);
            toolPanel.Controls.Add(_ownerPanel);
            toolPanel.Controls.Add(statusTextBox);

            // ── canvas ───────────────────────────────────────────────────
            canvas = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = Color.White,
            };

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scrollPanel.Controls.Add(canvas);

            Controls.Add(scrollPanel);
            Controls.Add(toolPanel);

            // ── events ───────────────────────────────────────────────────
            newButton.Click      += (_, __) => CreateNewMap();
            openButton.Click     += (_, __) => OpenMap();
            saveButton.Click     += (_, __) => SaveMap(false);
            saveAsButton.Click   += (_, __) => SaveMap(true);
            simulateButton.Click += (_, __) => EnterSimulation();
            _undoButton.Click    += (_, __) => PerformUndo();
            _redoButton.Click    += (_, __) => PerformRedo();
            _simPrevButton.Click    += (_, __) => SimStepBack();
            _simNextButton.Click    += (_, __) => SimStepForward();
            _simRestartButton.Click += (_, __) => SimRestart();
            _simExitButton.Click    += (_, __) => ExitSimulation();
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            canvas.MouseClick   += CanvasMouseClick;
            canvas.MouseDown    += CanvasMouseDown;
            canvas.MouseMove    += CanvasMouseMove;
            canvas.MouseUp      += CanvasMouseUp;
            toolComboBox.SelectedIndexChanged += (_, __) => UpdateToolPanels();
            terrainPalette.SelectionChanged += (_, t) => _lastPaintedTerrainType = t;

            UpdateToolPanels();
            LoadDefaultMap();
        }

        // ── helpers ──────────────────────────────────────────────────────

        static ComboBox CreateComboBox(Type enumType) =>
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110,
                DataSource = Enum.GetValues(enumType),
            };

        static Label Label(string text, int leftPad = 4) =>
            new Label { Text = text, AutoSize = true, Padding = new Padding(leftPad, 8, 0, 0) };

        static Panel MakePanel(params Control[] controls)
        {
            var p = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0), WrapContents = false };
            foreach (var c in controls)
                p.Controls.Add(c);
            return p;
        }

        void UpdateToolPanels()
        {
            var tool = toolComboBox.SelectedItem is EditorTool t ? t : EditorTool.Terrain;

            _terrainPanel.Visible   = tool == EditorTool.Terrain;
            _edgePanel.Visible      = tool == EditorTool.Edge;
            _unitPanel.Visible      = tool == EditorTool.Unit;
            _structurePanel.Visible = tool == EditorTool.Structure;
            _ownerPanel.Visible     = tool == EditorTool.Unit || tool == EditorTool.Structure;
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_session != null)
            {
                if (e.KeyCode == Keys.Right)       { SimStepForward(); e.Handled = true; }
                else if (e.KeyCode == Keys.Left)   { SimStepBack();    e.Handled = true; }
                else if (e.KeyCode == Keys.Home)   { SimRestart();     e.Handled = true; }
                else if (e.KeyCode == Keys.Escape) { ExitSimulation(); e.Handled = true; }
                return;
            }
            if (e.Control && e.KeyCode == Keys.Z) { PerformUndo(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { PerformRedo(); e.Handled = true; }
        }

        void PerformUndo()
        {
            if (!_history.CanUndo) return;
            var doc = _history.Undo(MapDocument.FromBoard(_board));
            _board = doc.ToBoard();
            _selectedTile = null;
            UpdateUndoRedoButtons();
            RenderBoard();
            SetStatus("Undo");
        }

        void PerformRedo()
        {
            if (!_history.CanRedo) return;
            var doc = _history.Redo(MapDocument.FromBoard(_board));
            _board = doc.ToBoard();
            _selectedTile = null;
            UpdateUndoRedoButtons();
            RenderBoard();
            SetStatus("Redo");
        }

        // Captured before-state for the current edit gesture.
        MapDocument _editBefore;

        /// <summary>Snapshot the current state before a mutation begins.</summary>
        void BeginEdit() => _editBefore = MapDocument.FromBoard(_board);

        /// <summary>Diff before→after and push to undo stack; call after the mutation is applied.</summary>
        void CommitEdit(string description)
        {
            if (_editBefore == null) return;
            _history.Commit(_editBefore, MapDocument.FromBoard(_board), description);
            _editBefore = null;
            UpdateUndoRedoButtons();
        }

        void UpdateUndoRedoButtons()
        {
            _undoButton.Enabled = _history.CanUndo;
            _redoButton.Enabled = _history.CanRedo;
        }

        // ── map operations ───────────────────────────────────────────────

        void LoadDefaultMap()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _board = new Board(
                File.ReadAllLines(Path.Combine(basePath, "BasicBoard.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardEdges.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardStructures.txt")));
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
                _history.Clear();
                UpdateUndoRedoButtons();
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
                _history.Clear();
                _history.TryLoad(dialog.FileName);
                UpdateUndoRedoButtons();
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
            _history.Save(path);
            _currentFilePath = path;
            SetStatus($"Saved {Path.GetFileName(path)}");
        }

        void EnterSimulation()
        {
            _session = BoardEditorService.StartSimulation(_board);
            _simPanel.Visible = true;
            _undoButton.Visible = false;
            _redoButton.Visible = false;
            canvas.MouseClick -= CanvasMouseClick;
            canvas.MouseDown  -= CanvasMouseDown;
            canvas.MouseMove  -= CanvasMouseMove;
            canvas.MouseUp    -= CanvasMouseUp;
            UpdateSimButtons();
            _board = _session.CurrentBoard;
            RenderBoard();
            SetStatus(_session.StatusLine());
        }

        void ExitSimulation()
        {
            _session = null;
            _simPanel.Visible = false;
            _undoButton.Visible = true;
            _redoButton.Visible = true;
            canvas.MouseClick += CanvasMouseClick;
            canvas.MouseDown  += CanvasMouseDown;
            canvas.MouseMove  += CanvasMouseMove;
            canvas.MouseUp    += CanvasMouseUp;
            RenderBoard();
            SetStatus("Simulation exited.");
        }

        void SimStepForward()
        {
            if (_session == null) return;
            _session.StepForward();
            _board = _session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(_session.StatusLine() + (_session.IsFinished && !_session.CanStepForward ? "  [Simulation ended]" : ""));
        }

        void SimStepBack()
        {
            if (_session == null) return;
            _session.StepBack();
            _board = _session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(_session.StatusLine());
        }

        void SimRestart()
        {
            if (_session == null) return;
            _session.Restart();
            _board = _session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(_session.StatusLine());
        }

        void UpdateSimButtons()
        {
            _simPrevButton.Enabled    = _session?.CanStepBack ?? false;
            _simNextButton.Enabled    = _session?.CanStepForward ?? false;
            _simRestartButton.Enabled = _session?.CanStepBack ?? false;
        }

        // ── mouse handling ───────────────────────────────────────────────

        void CanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var selectedTool = toolComboBox.SelectedItem is EditorTool tool ? tool : EditorTool.Terrain;
            if (selectedTool != EditorTool.Terrain)
                return;

            BeginEdit();
            _isPaintingTerrain = true;
            _lastPaintedTileIndex = null;
            _lastPaintedTerrainType = terrainPalette.SelectedTerrain;
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
                _board = BoardEditorService.RebuildBoard(_board);
                CommitEdit("Paint terrain");
            }

            _isPaintingTerrain = false;
            _lastPaintedTileIndex = null;
        }

        void PaintTerrainAt(int x, int y)
        {
            var tile = BoardEditorService.HitTest(_board, x, y);
            if (tile == null || _lastPaintedTileIndex == tile.Index)
                return;

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

            switch ((EditorTool)toolComboBox.SelectedItem)
            {
                case EditorTool.Terrain:
                    return;
                case EditorTool.Structure:
                    BeginEdit();
                    _board = BoardEditorService.SetStructure(_board, tile, (StructureType)structureTypeComboBox.SelectedItem, (int)ownerNumeric.Value);
                    CommitEdit($"Set structure at {tile.Index}");
                    _selectedTile = null;
                    SetStatus($"Structure updated at tile {tile.Index}");
                    break;
                case EditorTool.Unit:
                    BeginEdit();
                    _board = BoardEditorService.AddUnit(_board, tile, (UnitType)unitTypeComboBox.SelectedItem, (MovementType)movementTypeComboBox.SelectedItem, (int)ownerNumeric.Value);
                    CommitEdit($"Add unit at {tile.Index}");
                    _selectedTile = null;
                    SetStatus($"Unit added at tile {tile.Index}");
                    break;
                case EditorTool.Erase:
                    BeginEdit();
                    if (_selectedTile != null && BoardEditorService.AreAdjacent(_board[_selectedTile.Index], _board[tile.Index]))
                    {
                        _board = BoardEditorService.SetEdge(_board, _board[_selectedTile.Index], _board[tile.Index], EdgeType.None, false);
                        CommitEdit($"Remove edge {_selectedTile.Index}-{tile.Index}");
                        SetStatus($"Edge removed between {_selectedTile.Index} and {tile.Index}");
                        _selectedTile = null;
                    }
                    else
                    {
                        _board = BoardEditorService.EraseTileContent(_board, tile);
                        CommitEdit($"Erase tile {tile.Index}");
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

                    BeginEdit();
                    _board = BoardEditorService.SetEdge(_board, _board[_selectedTile.Index], _board[tile.Index], (EdgeType)edgeComboBox.SelectedItem, roadCheckBox.Checked);
                    CommitEdit($"Set edge {_selectedTile.Index}-{tile.Index}");
                    SetStatus($"Edge updated between {_selectedTile.Index} and {tile.Index}");
                    _selectedTile = null;
                    break;
            }

            RenderBoard();
        }

        // ── rendering ────────────────────────────────────────────────────

        void RenderBoard()
        {
            var selectedTool = toolComboBox.SelectedItem is EditorTool tool ? tool : EditorTool.Terrain;
            var showSelectedTile = selectedTool == EditorTool.Edge && _selectedTile != null;
            foreach (var tile in _board.Tiles)
                tile.IsSelected = showSelectedTile && tile.Index == _selectedTile.Index;

            var drawing = GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, _board.Width, _board.Height, _board.Tiles, _board.Edges, _board.Structures, null, null, _board.Units);
            var previous = canvas.Image;
            canvas.Image = new Bitmap(drawing.ToBitmap());
            previous?.Dispose();
        }

        void SetStatus(string message) => statusTextBox.Text = message;
    }
}
