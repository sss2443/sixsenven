using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using LootSearch.Core;

namespace LootSearch.UI
{
    internal static class ItemArt
    {
        public static void DrawShadow(Graphics g, RectangleF area, ItemShape shape, int alpha)
        {
            var color = Color.FromArgb(alpha, Theme.Shadow);
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(Color.FromArgb(Math.Min(255, alpha + 25), 20, 28, 46), Math.Max(1f, area.Width * 0.02f)))
            {
                DrawShape(g, area, shape, brush, pen);
            }
        }

        public static void DrawItem(Graphics g, RectangleF area, ItemShape shape, Color color)
        {
            DrawGlow(g, area, color, 110);

            using (var path = BuildPath(shape, area))
            using (var brush = new LinearGradientBrush(
                       new RectangleF(area.X, area.Y, Math.Max(1f, area.Width), Math.Max(1f, area.Height)),
                       Theme.Lighten(color, 0.55f), Theme.Darken(color, 0.28f), 55f))
            {
                if (shape == ItemShape.Ring)
                {
                    DrawRingStroke(g, area, brush, color);
                }
                else
                {
                    g.FillPath(brush, path);
                    using (var pen = new Pen(Theme.Lighten(color, 0.75f), 1.4f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            DrawDetails(g, area, shape, color);
        }

        private static void DrawGlow(Graphics g, RectangleF area, Color color, int strength)
        {
            var bounds = RectangleF.Inflate(area, area.Width * 0.28f, area.Height * 0.28f);
            for (int i = 3; i >= 1; i--)
            {
                float scale = i / 3f;
                var r = RectangleF.Inflate(bounds, -bounds.Width * (1 - scale) * 0.6f, -bounds.Height * (1 - scale) * 0.6f);
                using (var path = BuildPath(ItemShape.Coin, r))
                using (var brush = new SolidBrush(Color.FromArgb((int)(strength * (1 - scale) * 0.55f), color)))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private static void DrawDetails(Graphics g, RectangleF area, ItemShape shape, Color color)
        {
            using (var pen = new Pen(Theme.WithAlpha(Theme.Lighten(color, 0.85f), 130), Math.Max(1f, area.Width * 0.018f)))
            {
                switch (shape)
                {
                    case ItemShape.Book:
                        g.DrawLine(pen,
                            area.X + area.Width * 0.32f, area.Y + area.Height * 0.30f,
                            area.X + area.Width * 0.32f, area.Y + area.Height * 0.80f);
                        break;
                    case ItemShape.Coin:
                        g.DrawEllipse(pen,
                            area.X + area.Width * 0.20f, area.Y + area.Height * 0.20f,
                            area.Width * 0.60f, area.Height * 0.60f);
                        break;
                    case ItemShape.Fish:
                        using (var brush = new SolidBrush(Theme.WithAlpha(Color.White, 180)))
                        {
                            g.FillEllipse(brush,
                                area.X + area.Width * 0.24f, area.Y + area.Height * 0.42f,
                                Math.Max(2f, area.Width * 0.06f), Math.Max(2f, area.Height * 0.06f));
                        }
                        break;
                    case ItemShape.Scroll:
                        g.DrawLine(pen,
                            area.X + area.Width * 0.26f, area.Y + area.Height * 0.42f,
                            area.X + area.Width * 0.74f, area.Y + area.Height * 0.42f);
                        g.DrawLine(pen,
                            area.X + area.Width * 0.26f, area.Y + area.Height * 0.56f,
                            area.X + area.Width * 0.66f, area.Y + area.Height * 0.56f);
                        break;
                }
            }
        }

        private static void DrawRingStroke(Graphics g, RectangleF area, Brush brush, Color color)
        {
            float thickness = Math.Min(area.Width, area.Height) * 0.13f;
            var rect = new RectangleF(
                area.X + area.Width * 0.12f,
                area.Y + area.Height * 0.28f,
                area.Width * 0.76f,
                area.Height * 0.66f);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (var pen = new Pen(Theme.Lighten(color, 0.35f), thickness))
                {
                    pen.Alignment = PenAlignment.Center;
                    g.DrawEllipse(pen, rect);
                }
            }

            float gemSize = Math.Min(area.Width, area.Height) * 0.22f;
            var gemRect = new RectangleF(
                area.X + area.Width * 0.5f - gemSize * 0.5f,
                area.Y + area.Height * 0.10f,
                gemSize,
                gemSize);
            using (var gem = new GraphicsPath())
            {
                gem.AddPolygon(new[]
                {
                    new PointF(gemRect.X + gemRect.Width * 0.5f, gemRect.Y),
                    new PointF(gemRect.Right, gemRect.Y + gemRect.Height * 0.45f),
                    new PointF(gemRect.X + gemRect.Width * 0.5f, gemRect.Bottom),
                    new PointF(gemRect.X, gemRect.Y + gemRect.Height * 0.45f)
                });
                g.FillPath(brush, gem);
            }
        }

        public static GraphicsPath BuildPath(ItemShape shape, RectangleF r)
        {
            var path = new GraphicsPath();

            switch (shape)
            {
                case ItemShape.Gem:
                    path.AddPolygon(new[]
                    {
                        P(r, 0.50f, 0.06f), P(r, 0.88f, 0.32f), P(r, 0.72f, 0.94f),
                        P(r, 0.28f, 0.94f), P(r, 0.12f, 0.32f)
                    });
                    break;

                case ItemShape.Coin:
                    path.AddEllipse(Inset(r, 0.05f, 0.18f, 0.90f, 0.68f));
                    break;

                case ItemShape.Ring:
                    path.AddEllipse(Inset(r, 0.12f, 0.28f, 0.76f, 0.66f));
                    break;

                case ItemShape.Key:
                    path.AddEllipse(Inset(r, 0.04f, 0.22f, 0.34f, 0.34f));
                    path.AddRectangle(Inset(r, 0.31f, 0.43f, 0.50f, 0.13f));
                    path.AddRectangle(Inset(r, 0.66f, 0.50f, 0.10f, 0.15f));
                    path.AddRectangle(Inset(r, 0.78f, 0.50f, 0.10f, 0.20f));
                    break;

                case ItemShape.Bottle:
                    path.AddRectangle(Inset(r, 0.40f, 0.05f, 0.20f, 0.14f));
                    path.AddRectangle(Inset(r, 0.44f, 0.16f, 0.12f, 0.16f));
                    path.AddEllipse(Inset(r, 0.22f, 0.28f, 0.56f, 0.66f));
                    break;

                case ItemShape.Star:
                    path.AddPolygon(StarPoints(r));
                    break;

                case ItemShape.Book:
                    path.AddPolygon(new[]
                    {
                        P(r, 0.16f, 0.16f), P(r, 0.86f, 0.22f), P(r, 0.86f, 0.84f), P(r, 0.16f, 0.90f)
                    });
                    break;

                case ItemShape.Crown:
                    path.AddPolygon(new[]
                    {
                        P(r, 0.10f, 0.74f), P(r, 0.06f, 0.26f), P(r, 0.32f, 0.50f),
                        P(r, 0.50f, 0.14f), P(r, 0.68f, 0.50f), P(r, 0.94f, 0.26f), P(r, 0.90f, 0.74f)
                    });
                    path.AddRectangle(Inset(r, 0.10f, 0.76f, 0.80f, 0.14f));
                    break;

                case ItemShape.Gear:
                    path.AddPolygon(GearPoints(r));
                    break;

                case ItemShape.Fish:
                    path.AddEllipse(Inset(r, 0.08f, 0.30f, 0.62f, 0.44f));
                    path.AddPolygon(new[]
                    {
                        P(r, 0.66f, 0.50f), P(r, 0.98f, 0.22f), P(r, 0.98f, 0.78f)
                    });
                    break;

                case ItemShape.Scroll:
                    path.AddRectangle(Inset(r, 0.18f, 0.22f, 0.64f, 0.58f));
                    path.AddEllipse(Inset(r, 0.08f, 0.18f, 0.18f, 0.66f));
                    path.AddEllipse(Inset(r, 0.74f, 0.18f, 0.18f, 0.66f));
                    break;
            }

            return path;
        }

        private static RectangleF Inset(RectangleF r, float x, float y, float w, float h)
        {
            return new RectangleF(r.X + r.Width * x, r.Y + r.Height * y, r.Width * w, r.Height * h);
        }

        private static PointF P(RectangleF r, float x, float y)
        {
            return new PointF(r.X + r.Width * x, r.Y + r.Height * y);
        }

        private static PointF[] StarPoints(RectangleF r)
        {
            var points = new PointF[10];
            float cx = r.X + r.Width * 0.5f;
            float cy = r.Y + r.Height * 0.5f;
            float outer = Math.Min(r.Width, r.Height) * 0.46f;
            float inner = outer * 0.44f;
            for (int i = 0; i < 10; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                float radius = (i % 2 == 0) ? outer : inner;
                points[i] = new PointF(
                    cx + (float)Math.Cos(angle) * radius,
                    cy + (float)Math.Sin(angle) * radius);
            }
            return points;
        }

        private static PointF[] GearPoints(RectangleF r)
        {
            const int teeth = 8;
            int count = teeth * 2;
            var points = new PointF[count];
            float cx = r.X + r.Width * 0.5f;
            float cy = r.Y + r.Height * 0.5f;
            float outer = Math.Min(r.Width, r.Height) * 0.48f;
            float inner = outer * 0.74f;
            for (int i = 0; i < count; i++)
            {
                double angle = i * Math.PI * 2 / count;
                float radius = (i % 2 == 0) ? outer : inner;
                points[i] = new PointF(
                    cx + (float)Math.Cos(angle) * radius,
                    cy + (float)Math.Sin(angle) * radius);
            }
            return points;
        }

        private static void DrawShape(Graphics g, RectangleF area, ItemShape shape, Brush brush, Pen pen)
        {
            if (shape == ItemShape.Ring)
            {
                DrawRingStroke(g, area, brush, pen.Color);
                return;
            }

            using (var path = BuildPath(shape, area))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            if (shape == ItemShape.Book)
            {
                using (var p2 = new Pen(Theme.WithAlpha(pen.Color, 160), Math.Max(1f, area.Width * 0.018f)))
                {
                    g.DrawLine(p2,
                        area.X + area.Width * 0.32f, area.Y + area.Height * 0.26f,
                        area.X + area.Width * 0.32f, area.Y + area.Height * 0.84f);
                }
            }
        }
    }
}
