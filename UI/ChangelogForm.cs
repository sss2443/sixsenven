using System;
using System.Drawing;
using System.Windows.Forms;

namespace LootSearch.UI
{
    public sealed class ChangelogForm : Form
    {
        public ChangelogForm()
        {
            Text = "更新日志";
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(760, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label
            {
                Text = "更新日志",
                Font = new Font(Theme.FontTitle.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(34, 24)
            };

            var box = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(34, 78),
                Size = new Size(692, 460),
                BackColor = Color.FromArgb(18, 24, 38),
                ForeColor = Theme.Text,
                Font = Theme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                Text = BuildText()
            };

            var closeButton = new Button
            {
                Text = "关闭",
                Location = new Point(34, 550),
                Size = new Size(160, 44),
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
            Controls.Add(box);
            Controls.Add(title);
        }

        private static string BuildText()
        {
            return
@"v1.1  ·  2026-11-24
  · 新增地图系统：零号大坝（默认）/ 长弓西谷（解锁 ¥90,000）
    长弓西谷整体出货概率提升：稀有、史诗、传说权重上调，物品单价 ×1.15
  · 新增宠物系统：¥10,000 一颗宠物蛋
      狗 65%   价值 +5%   搜索速度 +5%
      猫 30%   价值 +12%  搜索速度 +12%
      龙 5%    价值 +30%  搜索速度 +25%
    同一只宠物可以重复孵化，最多同时出战 3 只，加成叠加
  · 新增管理员模式（设置里开启）
      · 一键发放 ¥999,999
      · 自选造型 / 品质 / 价值，生成任意物品放进背包
  · 新增开始页面：开始游戏 / 更新日志 / 设置 / 退出
  · 新增设置项
      · 自动搜索：点一下阴影直接搜出，不用按住
      · 自动出售：搜到立刻按价值卖成现金
  · 存档结构升级：地图解锁、宠物列表与出战状态、设置项一并保存

v1.0  ·  2026-11-23
  · 初版：阴影棋盘、放大镜光圈搜索、五档箱子（灰 / 绿 / 蓝 / 紫 / 红）
  · 背包与出售、道具稀有度、自动存档与手动存档
";
        }
    }
}
