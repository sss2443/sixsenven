using System;
using System.Drawing;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class AdminForm : Form
    {
        private readonly GameEngine engine;
        private readonly Label moneyLabel;
        private readonly ComboBox shapeBox;
        private readonly ComboBox rarityBox;
        private readonly NumericUpDown valueBox;
        private readonly Label resultLabel;
        private readonly Label bagLabel;

        public AdminForm(GameEngine engine)
        {
            this.engine = engine;

            Text = "管理员模式";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(820, 600);
            MinimumSize = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label
            {
                Text = "管理员模式",
                Font = new Font(Theme.FontTitle.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Theme.Accent,
                AutoSize = true,
                Location = new Point(34, 24)
            };

            moneyLabel = new Label
            {
                Text = "",
                Font = new Font(Theme.FontBody.FontFamily, 13f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(34, 64)
            };

            var giveButton = new Button
            {
                Text = "给我 ¥999,999",
                Location = new Point(34, 104),
                Size = new Size(230, 52),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Darken(Theme.Accent, 0.74f),
                ForeColor = Theme.Lighten(Theme.Accent, 0.5f),
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            giveButton.FlatAppearance.BorderColor = Theme.Accent;
            giveButton.Click += (s, e) =>
            {
                engine.GiveAdminMoney(999999);
                AppHost.Save();
                RefreshAll();
                resultLabel.ForeColor = Theme.Accent;
                resultLabel.Text = "已发放 ¥999,999。";
            };

            var divider = new Label
            {
                Text = "生成任意物品",
                Font = new Font(Theme.FontBody.FontFamily, 12f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(34, 186)
            };

            var shapeCaption = new Label
            {
                Text = "造型",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(36, 224)
            };

            shapeBox = new ComboBox
            {
                Location = new Point(36, 246),
                Size = new Size(220, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(24, 32, 50),
                ForeColor = Theme.Text,
                FlatStyle = FlatStyle.Flat
            };
            foreach (var shape in (ItemShape[])Enum.GetValues(typeof(ItemShape)))
                shapeBox.Items.Add(NameBook.ShapeName(shape));
            shapeBox.SelectedIndex = 0;

            var rarityCaption = new Label
            {
                Text = "品质",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(282, 224)
            };

            rarityBox = new ComboBox
            {
                Location = new Point(282, 246),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(24, 32, 50),
                ForeColor = Theme.Text,
                FlatStyle = FlatStyle.Flat
            };
            foreach (var rarity in (Rarity[])Enum.GetValues(typeof(Rarity)))
                rarityBox.Items.Add(NameBook.RarityName(rarity));
            rarityBox.SelectedIndex = 3;

            var valueCaption = new Label
            {
                Text = "价值",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(494, 224)
            };

            valueBox = new NumericUpDown
            {
                Location = new Point(494, 246),
                Size = new Size(170, 30),
                Maximum = 10000000,
                Minimum = 0,
                Increment = 100,
                Value = 5000,
                ThousandsSeparator = true,
                BackColor = Color.FromArgb(24, 32, 50),
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };

            var createButton = new Button
            {
                Text = "生成并放入背包",
                Location = new Point(36, 302),
                Size = new Size(628, 52),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 38, 58),
                ForeColor = Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            createButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Accent, 160);
            createButton.Click += (s, e) =>
            {
                var shape = (ItemShape)shapeBox.SelectedIndex;
                var rarity = (Rarity)rarityBox.SelectedIndex;
                int value = (int)valueBox.Value;
                var item = engine.AdminCreateItem(shape, rarity, value);
                AppHost.Save();
                RefreshAll();
                resultLabel.ForeColor = Theme.RarityColor(rarity);
                resultLabel.Text = $"已生成「{item.Name}」价值 ¥{item.Value:N0}，已放进背包。";
            };

            resultLabel = new Label
            {
                Text = "生成的物品默认传说品质可自选，出售规则和正常物品一样。",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                Size = new Size(740, 56),
                Location = new Point(38, 386)
            };

            bagLabel = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                AutoSize = true,
                Location = new Point(38, 458)
            };

            var closeButton = new Button
            {
                Text = "关闭",
                Location = new Point(628, 510),
                Size = new Size(166, 46),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 34, 52),
                ForeColor = Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            closeButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Line, 210);
            closeButton.Click += (s, e) => Close();

            Controls.Add(closeButton);
            Controls.Add(bagLabel);
            Controls.Add(resultLabel);
            Controls.Add(createButton);
            Controls.Add(valueBox);
            Controls.Add(valueCaption);
            Controls.Add(rarityBox);
            Controls.Add(rarityCaption);
            Controls.Add(shapeBox);
            Controls.Add(shapeCaption);
            Controls.Add(divider);
            Controls.Add(giveButton);
            Controls.Add(moneyLabel);
            Controls.Add(title);

            RefreshAll();
        }

        private void RefreshAll()
        {
            moneyLabel.Text = "当前资金 ¥" + engine.Player.Money.ToString("N0");
            bagLabel.Text = $"背包 {engine.Player.Bag.Count} 件 · 待售 ¥{engine.Player.BagValue:N0}";
        }
    }
}
