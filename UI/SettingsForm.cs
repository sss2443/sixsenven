using System;
using System.Drawing;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class SettingsForm : Form
    {
        private readonly GameEngine engine;
        private readonly CheckBox autoSearchBox;
        private readonly CheckBox autoSellBox;
        private readonly CheckBox adminBox;
        private readonly Label hintLabel;

        public SettingsForm(GameEngine engine)
        {
            this.engine = engine;

            Text = "设置";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 580);
            MinimumSize = new Size(700, 540);
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label
            {
                Text = "设置",
                Font = new Font(Theme.FontTitle.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(34, 26)
            };

            var subtitle = new Label
            {
                Text = "改动会立刻写进存档，不需要重启",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(36, 64)
            };

            autoSearchBox = CreateToggle("自动搜索", "点一下阴影就直接搜出来，不用再按住不放", new Point(36, 112));
            autoSearchBox.CheckedChanged += (s, e) =>
            {
                engine.Player.AutoSearch = autoSearchBox.Checked;
                AppHost.Save();
            };

            autoSellBox = CreateToggle("自动出售", "搜到的物品立刻按价值卖掉，不留在背包里", new Point(36, 202));
            autoSellBox.CheckedChanged += (s, e) =>
            {
                engine.Player.AutoSell = autoSellBox.Checked;
                AppHost.Save();
            };

            adminBox = CreateToggle("管理员模式", "开启后游戏内出现「管理员」按钮：给 ¥999,999、生成任意物品", new Point(36, 292));
            adminBox.CheckedChanged += (s, e) =>
            {
                engine.Player.AdminMode = adminBox.Checked;
                AppHost.Save();
            };

            hintLabel = new Label
            {
                Text = "提示：自动搜索 + 自动出售 就是纯挂机刷钱模式。",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(38, 396)
            };

            var resetButton = new Button
            {
                Text = "清空存档并重开",
                Location = new Point(38, 486),
                Size = new Size(184, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 30, 38),
                ForeColor = Theme.Danger,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            resetButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Danger, 170);
            resetButton.Click += (s, e) => ResetProgress();

            var closeButton = new Button
            {
                Text = "完成",
                Location = new Point(556, 486),
                Size = new Size(166, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Darken(Theme.Accent, 0.72f),
                ForeColor = Theme.Lighten(Theme.Accent, 0.5f),
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            closeButton.FlatAppearance.BorderColor = Theme.Accent;
            closeButton.Click += (s, e) => Close();

            autoSearchBox.Checked = engine.Player.AutoSearch;
            autoSellBox.Checked = engine.Player.AutoSell;
            adminBox.Checked = engine.Player.AdminMode;

            Controls.Add(hintLabel);
            Controls.Add(closeButton);
            Controls.Add(resetButton);
            Controls.Add(adminBox);
            Controls.Add(autoSellBox);
            Controls.Add(autoSearchBox);
            Controls.Add(subtitle);
            Controls.Add(title);
        }

        private CheckBox CreateToggle(string text, string description, Point location)
        {
            var box = new CheckBox
            {
                Text = text,
                Location = location,
                Size = new Size(660, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Theme.Text,
                Font = new Font(Theme.FontBody.FontFamily, 11.5f, FontStyle.Bold),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var desc = new Label
            {
                Text = description,
                Location = new Point(location.X + 26, location.Y + 34),
                Size = new Size(660, 26),
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent
            };

            Controls.Add(desc);
            return box;
        }

        private void ResetProgress()
        {
            var result = MessageBox.Show(this,
                "确定要清空存档重新开始吗？资金回到 ¥1,200，地图、宠物、背包全部清掉（设置项保留）。",
                "清空存档", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.OK) return;

            engine.NewGame();
            AppHost.Save();
            Close();
        }
    }
}
