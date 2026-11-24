using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using LootSearch.Core;

namespace LootSearch.UI
{
    public sealed class MainForm : Form
    {
        private readonly GameEngine engine;

        private readonly BoardPanel board;
        private readonly Label moneyValue;
        private readonly Label statsLabel;
        private readonly Label statusLabel;
        private readonly Label bagTitle;
        private readonly Label bagValue;
        private readonly Label mapInfoLabel;
        private readonly ListBox bagList;
        private readonly Button sellSelectedButton;
        private readonly Button sellAllButton;
        private readonly Button petsButton;
        private readonly Button adminButton;
        private readonly List<Button> mapButtons = new List<Button>();
        private readonly List<Item> shownItems = new List<Item>();

        public MainForm(PlayerState player)
        {
            engine = new GameEngine(player);

            Text = "摸物品 · 开箱寻宝";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1320, 880);
            MinimumSize = new Size(1180, 820);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;

            // ---------- 顶部栏 ----------
            var header = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = Color.FromArgb(21, 28, 44) };
            header.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Accent, 90), 1f))
                {
                    e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            var title = new Label
            {
                Text = "摸物品 · 开箱寻宝",
                Font = Theme.FontTitle,
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(28, 20)
            };

            var subtitle = new Label
            {
                Text = "阴影就是物品 · 按住左键用放大镜搜索 · 搜到的可以卖钱",
                Font = Theme.FontSmall,
                ForeColor = Theme.WithAlpha(Theme.SubText, 210),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(30, 64)
            };

            var moneyCaption = new Label
            {
                Text = "资金",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(430, 16)
            };

            moneyValue = new Label
            {
                Text = "¥0",
                Font = Theme.FontMoney,
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(426, 34)
            };

            statsLabel = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(432, 78)
            };

            var headerButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 660,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 34, 26, 0),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            var saveButton = CreateGhostButton("保存进度", 104);
            saveButton.Click += (s, e) => SaveNow("已保存进度");

            adminButton = CreateGhostButton("管理员", 92);
            adminButton.ForeColor = Theme.Accent;
            adminButton.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Accent, 190);
            adminButton.Click += (s, e) => OpenAdmin();

            petsButton = CreateGhostButton("宠物", 86);
            petsButton.Click += (s, e) => OpenPets();

            var settingsButton = CreateGhostButton("设置", 86);
            settingsButton.Click += (s, e) => OpenSettings();

            var hallButton = CreateGhostButton("◀ 大厅", 100);
            hallButton.Click += (s, e) => AppHost.BackToHall(this);

            headerButtons.Controls.Add(saveButton);
            headerButtons.Controls.Add(adminButton);
            headerButtons.Controls.Add(petsButton);
            headerButtons.Controls.Add(settingsButton);
            headerButtons.Controls.Add(hallButton);

            header.Controls.Add(headerButtons);
            header.Controls.Add(statsLabel);
            header.Controls.Add(moneyValue);
            header.Controls.Add(moneyCaption);
            header.Controls.Add(subtitle);
            header.Controls.Add(title);

            // ---------- 地图栏 ----------
            var mapBar = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.FromArgb(15, 21, 34) };
            mapBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 130), 1f))
                {
                    e.Graphics.DrawLine(pen, 0, mapBar.Height - 1, mapBar.Width, mapBar.Height - 1);
                }
            };

            var mapCaption = new Label
            {
                Text = "地图",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(26, 24)
            };

            int mapX = 84;
            foreach (var map in MapDefinition.All)
            {
                var mapButton = new Button
                {
                    Text = map.Name,
                    Location = new Point(mapX, 13),
                    Size = new Size(206, 40),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(24, 32, 50),
                    ForeColor = Theme.Text,
                    Font = Theme.FontBody,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                var captured = map;
                mapButton.Click += (s, e) => OnMapClick(captured);
                mapBar.Controls.Add(mapButton);
                mapButtons.Add(mapButton);
                mapX += 218;
            }

            mapInfoLabel = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(mapX + 6, 23)
            };

            mapBar.Controls.Add(mapInfoLabel);
            mapBar.Controls.Add(mapCaption);

            // ---------- 箱子选择栏 ----------
            var tierBar = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Color.FromArgb(13, 18, 29) };
            tierBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 160), 1f))
                {
                    e.Graphics.DrawLine(pen, 0, tierBar.Height - 1, tierBar.Width, tierBar.Height - 1);
                }
            };

            var tierFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 12),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            foreach (var tier in BoxTierDefinition.All)
            {
                var accent = Color.FromArgb(tier.AccentArgb);
                var button = new Button
                {
                    Text = $"{tier.Name}  {tier.FeeText}",
                    Size = new Size(172, 54),
                    Margin = new Padding(0, 0, 14, 0),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.Darken(accent, 0.74f),
                    ForeColor = Theme.Lighten(accent, 0.55f),
                    Font = Theme.FontButton,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                button.FlatAppearance.BorderColor = accent;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.MouseOverBackColor = Theme.Darken(accent, 0.58f);
                button.FlatAppearance.MouseDownBackColor = Theme.Darken(accent, 0.46f);
                var capturedTier = tier;
                button.Click += (s, e) => OpenTier(capturedTier);
                tierFlow.Controls.Add(button);
            }

            tierBar.Controls.Add(tierFlow);

            // ---------- 右侧背包 ----------
            var right = new Panel { Dock = DockStyle.Right, Width = 344, BackColor = Theme.Panel };
            right.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 200), 1f))
                {
                    e.Graphics.DrawLine(pen, 0, 0, 0, right.Height);
                }
            };

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Panel
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 148));

            var bagHeader = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel };
            bagTitle = new Label
            {
                Text = "背包 · 0 件",
                Font = new Font(Theme.FontBody.FontFamily, 11.5f, FontStyle.Bold),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 20)
            };
            bagValue = new Label
            {
                Text = "待售 ¥0",
                Font = Theme.FontBody,
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, 52)
            };
            bagHeader.Controls.Add(bagTitle);
            bagHeader.Controls.Add(bagValue);

            var bagHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 0, 18, 6), BackColor = Theme.Panel };
            bagList = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 58,
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text,
                Font = Theme.FontBag,
                IntegralHeight = false
            };
            bagList.DrawItem += BagList_DrawItem;
            bagList.DoubleClick += (s, e) => SellSelected();
            bagHost.Controls.Add(bagList);

            var bagFooter = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel };
            sellSelectedButton = CreateActionButton("出售选中", new Point(20, 18), new Size(146, 46));
            sellSelectedButton.Click += (s, e) => SellSelected();

            sellAllButton = CreateActionButton("全部出售", new Point(174, 18), new Size(146, 46));
            sellAllButton.Click += (s, e) => SellAll();

            var sellHint = new Label
            {
                Text = "双击背包里的物品也能直接卖掉",
                Font = Theme.FontSmall,
                ForeColor = Theme.SubText,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 78)
            };

            bagFooter.Controls.Add(sellHint);
            bagFooter.Controls.Add(sellSelectedButton);
            bagFooter.Controls.Add(sellAllButton);

            rightLayout.Controls.Add(bagHeader, 0, 0);
            rightLayout.Controls.Add(bagHost, 0, 1);
            rightLayout.Controls.Add(bagFooter, 0, 2);
            right.Controls.Add(rightLayout);

            // ---------- 底部状态栏 ----------
            statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(26, 0, 16, 0),
                ForeColor = Theme.SubText,
                BackColor = Color.FromArgb(11, 15, 24),
                Font = Theme.FontSmall,
                Text = "欢迎回来。"
            };

            // ---------- 棋盘 ----------
            board = new BoardPanel(engine);
            board.Dock = DockStyle.Fill;
            board.ItemFound += OnItemFound;
            board.BoardCleared += OnBoardCleared;

            Controls.Add(board);
            Controls.Add(statusLabel);
            Controls.Add(right);
            Controls.Add(tierBar);
            Controls.Add(mapBar);
            Controls.Add(header);

            RefreshAll();
            SetStatus(engine.Board == null
                ? $"当前地图：{engine.CurrentMap.Name}。选一个箱子开始。"
                : $"上次的「{engine.CurrentMap.Name} · {BoxTierDefinition.ById(engine.Board.TierId).Name}」还没搜完，继续吧。");

            FormClosing += (s, e) => SaveQuietly();
        }

        private Button CreateGhostButton(string text, int width)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, 40),
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 34, 52),
                ForeColor = Theme.Text,
                Font = Theme.FontBody,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Line, 220);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 45, 68);
            return button;
        }

        private Button CreateActionButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 38, 58),
                ForeColor = Theme.Text,
                Font = Theme.FontButton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Accent, 150);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 50, 74);
            return button;
        }

        public void RefreshAll()
        {
            RefreshMoney();
            RefreshBag();
            RefreshMapBar();
            RefreshHeaderButtons();
            board.Invalidate();
        }

        private void RefreshHeaderButtons()
        {
            adminButton.Visible = engine.Player.AdminMode;
            petsButton.Text = $"宠物 {engine.Player.EquippedPetIds.Count}/3";
        }

        private void RefreshMapBar()
        {
            var maps = MapDefinition.All;
            for (int i = 0; i < mapButtons.Count && i < maps.Count; i++)
            {
                var map = maps[i];
                var button = mapButtons[i];
                bool unlocked = engine.IsMapUnlocked(map);
                bool current = string.Equals(engine.Player.CurrentMapId, map.Id, StringComparison.OrdinalIgnoreCase);
                var accent = Color.FromArgb(map.AccentArgb);

                if (!unlocked)
                {
                    button.Text = $"{map.Name} · 解锁 ¥{map.UnlockFee:N0}";
                    button.ForeColor = Theme.SubText;
                    button.BackColor = Color.FromArgb(20, 26, 40);
                    button.FlatAppearance.BorderColor = Theme.WithAlpha(Theme.Line, 180);
                }
                else if (current)
                {
                    button.Text = $"✔ {map.Name}";
                    button.ForeColor = Theme.Lighten(accent, 0.5f);
                    button.BackColor = Theme.Darken(accent, 0.72f);
                    button.FlatAppearance.BorderColor = accent;
                }
                else
                {
                    button.Text = map.Name;
                    button.ForeColor = Theme.Text;
                    button.BackColor = Color.FromArgb(24, 32, 50);
                    button.FlatAppearance.BorderColor = Theme.WithAlpha(accent, 150);
                }
            }

            string bonus = "无";
            var equipped = engine.EquippedPets;
            if (equipped.Count > 0)
            {
                bonus = $"价值 +{equipped.Sum(p => p.ValueBonusPercent)}% · 速度 +{equipped.Sum(p => p.SpeedBonusPercent)}%";
            }
            mapInfoLabel.Text = $"{engine.CurrentMap.Description} · 宠物加成 {bonus}";
        }

        private void OnMapClick(MapDefinition map)
        {
            if (map == null) return;
            bool unlocked = engine.IsMapUnlocked(map);

            if (!unlocked)
            {
                var result = MessageBox.Show(this,
                    $"解锁「{map.Name}」需要 ¥{map.UnlockFee:N0}，当前资金 ¥{engine.Player.Money:N0}。\n\n{map.Description}\n\n确认解锁吗？",
                    "解锁地图", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.OK) return;

                if (!engine.UnlockMap(map, out string unlockError))
                {
                    SetStatus(unlockError);
                    return;
                }

                engine.SwitchMap(map, out _);
                RefreshAll();
                SaveQuietly();
                SetStatus($"已解锁并切换到「{map.Name}」，这张图出货概率整体提升。");
                return;
            }

            if (string.Equals(engine.Player.CurrentMapId, map.Id, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus($"已经在「{map.Name}」了。");
                return;
            }

            var current = engine.Board;
            if (current != null)
            {
                int remaining = current.ItemTotalCount - current.FoundCount;
                if (remaining > 0)
                {
                    var confirm = MessageBox.Show(this,
                        $"当前箱子还有 {remaining} 件没搜完，换地图会放弃它们。\n\n确认切到「{map.Name}」吗？",
                        "确认换地图", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (confirm != DialogResult.OK) return;
                }
            }

            if (!engine.SwitchMap(map, out string error))
            {
                SetStatus(error);
                return;
            }

            RefreshAll();
            SaveQuietly();
            SetStatus($"已切换到「{map.Name}」。");
        }

        private void OpenTier(BoxTierDefinition tier)
        {
            if (tier == null) return;

            var current = engine.Board;
            if (current != null)
            {
                int remaining = current.ItemTotalCount - current.FoundCount;
                if (remaining > 0)
                {
                    var result = MessageBox.Show(this,
                        $"当前箱子还有 {remaining} 件物品没搜完，换箱会放弃它们。\n\n确认开「{engine.CurrentMap.Name} · {tier.Name}」（入场费 {tier.FeeText}）吗？",
                        "确认换箱", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.OK) return;
                }
            }

            if (!engine.OpenBox(tier, out string error))
            {
                SetStatus(error);
                try { System.Media.SystemSounds.Exclamation.Play(); } catch { }
                return;
            }

            board.ResetInteraction();
            RefreshAll();
            SaveQuietly();
            SetStatus($"已开「{engine.CurrentMap.Name} · {tier.Name}」。按住左键在阴影物品上，放大镜转满一圈就搜到了。");
        }

        private void OnItemFound(Item item)
        {
            bool autoSold = false;
            if (engine.Player.AutoSell)
            {
                autoSold = engine.Sell(item.Id) > 0;
            }

            RefreshMoney();
            RefreshBag();
            SaveQuietly();
            SetStatus(autoSold
                ? $"自动出售「{item.Name}」 +¥{item.Value:N0}"
                : $"搜到「{item.Name}」  价值 ¥{item.Value:N0}");
        }

        private void OnBoardCleared()
        {
            SaveQuietly();
            var boardState = engine.Board;
            SetStatus($"本箱全部搜完：{boardState.ItemTotalCount} 件，总价值 ¥{boardState.RevealedValue:N0}。");
        }

        private void SellSelected()
        {
            if (bagList.SelectedIndex < 0 || bagList.SelectedIndex >= shownItems.Count)
            {
                SetStatus("先在背包里选中一件物品。");
                return;
            }

            var item = shownItems[bagList.SelectedIndex];
            int gained = engine.Sell(item.Id);
            if (gained > 0)
            {
                RefreshMoney();
                RefreshBag();
                SaveQuietly();
                SetStatus($"卖出「{item.Name}」 +¥{gained:N0}");
            }
        }

        private void SellAll()
        {
            if (engine.Player.Bag.Count == 0)
            {
                SetStatus("背包是空的，先去搜点东西。");
                return;
            }

            int count = engine.Player.Bag.Count;
            int total = engine.SellAll();
            RefreshMoney();
            RefreshBag();
            SaveQuietly();
            SetStatus($"一键出售 {count} 件，收入 ¥{total:N0}");
        }

        private void OpenSettings()
        {
            using (var form = new SettingsForm(engine))
            {
                form.ShowDialog(this);
            }
            RefreshAll();
            SaveQuietly();
        }

        private void OpenPets()
        {
            using (var form = new PetsForm(engine))
            {
                form.ShowDialog(this);
            }
            RefreshAll();
            SaveQuietly();
        }

        private void OpenAdmin()
        {
            using (var form = new AdminForm(engine))
            {
                form.ShowDialog(this);
            }
            RefreshAll();
            SaveQuietly();
        }

        private void RefreshMoney()
        {
            var player = engine.Player;
            moneyValue.Text = "¥" + player.Money.ToString("N0");
            statsLabel.Text = $"已搜 {player.TotalFound} 件 · 已售 {player.TotalSold} 件 · 支出 ¥{player.TotalSpent:N0} · 收入 ¥{player.TotalEarned:N0}";
        }

        private void RefreshBag()
        {
            var player = engine.Player;
            shownItems.Clear();
            shownItems.AddRange(player.Bag.OrderByDescending(i => i.Value));

            bagList.BeginUpdate();
            bagList.Items.Clear();
            foreach (var item in shownItems)
                bagList.Items.Add(item.Name);
            bagList.EndUpdate();

            bagTitle.Text = $"背包 · {shownItems.Count} 件";
            bagValue.Text = $"待售 ¥{player.BagValue:N0}";
            sellSelectedButton.Enabled = shownItems.Count > 0;
            sellAllButton.Enabled = shownItems.Count > 0;
        }

        private void SetStatus(string text)
        {
            statusLabel.Text = text;
        }

        private void SaveQuietly()
        {
            AppHost.Save();
        }

        private void SaveNow(string message)
        {
            bool ok = AppHost.Save();
            SetStatus(ok ? $"{message}（{SaveSystem.DefaultPath}）" : "保存失败，请检查文件夹写入权限。");
        }

        private void BagList_DrawItem(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var brush = new SolidBrush(selected ? Color.FromArgb(34, 46, 70) : Theme.Panel))
            {
                g.FillRectangle(brush, rect);
            }

            if (e.Index < 0 || e.Index >= shownItems.Count)
            {
                return;
            }

            var item = shownItems[e.Index];
            var rarityColor = Theme.RarityColor(item.Rarity);

            var icon = new RectangleF(rect.X + 10, rect.Y + 12, 34, 34);
            ItemArt.DrawItem(g, icon, item.Shape, rarityColor);

            using (var nameBrush = new SolidBrush(Theme.Text))
            using (var subBrush = new SolidBrush(Theme.WithAlpha(rarityColor, 210)))
            using (var valueBrush = new SolidBrush(Theme.Accent))
            {
                g.DrawString(item.Name, Theme.FontBag, nameBrush, rect.X + 54, rect.Y + 10);
                string sub = NameBook.RarityName(item.Rarity) + "品质";
                if (item.FromAdmin) sub += " · 管理员生成";
                g.DrawString(sub, Theme.FontSmall, subBrush, rect.X + 54, rect.Y + 31);

                string valueText = "+¥" + item.Value.ToString("N0");
                var size = g.MeasureString(valueText, Theme.FontBag);
                g.DrawString(valueText, Theme.FontBag, valueBrush, rect.Right - size.Width - 12, rect.Y + 18);
            }

            using (var pen = new Pen(Theme.WithAlpha(Theme.Line, 120), 1f))
            {
                g.DrawLine(pen, rect.X + 8, rect.Bottom - 1, rect.Right - 8, rect.Bottom - 1);
            }
        }
    }
}
