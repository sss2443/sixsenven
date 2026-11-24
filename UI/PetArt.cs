using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using LootSearch.Core;

namespace LootSearch.UI
{
    internal static class PetArt
    {
        public static Color KindColor(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Dragon: return Color.FromArgb(70, 214, 190);
                case PetKind.Cat: return Color.FromArgb(186, 166, 240);
                default: return Color.FromArgb(222, 160, 96);
            }
        }

        public static void Draw(Graphics g, RectangleF area, PetKind kind, bool equipped)
        {
            var color = KindColor(kind);
            float size = Math.Min(area.Width, area.Height);
            float cx = area.X + area.Width / 2f;
            float cy = area.Y + area.Height / 2f;
            float headR = size * 0.34f;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (equipped)
            {
                using (var brush = new SolidBrush(Color.FromArgb(46, color)))
                {
                    g.FillEllipse(brush,
                        cx - headR * 1.9f, cy - headR * 1.9f,
                        headR * 3.8f, headR * 3.8f);
                }
            }

            using (var face = new LinearGradientBrush(
                       new RectangleF(cx - headR, cy - headR, headR * 2, headR * 2),
                       Theme.Lighten(color, 0.35f), Theme.Darken(color, 0.2f), 60f))
            {
                g.FillEllipse(face, cx - headR, cy - headR, headR * 2, headR * 2);
            }

            using (var line = new Pen(Color.FromArgb(255, Theme.Darken(color, 0.55f)), Math.Max(1.2f, size * 0.03f)))
            {
                // 耳朵 / 角
                switch (kind)
                {
                    case PetKind.Cat:
                        g.FillPolygon(new SolidBrush(Theme.Darken(color, 0.15f)), new[]
                        {
                            new PointF(cx - headR * 0.86f, cy - headR * 0.5f),
                            new PointF(cx - headR * 0.55f, cy - headR * 1.5f),
                            new PointF(cx - headR * 0.12f, cy - headR * 0.86f)
                        });
                        g.FillPolygon(new SolidBrush(Theme.Darken(color, 0.15f)), new[]
                        {
                            new PointF(cx + headR * 0.86f, cy - headR * 0.5f),
                            new PointF(cx + headR * 0.55f, cy - headR * 1.5f),
                            new PointF(cx + headR * 0.12f, cy - headR * 0.86f)
                        });
                        break;

                    case PetKind.Dragon:
                        g.FillPolygon(new SolidBrush(Theme.Lighten(color, 0.5f)), new[]
                        {
                            new PointF(cx - headR * 0.75f, cy - headR * 0.7f),
                            new PointF(cx - headR * 0.35f, cy - headR * 1.65f),
                            new PointF(cx - headR * 0.05f, cy - headR * 0.85f)
                        });
                        g.FillPolygon(new SolidBrush(Theme.Lighten(color, 0.5f)), new[]
                        {
                            new PointF(cx + headR * 0.75f, cy - headR * 0.7f),
                            new PointF(cx + headR * 0.35f, cy - headR * 1.65f),
                            new PointF(cx + headR * 0.05f, cy - headR * 0.85f)
                        });
                        break;

                    default:
                        g.FillEllipse(new SolidBrush(Theme.Darken(color, 0.3f)),
                            cx - headR * 1.15f, cy - headR * 0.55f, headR * 0.6f, headR * 1.1f);
                        g.FillEllipse(new SolidBrush(Theme.Darken(color, 0.3f)),
                            cx + headR * 0.55f, cy - headR * 0.55f, headR * 0.6f, headR * 1.1f);
                        break;
                }

                // 眼睛
                using (var eye = new SolidBrush(Color.FromArgb(24, 28, 38)))
                {
                    g.FillEllipse(eye, cx - headR * 0.52f, cy - headR * 0.25f, headR * 0.22f, headR * 0.22f);
                    g.FillEllipse(eye, cx + headR * 0.30f, cy - headR * 0.25f, headR * 0.22f, headR * 0.22f);
                }

                // 嘴 / 鼻
                g.DrawArc(line,
                    cx - headR * 0.34f, cy + headR * 0.06f,
                    headR * 0.68f, headR * 0.5f, 10f, 160f);

                if (kind == PetKind.Cat)
                {
                    g.DrawLine(line, cx - headR * 1.1f, cy + headR * 0.2f, cx - headR * 0.55f, cy + headR * 0.32f);
                    g.DrawLine(line, cx + headR * 1.1f, cy + headR * 0.2f, cx + headR * 0.55f, cy + headR * 0.32f);
                }
            }
        }
    }
}
