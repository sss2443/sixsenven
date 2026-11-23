using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class BoardPanel : Control
    {
        private readonly GameEngine engine;
        private readonly Timer timer;
        private readonly Dictionary<int, DateTime> flashUntil = new Dictionary<int, DateTime>();
        private int hoverIndex = -1;
        private int searchIndex = -1;
        private double progress;
        private DateTime lastTick = DateTime.Now;
        private Point mousePosition;

        public event Action<Item> ItemFound;
        public event Action BoardCleared;

        public BoardPanel(GameEngine engine)
        {
            this.engine = engine;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
            Cursor = Cursors.Cross;
            timer = new Timer { Interval = 16 };
            timer.Tick += (s, e) => Tick();
            timer.Start();
        }

        public void ResetInteraction()
        {
            hoverIndex = -1;
            searchIndex = -1;
            progress = 0;
            flashUntil.Clear();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Tick()
        {
            var now = DateTime.Now;
            double dt = Math.Min(0.05, Math.Max(0, (now - lastTick).TotalSeconds));
            lastTick = now;

            var board = engine.Board;
            if (board != null)
            {
                if (searchIndex >= 0 && hoverIndex == searchIndex && searchIndex < board.Cells.Count)
                {
                    var cell = board.Cells[searchIndex];
                    if (cell.Item != null && !cell.Revealed)
                    {
                        var tier = BoxTierDefinition.ById(board.TierId);
                        progress += dt / Math.Max(0.25, tier.SearchSeconds);
                        if (progress >= 1.0)
                        {
                            progress = 0;
                            int index = searchIndex;
                            searchIndex = -1;
                            var item = engine.RevealCell(index);
                            if (item != null)
                            {
                                flashUntil[index] = now.AddMilliseconds(700);
                                ItemFound?.Invoke(item);
                                if (board.IsCleared)
                                    BoardCleared?.Invoke();
                            }
                        }
                    }
                }
                else
                {
                    progress -= dt * 1.8;
                    if (progress < 0) progress = 0;
                }
            }

            bool needsPaint = progress > 0.001 || searchIndex >= 0 || flashUntil.Count > 0;
            if (needsPaint)
            {
                var expired = flashUntil.Where(kv => kv.Value < now).Select(kv => kv.Key).ToList();
                foreach (var key in expired) flashUntil.Remove(key);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawBackground(g);

            var board = engine.Board;
            if (board == null || board.Cells.Count == 0)
            {
                DrawEmptyState(g);
                return;
            }

            var tiles = ComputeTiles(board.Columns, board.Rows);
            var tier = BoxTierDefinition.ById(board.TierId);
            Color tierColor = Color.FromArgb(tier.AccentArgb);

            using (var brushValue = new SolidBrush(Theme.WithAlpha(tierColor, 215)))
            {
                for (int i = 0; i < board.Cells.Count && i < tiles.Length; i++)
                {
                    DrawTile(g, tiles[i], board.Cells[i], i == hoverIndex, tierColor, Theme.FontSmall, brushValue);
                }
            }

            if (progress > 0.01 && hoverIndex >= 0 && hoverIndex < tiles.Length)
            {
                var cell = board.Cells[hoverIndex];
                if (cell.Item != null && !cell.Revealed)
                {
                    DrawMagnifier(g, tiles[hoverIndex], cell.Item, (float)progress);
                }
            }

            if (board.IsCleared)
            {
                DrawClearedBanner(g, board);
            }
        }

        private void DrawBackground(Graphics g)
        {
            using (var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(13, 18, 29), Color.FromArgb(8, 11, 18), 90f))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // 淡淡的方格纸纹理，呼应草图上画的格子
            using (var pen = new Pen(Color.FromArgb(14, 255, 255, 255)))
            {
                for (int x = 0; x < Width; x += 44)
                    g.DrawLine(pen, x, 0, x, Height);
                for (int y = 0; y < Height; y += 44)
                    g.DrawLine(pen, 0, y, Width, y);
            }
        }

        private void DrawEmptyState(Graphics g)
        {
            var center = new RectangleF(0, 0, Width, Height);
            using (var title = new Font(Theme.FontTitle.FontFamily, 15f, FontStyle.Bold))
            using (var brush = new SolidBrush(Theme.SubText))
            using (var gold = new SolidBrush(Theme.Accent))
            {
                var text1 = "挑一个箱子开始寻宝";
                var text2 = "灰箱免费 · 绿箱 ¥1,000 · 蓝箱 ¥5,000 · 紫箱 ¥10,000 · 红箱 ¥30,000";
                var size1 = g.MeasureString(text1, title);
                var size2 = g.MeasureString(text2, Theme.FontBody);
                g.DrawString(text1, title, brush, center.Width / 2 - size1.Width / 2, center.Height / 2 - 46);
                g.DrawString(text2, Theme.FontBody, new SolidBrush(Theme.WithAlpha(Theme.SubText, 170)),
                    center.Width / 2 - size2.Width / 2, center.Height / 2 + 4);
                g.DrawString("按住鼠标左键，在阴影物品上按住不放", Theme.FontBody, gold,
                    center.Width / 2 - g.MeasureString("按住鼠标左键，在阴影物品上按住不放", Theme.FontBody).Width / 2,
                    center.Height / 2 + 32);
            }
        }

        private RectangleF[] ComputeTiles(int columns, int rows)
        {
            float pad = 24f;
            float gap = 14f;
            float availW = Width - pad * 2 - gap * (columns - 1);
            float availH = Height - pad * 2 - gap * (rows - 1);
            float size = Math.Max(70f, Math.Min(availW / columns, availH / rows));
            float totalW = size * columns + gap * (columns - 1);
            float totalH = size * rows + gap * (rows - 1);
            float ox = (Width - totalW) / 2f;
            float oy = (Height - totalH) / 2f;

            var tiles = new RectangleF[columns * rows];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    tiles[row * columns + col] = new RectangleF(
                        ox + col * (size + gap),
                        oy + row * (size + gap),
                        size, size);
                }
            }
            return tiles;
        }

        private void DrawTile(Graphics g, RectangleF rect, CellState cell, bool hover, Color tierColor, Font valueFont, Brush valueBrush)
        {
            bool hasItem = cell.Item != null;
            bool revealed = cell.Revealed;
            Color itemColor = hasItem ? Theme.RarityColor(cell.Item.Rarity) : Theme.SubText;

            var fill = revealed ? Theme.TileRevealed : Theme.Tile;
            using (var path = new GraphicsPath())
            {
                AddRounded(path, rect, 14f);
                using (var brush = new LinearGradientBrush(rect, Theme.Lighten(fill, 0.06f), Theme.Darken(fill, 0.18f), 90f))
                {
                    g.FillPath(brush, path);
                }

                Color borderColor = revealed
                    ? Theme.WithAlpha(Theme.Lighten(itemColor, 0.3f), 150)
                    : Theme.Line;
                if (hover && hasItem && !revealed)
                    borderColor = Theme.WithAlpha(Theme.Accent, 200);

                using (var pen = new Pen(borderColor, hover && hasItem && !revealed ? 2f : 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }

            var inner = RectangleF.Inflate(rect, -rect.Width * 0.2f, -rect.Height * 0.24f);
            inner.Y += rect.Height * 0.02f;

            if (hasItem && !revealed)
            {
                ItemArt.DrawShadow(g, inner, cell.Item.Shape, 235);
            }
            else if (hasItem && revealed)
            {
                ItemArt.DrawItem(g, inner, cell.Item.Shape, itemColor);

                string valueText = "¥" + cell.Item.Value.ToString("N0");
                var size = g.MeasureString(valueText, valueFont);
                g.DrawString(valueText, valueFont, valueBrush,
                    rect.X + rect.Width / 2 - size.Width / 2, rect.Bottom - size.Height - 8);

                if (flashUntil.TryGetValue(cell.Index, out var until) && DateTime.Now < until)
                {
                    double t = (until - DateTime.Now).TotalMilliseconds / 700.0;
                    using (var path = new GraphicsPath())
                    {
                        AddRounded(path, rect, 14f);
                        using (var pen = new Pen(Color.FromArgb((int)(200 * t), Theme.Accent), 3f))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                }
            }
            else
            {
                using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 2f))
                using (var brush = new SolidBrush(Color.FromArgb(34, 255, 255, 255)))
                {
                    var dot = new RectangleF(rect.X + rect.Width / 2 - 3, rect.Y + rect.Height / 2 - 3, 6, 6);
                    g.FillEllipse(brush, dot);
                }
            }
        }

        private void DrawMagnifier(Graphics g, RectangleF tile, Item item, float prog)
        {
            float lensR = Math.Max(26f, tile.Width * 0.27f);
            var center = new PointF(tile.X + tile.Width / 2f, tile.Y + tile.Height / 2f);
            if (mousePosition != Point.Empty)
            {
                center.X = Math.Max(tile.X + lensR * 0.55f, Math.Min(tile.Right - lensR * 1.35f, mousePosition.X));
                center.Y = Math.Max(tile.Y + lensR * 0.55f, Math.Min(tile.Bottom - lensR * 1.35f, mousePosition.Y));
            }

            var lens = new RectangleF(center.X - lensR, center.Y - lensR, lensR * 2, lensR * 2);

            // 手柄
            using (var pen = new Pen(Color.FromArgb(210, 120, 138, 168), lensR * 0.22f))
            {
                float ux = 0.72f, uy = 0.72f;
                var p1 = new PointF(center.X + lensR * ux, center.Y + lensR * uy);
                var p2 = new PointF(center.X + lensR * (ux + 0.55f), center.Y + lensR * (uy + 0.55f));
                g.DrawLine(pen, p1, p2);
            }

            // 进度底环 + 进度弧
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 4f))
            {
                g.DrawEllipse(pen, RectangleF.Inflate(lens, 4, 4));
            }
            if (prog > 0.001f)
            {
                using (var pen = new Pen(Theme.Accent, 5f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, RectangleF.Inflate(lens, 4, 4), -90f, Math.Min(359.9f, 360f * prog));
                }
            }

            // 镜片玻璃
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(lens);
                using (var brush = new PathGradientBrush(path)
                {
                    CenterColor = Color.FromArgb(56, 76, 112),
                    SurroundColors = new[] { Color.FromArgb(8, 11, 18) }
                })
                {
                    g.FillPath(brush, path);
                }

                var old = g.Clip;
                g.SetClip(path, CombineMode.Intersect);
                var magnified = new RectangleF(
                    center.X - lensR * 1.55f,
                    center.Y - lensR * 1.35f,
                    lensR * 3.1f,
                    lensR * 2.7f);
                ItemArt.DrawShadow(g, magnified, item.Shape, 245);
                g.Clip = old;
            }

            using (var pen = new Pen(Color.FromArgb(220, 210, 224, 246), 2.2f))
            {
                g.DrawEllipse(pen, lens);
            }
            using (var brush = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
            {
                g.FillEllipse(brush,
                    lens.X + lens.Width * 0.20f, lens.Y + lens.Height * 0.14f,
                    lens.Width * 0.34f, lens.Height * 0.22f);
            }
        }

        private void DrawClearedBanner(Graphics g, BoardState board)
        {
            var tier = BoxTierDefinition.ById(board.TierId);
            string text = $"「{tier.Name}」已搜完 · 共 {board.ItemTotalCount} 件 · 总价值 ¥{board.RevealedValue:N0}";
            var size = g.MeasureString(text, Theme.FontButton);
            var rect = new RectangleF(Width / 2f - size.Width / 2f - 22, 14, size.Width + 44, size.Height + 20);

            using (var path = new GraphicsPath())
            {
                AddRounded(path, rect, 18f);
                using (var brush = new SolidBrush(Color.FromArgb(232, 24, 32, 50)))
                    g.FillPath(brush, path);
                using (var pen = new Pen(Theme.WithAlpha(Theme.Accent, 200), 1.6f))
                    g.DrawPath(pen, path);
            }
            using (var brush = new SolidBrush(Theme.Accent))
            {
                g.DrawString(text, Theme.FontButton, brush,
                    rect.X + 22, rect.Y + (rect.Height - size.Height) / 2f);
            }
        }

        private static void AddRounded(GraphicsPath path, RectangleF rect, float radius)
        {
            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
        }

        private int HitTest(Point location)
        {
            var board = engine.Board;
            if (board == null) return -1;
            var tiles = ComputeTiles(board.Columns, board.Rows);
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i].Contains(location)) return i;
            }
            return -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button != MouseButtons.Left) return;
            var board = engine.Board;
            if (board == null) return;
            int index = HitTest(e.Location);
            if (index < 0) return;
            var cell = board.Cells[index];
            if (cell.Item == null || cell.Revealed) return;
            hoverIndex = index;
            searchIndex = index;
            progress = 0;
            mousePosition = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mousePosition = e.Location;
            int index = HitTest(e.Location);
            if (index != hoverIndex)
            {
                hoverIndex = index;
                if (searchIndex != index)
                {
                    searchIndex = -1;
                    progress = 0;
                }
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            searchIndex = -1;
            progress = 0;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoverIndex = -1;
            searchIndex = -1;
            progress = 0;
            mousePosition = Point.Empty;
        }
    }
}
