using System;
using System.Drawing;
using System.Linq;
using LootSearch.Core;

namespace LootSearch.UI
{
    internal static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(9, 13, 21);
        public static readonly Color Panel = Color.FromArgb(17, 23, 36);
        public static readonly Color PanelSoft = Color.FromArgb(22, 29, 45);
        public static readonly Color Tile = Color.FromArgb(27, 36, 56);
        public static readonly Color TileRevealed = Color.FromArgb(35, 45, 70);
        public static readonly Color Line = Color.FromArgb(44, 58, 86);
        public static readonly Color Text = Color.FromArgb(233, 239, 248);
        public static readonly Color SubText = Color.FromArgb(139, 152, 176);
        public static readonly Color Accent = Color.FromArgb(255, 199, 87);
        public static readonly Color Accent2 = Color.FromArgb(66, 224, 190);
        public static readonly Color Danger = Color.FromArgb(255, 104, 104);
        public static readonly Color Shadow = Color.FromArgb(6, 9, 16);

        public static readonly Font FontTitle;
        public static readonly Font FontMoney;
        public static readonly Font FontBody;
        public static readonly Font FontSmall;
        public static readonly Font FontButton;
        public static readonly Font FontBag;

        static Theme()
        {
            string family = ResolveFamily("Microsoft YaHei UI", "Microsoft YaHei", "微软雅黑", "Segoe UI");
            FontTitle = new Font(family, 17f, FontStyle.Bold, GraphicsUnit.Point);
            FontMoney = new Font(family, 20f, FontStyle.Bold, GraphicsUnit.Point);
            FontBody = new Font(family, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            FontSmall = new Font(family, 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            FontButton = new Font(family, 10.5f, FontStyle.Bold, GraphicsUnit.Point);
            FontBag = new Font(family, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private static string ResolveFamily(params string[] candidates)
        {
            try
            {
                var families = FontFamily.Families.Select(f => f.Name).ToList();
                foreach (var name in candidates)
                {
                    if (families.Any(f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase)))
                        return name;
                }
            }
            catch
            {
                // ignore and fall through
            }
            return FontFamily.GenericSansSerif.Name;
        }

        public static Color RarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare: return Color.FromArgb(96, 176, 255);
                case Rarity.Epic: return Color.FromArgb(182, 128, 255);
                case Rarity.Legendary: return Color.FromArgb(255, 198, 77);
                default: return Color.FromArgb(205, 214, 228);
            }
        }

        public static Color Lighten(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                (int)(color.R + (255 - color.R) * amount),
                (int)(color.G + (255 - color.G) * amount),
                (int)(color.B + (255 - color.B) * amount));
        }

        public static Color Darken(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1 - amount)),
                (int)(color.G * (1 - amount)),
                (int)(color.B * (1 - amount)));
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), color);
        }

        public static GraphicsPathHelper RoundedRect(RectangleF rect, float radius)
        {
            return new GraphicsPathHelper(rect, radius);
        }
    }

    internal sealed class GraphicsPathHelper : IDisposable
    {
        public GraphicsPathHelper(RectangleF rect, float radius)
        {
            Path = new System.Drawing.Drawing2D.GraphicsPath();
            float d = Math.Max(1f, Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height)));
            Path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            Path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            Path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            Path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            Path.CloseFigure();
        }

        public System.Drawing.Drawing2D.GraphicsPath Path { get; }

        public void Dispose()
        {
            Path.Dispose();
        }
    }
}
