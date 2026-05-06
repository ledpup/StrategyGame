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
        readonly Panel terrainPanel;
        readonly Panel edgePanel;
        readonly Panel unitPanel;
        readonly Panel structurePanel;
        readonly Panel ownerPanel;

        Board board;
        string currentFilePath;
        Tile selectedTile;
        bool isPaintingTerrain;
        int? lastPaintedTileIndex;
        TerrainType lastPaintedTerrainType;
        readonly MapHistory history = new();
        Button undoButton;
        Button redoButton;

        // Simulation mode
        SimulationSession session;
        readonly Panel simPanel;
        readonly Button simPrevButton;
        readonly Button simNextButton;
        readonly Button simRestartButton;
        readonly Button simExitButton;

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
            undoButton        = new Button { Text = "Undo",     Width = 70, Height = 28, Enabled = false };
            redoButton        = new Button { Text = "Redo",     Width = 70, Height = 28, Enabled = false };

            // ── simulation controls (hidden until simulation mode) ────────
            simPrevButton     = new Button { Text = "◀ Prev",    Width = 80, Height = 28 };
            simNextButton     = new Button { Text = "Next ▶",    Width = 80, Height = 28 };
            simRestartButton  = new Button { Text = "↺ Restart", Width = 85, Height = 28 };
            simExitButton     = new Button { Text = "✕ Exit",    Width = 80, Height = 28 };
            simPanel = MakePanel(
                Label("Simulation:", leftPad: 6),
                simPrevButton,
                simNextButton,
                simRestartButton,
                simExitButton);
            simPanel.Visible = false;

            toolComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                Height = 28,
                DataSource = Enum.GetValues(typeof(EditorTool)),
            };

            // ── terrain palette panel ────────────────────────────────────
            terrainPalette = new TerrainPalette { Margin = new Padding(6, 4, 0, 0) };
            terrainPanel = new Panel { AutoSize = true, Margin = new Padding(0) };
            terrainPanel.Controls.Add(terrainPalette);

            // ── edge panel ───────────────────────────────────────────────
            edgeComboBox = CreateComboBox(typeof(EdgeType));
            roadCheckBox = new CheckBox { Text = "Road", AutoSize = true, Margin = new Padding(4, 8, 0, 0) };
            edgePanel = MakePanel(
                Label("Edge"),
                edgeComboBox,
                roadCheckBox);

            // ── unit panel ───────────────────────────────────────────────
            unitTypeComboBox     = CreateComboBox(typeof(UnitType));
            movementTypeComboBox = CreateComboBox(typeof(MovementType));
            unitPanel = MakePanel(
                Label("Unit"),
                unitTypeComboBox,
                Label("Move"),
                movementTypeComboBox);

            // ── structure panel ──────────────────────────────────────────
            structureTypeComboBox = CreateComboBox(typeof(StructureType));
            structurePanel = MakePanel(
                Label("Structure"),
                structureTypeComboBox);

            // ── owner panel (unit + structure) ───────────────────────────
            ownerNumeric = new NumericUpDown { Minimum = 0, Maximum = 7, Width = 55 };
            ownerPanel = MakePanel(Label("Owner"), ownerNumeric);

            // ── status ───────────────────────────────────────────────────
            statusTextBox = new TextBox { Width = 380, ReadOnly = true, Margin = new Padding(8, 8, 0, 0) };

            toolPanel.Controls.Add(newButton);
            toolPanel.Controls.Add(openButton);
            toolPanel.Controls.Add(saveButton);
            toolPanel.Controls.Add(saveAsButton);
            toolPanel.Controls.Add(simulateButton);
            toolPanel.Controls.Add(undoButton);
            toolPanel.Controls.Add(redoButton);
            toolPanel.Controls.Add(simPanel);
            toolPanel.Controls.Add(Label("Tool", leftPad: 10));
            toolPanel.Controls.Add(toolComboBox);
            toolPanel.Controls.Add(terrainPanel);
            toolPanel.Controls.Add(edgePanel);
            toolPanel.Controls.Add(unitPanel);
            toolPanel.Controls.Add(structurePanel);
            toolPanel.Controls.Add(ownerPanel);
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
            undoButton.Click    += (_, __) => PerformUndo();
            redoButton.Click    += (_, __) => PerformRedo();
            simPrevButton.Click    += (_, __) => SimStepBack();
            simNextButton.Click    += (_, __) => SimStepForward();
            simRestartButton.Click += (_, __) => SimRestart();
            simExitButton.Click    += (_, __) => ExitSimulation();
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            canvas.MouseClick   += CanvasMouseClick;
            canvas.MouseDown    += CanvasMouseDown;
            canvas.MouseMove    += CanvasMouseMove;
            canvas.MouseUp      += CanvasMouseUp;
            toolComboBox.SelectedIndexChanged += (_, __) => UpdateToolPanels();
            terrainPalette.SelectionChanged += (_, t) => lastPaintedTerrainType = t;

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

            terrainPanel.Visible   = tool == EditorTool.Terrain;
            edgePanel.Visible      = tool == EditorTool.Edge;
            unitPanel.Visible      = tool == EditorTool.Unit;
            structurePanel.Visible = tool == EditorTool.Structure;
            ownerPanel.Visible     = tool == EditorTool.Unit || tool == EditorTool.Structure;
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (session != null)
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
            if (!history.CanUndo) return;
            var doc = history.Undo(MapDocument.FromBoard(board));
            board = doc.ToBoard();
            selectedTile = null;
            UpdateUndoRedoButtons();
            RenderBoard();
            SetStatus("Undo");
        }

        void PerformRedo()
        {
            if (!history.CanRedo) return;
            var doc = history.Redo(MapDocument.FromBoard(board));
            board = doc.ToBoard();
            selectedTile = null;
            UpdateUndoRedoButtons();
            RenderBoard();
            SetStatus("Redo");
        }

        // Captured before-state for the current edit gesture.
        MapDocument editBefore;

        /// <summary>Snapshot the current state before a mutation begins.</summary>
        void BeginEdit() => editBefore = MapDocument.FromBoard(board);

        /// <summary>Diff before→after and push to undo stack; call after the mutation is applied.</summary>
        void CommitEdit(string description)
        {
            if (editBefore == null) return;
            history.Commit(editBefore, MapDocument.FromBoard(board), description);
            editBefore = null;
            UpdateUndoRedoButtons();
        }

        void UpdateUndoRedoButtons()
        {
            undoButton.Enabled = history.CanUndo;
            redoButton.Enabled = history.CanRedo;
        }

        // ── map operations ───────────────────────────────────────────────

        void LoadDefaultMap()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            board = new Board(
                File.ReadAllLines(Path.Combine(basePath, "BasicBoard.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardEdges.txt")),
                File.ReadAllLines(Path.Combine(basePath, "BasicBoardStructures.txt")));
            currentFilePath = null;
            selectedTile = null;
            RenderBoard();
        }

        void CreateNewMap()
        {
            using (var dialog = new NewMapForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                board = BoardEditorService.CreateDefaultBoard(dialog.MapWidth, dialog.MapHeight);
                currentFilePath = null;
                selectedTile = null;
                history.Clear();
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

                board = MapDocument.Load(dialog.FileName).ToBoard();
                currentFilePath = dialog.FileName;
                selectedTile = null;
                history.Clear();
                history.TryLoad(dialog.FileName);
                UpdateUndoRedoButtons();
                RenderBoard();
            }
        }

        void SaveMap(bool saveAs)
        {
            var path = currentFilePath;
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

            MapDocument.FromBoard(board).Save(path);
            history.Save(path);
            currentFilePath = path;
            SetStatus($"Saved {Path.GetFileName(path)}");
        }

        void EnterSimulation()
        {
            session = BoardEditorService.StartSimulation(board);
            simPanel.Visible = true;
            undoButton.Visible = false;
            redoButton.Visible = false;
            canvas.MouseClick -= CanvasMouseClick;
            canvas.MouseDown  -= CanvasMouseDown;
            canvas.MouseMove  -= CanvasMouseMove;
            canvas.MouseUp    -= CanvasMouseUp;
            UpdateSimButtons();
            board = session.CurrentBoard;
            RenderBoard();
            SetStatus(session.StatusLine());
        }

        void ExitSimulation()
        {
            session = null;
            simPanel.Visible = false;
            undoButton.Visible = true;
            redoButton.Visible = true;
            canvas.MouseClick += CanvasMouseClick;
            canvas.MouseDown  += CanvasMouseDown;
            canvas.MouseMove  += CanvasMouseMove;
            canvas.MouseUp    += CanvasMouseUp;
            RenderBoard();
            SetStatus("Simulation exited.");
        }

        void SimStepForward()
        {
            if (session == null) return;
            session.StepForward();
            board = session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(session.StatusLine() + (session.IsFinished && !session.CanStepForward ? "  [Simulation ended]" : ""));
        }

        void SimStepBack()
        {
            if (session == null) return;
            session.StepBack();
            board = session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(session.StatusLine());
        }

        void SimRestart()
        {
            if (session == null) return;
            session.Restart();
            board = session.CurrentBoard;
            UpdateSimButtons();
            RenderBoard();
            SetStatus(session.StatusLine());
        }

        void UpdateSimButtons()
        {
            simPrevButton.Enabled    = session?.CanStepBack ?? false;
            simNextButton.Enabled    = session?.CanStepForward ?? false;
            simRestartButton.Enabled = session?.CanStepBack ?? false;
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
            isPaintingTerrain = true;
            lastPaintedTileIndex = null;
            lastPaintedTerrainType = terrainPalette.SelectedTerrain;
            PaintTerrainAt(e.X, e.Y);
        }

        void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!isPaintingTerrain || e.Button != MouseButtons.Left)
                return;

            PaintTerrainAt(e.X, e.Y);
        }

        void CanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (isPaintingTerrain && lastPaintedTileIndex.HasValue)
            {
                board = BoardEditorService.RebuildBoard(board);
                CommitEdit("Paint terrain");
            }

            isPaintingTerrain = false;
            lastPaintedTileIndex = null;
        }

        void PaintTerrainAt(int x, int y)
        {
            var tile = BoardEditorService.HitTest(board, x, y);
            if (tile == null || lastPaintedTileIndex == tile.Index)
                return;

            BoardEditorService.SetTerrainDirect(tile, lastPaintedTerrainType);
            selectedTile = null;
            lastPaintedTileIndex = tile.Index;
            SetStatus($"Terrain changed at tile {tile.Index}");
            RenderBoard();
        }

        void CanvasMouseClick(object sender, MouseEventArgs e)
        {
            var tile = BoardEditorService.HitTest(board, e.X, e.Y);
            if (tile == null)
                return;

            switch ((EditorTool)toolComboBox.SelectedItem)
            {
                case EditorTool.Terrain:
                    return;
                case EditorTool.Structure:
                    BeginEdit();
                    board = BoardEditorService.SetStructure(board, tile, (StructureType)structureTypeComboBox.SelectedItem, (int)ownerNumeric.Value);
                    CommitEdit($"Set structure at {tile.Index}");
                    selectedTile = null;
                    SetStatus($"Structure updated at tile {tile.Index}");
                    break;
                case EditorTool.Unit:
                    BeginEdit();
                    board = BoardEditorService.AddUnit(board, tile, (UnitType)unitTypeComboBox.SelectedItem, (MovementType)movementTypeComboBox.SelectedItem, (int)ownerNumeric.Value);
                    CommitEdit($"Add unit at {tile.Index}");
                    selectedTile = null;
                    SetStatus($"Unit added at tile {tile.Index}");
                    break;
                case EditorTool.Erase:
                    BeginEdit();
                    if (selectedTile != null && BoardEditorService.AreAdjacent(board[selectedTile.Index], board[tile.Index]))
                    {
                        board = BoardEditorService.SetEdge(board, board[selectedTile.Index], board[tile.Index], EdgeType.None, false);
                        CommitEdit($"Remove edge {selectedTile.Index}-{tile.Index}");
                        SetStatus($"Edge removed between {selectedTile.Index} and {tile.Index}");
                        selectedTile = null;
                    }
                    else
                    {
                        board = BoardEditorService.EraseTileContent(board, tile);
                        CommitEdit($"Erase tile {tile.Index}");
                        SetStatus($"Tile content removed at {tile.Index}");
                        selectedTile = null;
                    }
                    break;
                case EditorTool.Edge:
                    if (selectedTile == null)
                    {
                        selectedTile = tile;
                        SetStatus($"Selected tile {selectedTile.Index}. Click adjacent tile for edge.");
                        RenderBoard();
                        return;
                    }

                    if (!BoardEditorService.AreAdjacent(board[selectedTile.Index], board[tile.Index]))
                    {
                        selectedTile = tile;
                        SetStatus($"Selected tile {selectedTile.Index}. Click adjacent tile for edge.");
                        RenderBoard();
                        return;
                    }

                    BeginEdit();
                    board = BoardEditorService.SetEdge(board, board[selectedTile.Index], board[tile.Index], (EdgeType)edgeComboBox.SelectedItem, roadCheckBox.Checked);
                    CommitEdit($"Set edge {selectedTile.Index}-{tile.Index}");
                    SetStatus($"Edge updated between {selectedTile.Index} and {tile.Index}");
                    selectedTile = null;
                    break;
            }

            RenderBoard();
        }

        // ── rendering ────────────────────────────────────────────────────

        void RenderBoard()
        {
            var selectedTool = toolComboBox.SelectedItem is EditorTool tool ? tool : EditorTool.Terrain;
            var showSelectedTile = selectedTool == EditorTool.Edge && selectedTile != null;
            foreach (var tile in board.Tiles)
                tile.IsSelected = showSelectedTile && tile.Index == selectedTile.Index;

            var drawing = GameBoardRenderer.Render(RenderPipeline.Board, RenderPipeline.Units, board.Width, board.Height, board.Tiles, board.Edges, board.Structures, null, null, board.Units);
            var previous = canvas.Image;
            canvas.Image = new Bitmap(drawing.ToBitmap());
            previous?.Dispose();
        }

        void SetStatus(string message) => statusTextBox.Text = message;
    }
}
