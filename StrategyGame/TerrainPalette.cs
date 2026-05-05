using GameModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace StrategyGame
{
    /// <summary>
    /// A palette of clickable flat-top hexes, one per terrain type, matching the map colours.
    /// The selected terrain is highlighted with a white border.
    /// </summary>
    internal class TerrainPalette : Control
    {
        static readonly TerrainType[] Terrains =
        {
            TerrainType.Grassland,
            TerrainType.Steppe,
            TerrainType.Forest,
            TerrainType.Hill,
            TerrainType.Mountain,
            TerrainType.Water,
            TerrainType.Wetland,
            TerrainType.Reef,
        };

        static readonly Dictionary<TerrainType, Color> TerrainColours = new Dictionary<TerrainType, Color>
        {
            { TerrainType.Grassland, Color.FromArgb(173, 255, 47) },
            { TerrainType.Steppe,    Color.FromArgb(255, 255, 0) },
            { TerrainType.Forest,    Color.FromArgb(0,   100, 0) },
            { TerrainType.Hill,      Color.FromArgb(244, 164, 96) },
            { TerrainType.Mountain,  Color.FromArgb(165, 42,  42) },
            { TerrainType.Water,     Color.FromArgb(173, 216, 230) },
            { TerrainType.Wetland,   Color.FromArgb(169, 169, 169) },
            { TerrainType.Reef,      Color.FromArgb(0,   0,   139) },
        };

        const int HexSize = 22;   // flat-to-flat radius

        public TerrainType SelectedTerrain { get; private set; } = TerrainType.Grassland;

        public event EventHandler<TerrainType> SelectionChanged;

        public TerrainPalette()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw, true);

            // Size to fit all terrains in one row
            var (w, h) = MeasureSize();
            Width = w;
            Height = h;
            MinimumSize = new Size(w, h);
        }

        static (int w, int h) MeasureSize()
        {
            // flat-top hex: width = sqrt(3) * size, height = 2 * size, offset each hex by width
            int hexW = (int)(Math.Sqrt(3) * HexSize) + 2;
            int hexH = HexSize * 2 + 2;
            int totalW = hexW * Terrains.Length + 4;
            return (totalW, hexH + 4);
        }

        PointF[] HexCorners(float cx, float cy)
        {
            // Pointy-top hex corners to match the map renderer
            var pts = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 180.0 * (60 * i - 30);
                pts[i] = new PointF(
                    cx + HexSize * (float)Math.Cos(angle),
                    cy + HexSize * (float)Math.Sin(angle));
            }
            return pts;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int hexW = (int)(Math.Sqrt(3) * HexSize) + 2;
            float cx = hexW / 2f + 2;
            float cy = Height / 2f;

            for (int i = 0; i < Terrains.Length; i++)
            {
                var terrain = Terrains[i];
                var pts = HexCorners(cx + i * hexW, cy);
                var color = TerrainColours[terrain];

                using (var brush = new SolidBrush(color))
                    g.FillPolygon(brush, pts);

                bool selected = terrain == SelectedTerrain;
                using (var pen = new Pen(selected ? Color.White : Color.Black, selected ? 3f : 1f))
                    g.DrawPolygon(pen, pts);

                // Label
                var name = terrain.ToString();
                using (var font = new Font("Arial", 6f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    var textColor = IsLightColour(color) ? Color.Black : Color.White;
                    using (var tb = new SolidBrush(textColor))
                        g.DrawString(name.Length > 6 ? name.Substring(0, 5) : name, font, tb,
                            cx + i * hexW, cy, sf);
                }
            }
        }

        static bool IsLightColour(Color c) => (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 128;

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int hexW = (int)(Math.Sqrt(3) * HexSize) + 2;
            float cy = Height / 2f;

            // Find closest hex centre
            float closest = float.MaxValue;
            TerrainType picked = SelectedTerrain;
            for (int i = 0; i < Terrains.Length; i++)
            {
                float cx = hexW / 2f + 2 + i * hexW;
                float dist = Math.Abs(e.X - cx) + Math.Abs(e.Y - cy);
                if (dist < closest)
                {
                    closest = dist;
                    picked = Terrains[i];
                }
            }

            if (picked != SelectedTerrain)
            {
                SelectedTerrain = picked;
                Invalidate();
                SelectionChanged?.Invoke(this, SelectedTerrain);
            }
        }
    }
}
