using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class HallForm : Form
    {
        private readonly Label moneyLabel;
        private readonly Label progressLabel;
        private readonly Label petLabel;
        private readonly Label adminHint;

        public HallForm()
        {
            Text = "摸物品 · 开箱寻宝";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(900, 720);
            MinimumSize = new Size(820, 660);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new LinearGradientBrush(ClientRectangle,
                           Color.FromArgb(14, 20, 33), Color.FromArgb(7, 10, 17), 90f))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
                using (var pen = new Pen(Color.FromArgb(16, 255, 255, 255)))
                {
                    for (int x = 0; x < Width; x += 46)
                        g.DrawLine(pen, x, 0, x, Height);
                    for (int y = 0; y < Height; y += 46)
                        g.DrawLine(pen, 0, y, Width, y);
                }
            };

            var title = new Label
            {
                Text = "摸物品 · 开箱寻宝",
                Font = new Font(Theme.FontTitle.FontFamily, 27f, FontStyle.Bold),
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(72, 78)
            };

            var subtitle = new Label
            {
                Text = "v1.1  ·  两张地图、宠物蛋、管理员模式、自动搜索与自动出售",
                Font = Theme.FontBody,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(76, 132)
            };

            var infoCard = new Panel
            {
                Location = new Point(72, 182),
                Size = new Size(756, 116),
                BackColor = Color.FromArgb(22, 29, 45)
            };
            infoCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 200), 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, infoCard.Width - 1, infoCard.Height - 1);
                }
            };

            moneyLabel = new Label
            {
                Text = "资金  ¥0",
                Font = new Font(Theme.FontBody.FontFamily, 13f, FontStyle.Bold),
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 20)
            };

            progressLabel = new Label
            {
                Text = "",
                Font = Theme.FontBody,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 56)
            };

            petLabel = new Label
            {
                Text = "",
                Font = Theme.FontBody,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 82)
            };

            adminHint = new Label
            {
                Text = "管理员模式已开启",
                Font = Theme.FontSmall,
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(600, 24)
            };

            infoCard.Controls.Add(adminHint);
            infoCard.Controls.Add(petLabel);
            infoCard.Controls.Add(progressLabel);
            infoCard.Controls.Add(moneyLabel);

            var startButton = CreateButton("开始游戏", new Point(72, 326), new Size(756, 70), true);
            startButton.Font = new Font(Theme.FontTitle.FontFamily, 15f, FontStyle.Bold);
            startButton.Click += (s, e) => AppHost.StartGame(this);

            var changelogButton = CreateButton("更新日志", new Point(72, 410), new Size(366, 54), false);
            changelogButton.Click += (s, e) =>
            {
                using (var form = new ChangelogForm())
                    form.ShowDialog(this);
            };

            var settingsButton = CreateButton("设置", new Point(462, 410), new Size(366, 54), false);
            settingsButton.Click += (s, e) => OpenSettings();

            var exitButton = CreateButton("退出游戏", new Point(72, 478), new Size(756, 50), false);
            exitButton.ForeColor = Theme.SubText;
            exitButton.Click += (s, e) => Close();

            var footer = new Label
            {
                Text = "存档在程序同目录 save.json，跟着 exe 一起拷走就能带走进度",
                Font = Theme.FontSmall,
                ForeColor = Theme.WithAlpha(Theme.SubText, 170),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(74, 566)
            };

            Controls.Add(footer);
            Controls.Add(exitButton);
            Controls.Add(settingsButton);
            Controls.Add(changelogButton);
            Controls.Add(startButton);
            Controls.Add(infoCard);
            Controls.Add(subtitle);
            Controls.Add(title);

            LoadSummary();
        }

        private Button CreateButton(string text, Point location, Size size, bool primary)
        {
            var color = primary ? Theme.Accent : Color.FromArgb(26, 34, 52);
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? Theme.Darken(Theme.Accent, 0.72f) : color,
                ForeColor = primary ? Theme.Lighten(Theme.Accent, 0.5f) : Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = primary ? Theme.Accent : Theme.WithAlpha(Theme.Line, 210);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = primary
                ? Theme.Darken(Theme.Accent, 0.58f)
                : Color.FromArgb(36, 46, 68);
            return button;
        }

        private void OpenSettings()
        {
            var engine = new GameEngine(AppHost.Player);
            using (var form = new SettingsForm(engine))
            {
                form.ShowDialog(this);
            }
            LoadSummary();
        }

        public void LoadSummary()
        {
            var player = AppHost.Player;
            moneyLabel.Text = "资金  ¥" + player.Money.ToString("N0");

            var maps = MapDefinition.All.Where(m => player.UnlockedMaps.Contains(m.Id)).Select(m => m.Name).ToList();
            progressLabel.Text = $"已解锁地图：{string.Join(" / ", maps)}    已搜 {player.TotalFound} 件 · 已售 {player.TotalSold} 件";

            string petText = player.Pets.Count == 0
                ? "宠物：还没有，进游戏花 ¥10,000 孵一颗蛋"
                : $"宠物：共 {player.Pets.Count} 只，出战 {player.EquippedPetIds.Count}/3";
            petLabel.Text = petText;

            adminHint.Visible = player.AdminMode;

            var currentMap = MapDefinition.ById(player.CurrentMapId);
            Text = $"摸物品 · 开箱寻宝 —— {currentMap.Name}";
        }
    }
}
