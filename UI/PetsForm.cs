using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class PetsForm : Form
    {
        private readonly GameEngine engine;
        private readonly ListBox petList;
        private readonly List<Pet> shownPets = new List<Pet>();
        private readonly Label moneyLabel;
        private readonly Label equipLabel;
        private readonly Label resultLabel;
        private readonly Button buyButton;
        private readonly Button toggleButton;

        public PetsForm(GameEngine engine)
        {
            this.engine = engine;

            Text = "宠物";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1040, 680);
            MinimumSize = new Size(980, 640);
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label
            {
                Text = "宠物",
                Font = new Font(Theme.FontTitle.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(34, 24)
            };

            moneyLabel = new Label
            {
                Text = "",
                Font = new Font(Theme.FontBody.FontFamily, 13f, FontStyle.Bold),
                ForeColor = Theme.Accent,
                AutoSize = true,
                Location = new Point(34, 62)
            };

            // ---------- 左侧：孵化 ----------
            var eggPanel = new Panel
            {
                Location = new Point(34, 108),
                Size = new Size(378, 470),
                BackColor = Color.FromArgb(22, 29, 45)
            };
            eggPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 190), 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, eggPanel.Width - 1, eggPanel.Height - 1);
                }
                using (var brush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                {
                    e.Graphics.FillEllipse(brush, 132, 42, 114, 150);
                }
                using (var pen = new Pen(Theme.WithAlpha(Theme.Accent, 120), 2f))
                {
                    e.Graphics.DrawEllipse(pen, 132, 42, 114, 150);
                }
            };

            var eggCaption = new Label
            {
                Text = "宠物蛋",
                Font = new Font(Theme.FontBody.FontFamily, 12f, FontStyle.Bold),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 212)
            };

            var dropInfo = new Label
            {
                Text = "狗 65%（价值+5% 速度+5%）\n猫 30%（价值+12% 速度+12%）\n龙 5%（价值+30% 速度+25%）",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 246)
            };

            buyButton = new Button
            {
                Text = $"买一颗蛋  ¥{PetCatalog.EggPrice:N0}",
                Location = new Point(24, 342),
                Size = new Size(330, 56),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Darken(Theme.Accent, 0.74f),
                ForeColor = Theme.Lighten(Theme.Accent, 0.5f),
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            buyButton.FlatAppearance.BorderColor = Theme.Accent;
            buyButton.Click += (s, e) => BuyEgg();

            resultLabel = new Label
            {
                Text = "同一个宠物可以重复出，重复的也能一起出战。",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                Size = new Size(336, 60),
                Location = new Point(24, 410)
            };

            eggPanel.Controls.Add(resultLabel);
            eggPanel.Controls.Add(buyButton);
            eggPanel.Controls.Add(dropInfo);
            eggPanel.Controls.Add(eggCaption);

            // ---------- 右侧：列表 ----------
            equipLabel = new Label
            {
                Text = "",
                Font = new Font(Theme.FontBody.FontFamily, 12f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(436, 112)
            };

            var listHost = new Panel
            {
                Location = new Point(432, 148),
                Size = new Size(574, 430),
                BackColor = Color.FromArgb(18, 24, 38)
            };
            listHost.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 190), 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, listHost.Width - 1, listHost.Height - 1);
                }
            };

            petList = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 76,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(18, 24, 38),
                ForeColor = Theme.Text,
                Font = Theme.FontBag,
                IntegralHeight = false
            };
            petList.DrawItem += PetList_DrawItem;
            petList.DoubleClick += (s, e) => ToggleSelected();
            listHost.Controls.Add(petList);

            toggleButton = new Button
            {
                Text = "出战 / 休息",
                Location = new Point(786, 594),
                Size = new Size(220, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 38, 58),
                ForeColor = Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            toggleButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Accent, 160);
            toggleButton.Click += (s, e) => ToggleSelected();

            var tip = new Label
            {
                Text = $"双击列表里的宠物也能切换出战状态；最多同时出战 3 只。孵化次数 {engine.Player.EggsHatched}",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(436, 604)
            };

            var closeButton = new Button
            {
                Text = "关闭",
                Location = new Point(34, 594),
                Size = new Size(150, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 34, 52),
                ForeColor = Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            closeButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Line, 210);
            closeButton.Click += (s, e) => Close();

            Controls.Add(tip);
            Controls.Add(closeButton);
            Controls.Add(toggleButton);
            Controls.Add(listHost);
            Controls.Add(equipLabel);
            Controls.Add(eggPanel);
            Controls.Add(moneyLabel);
            Controls.Add(title);

            RefreshAll();
        }

        private void BuyEgg()
        {
            var pet = engine.BuyEgg(out string error);
            if (pet == null)
            {
                resultLabel.ForeColor = Theme.Danger;
                resultLabel.Text = error;
                return;
            }

            var color = PetArt.KindColor(pet.Kind);
            resultLabel.ForeColor = color;
            resultLabel.Text = $"孵出 {pet.Name}！\n价值 +{pet.ValueBonusPercent}% · 速度 +{pet.SpeedBonusPercent}%";

            AppHost.Save();
            RefreshAll();

            int index = shownPets.FindIndex(p => p.Id == pet.Id);
            if (index >= 0) petList.SelectedIndex = index;

            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
        }

        private void ToggleSelected()
        {
            if (petList.SelectedIndex < 0 || petList.SelectedIndex >= shownPets.Count)
            {
                resultLabel.ForeColor = Theme.SubText;
                resultLabel.Text = "先在右侧选一只宠物。";
                return;
            }

            var pet = shownPets[petList.SelectedIndex];
            if (!engine.ToggleEquip(pet.Id, out string error))
            {
                resultLabel.ForeColor = Theme.Danger;
                resultLabel.Text = error;
                return;
            }

            AppHost.Save();
            RefreshAll();

            int index = shownPets.FindIndex(p => p.Id == pet.Id);
            if (index >= 0) petList.SelectedIndex = index;
            resultLabel.ForeColor = Theme.SubText;
            resultLabel.Text = engine.Player.EquippedPetIds.Contains(pet.Id)
                ? $"{pet.Name} 已出战。"
                : $"{pet.Name} 回去休息了。";
        }

        private void RefreshAll()
        {
            moneyLabel.Text = "资金 ¥" + engine.Player.Money.ToString("N0");

            shownPets.Clear();
            shownPets.AddRange(engine.Player.Pets.OrderByDescending(p => p.Kind));

            petList.BeginUpdate();
            petList.Items.Clear();
            foreach (var pet in shownPets)
                petList.Items.Add(pet.Name);
            petList.EndUpdate();

            equipLabel.Text = $"我的宠物（{shownPets.Count} 只） · 出战 {engine.Player.EquippedPetIds.Count}/3";

            buyButton.Enabled = engine.Player.Money >= PetCatalog.EggPrice;

            var equipped = engine.EquippedPets;
            if (equipped.Count > 0)
            {
                equipLabel.Text += $" · 加成 价值+{equipped.Sum(p => p.ValueBonusPercent)}% 速度+{equipped.Sum(p => p.SpeedBonusPercent)}%";
            }
        }

        private void PetList_DrawItem(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var brush = new SolidBrush(selected ? Color.FromArgb(34, 46, 70) : Color.FromArgb(18, 24, 38)))
            {
                g.FillRectangle(brush, rect);
            }

            if (e.Index < 0 || e.Index >= shownPets.Count) return;

            var pet = shownPets[e.Index];
            bool equipped = engine.Player.EquippedPetIds.Contains(pet.Id);
            var color = PetArt.KindColor(pet.Kind);

            var icon = new RectangleF(rect.X + 12, rect.Y + 12, 50, 50);
            PetArt.Draw(g, icon, pet.Kind, equipped);

            using (var nameBrush = new SolidBrush(Theme.Text))
            using (var subBrush = new SolidBrush(Theme.WithAlpha(color, 220)))
            using (var tagBrush = new SolidBrush(equipped ? Theme.Accent : Theme.SubText))
            {
                g.DrawString(pet.Name, Theme.FontBag, nameBrush, rect.X + 76, rect.Y + 14);
                g.DrawString($"{PetCatalog.KindName(pet.Kind)} · {PetCatalog.RarityTag(pet.Kind)}", Theme.FontSmall, subBrush, rect.X + 76, rect.Y + 36);
                g.DrawString($"价值 +{pet.ValueBonusPercent}%  ·  速度 +{pet.SpeedBonusPercent}%", Theme.FontSmall, subBrush, rect.X + 76, rect.Y + 52);

                string tag = equipped ? "出战中" : "待命";
                var size = g.MeasureString(tag, Theme.FontSmall);
                g.DrawString(tag, Theme.FontSmall, tagBrush, rect.Right - size.Width - 16, rect.Y + 28);
            }

            using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 110), 1f))
            {
                g.DrawLine(pen, rect.X + 10, rect.Bottom - 1, rect.Right - 10, rect.Bottom - 1);
            }
        }
    }
}
