using System;
using LootSearch.Core;

namespace LootSearch.UI
{
    internal static class AppHost
    {
        private static PlayerState player;

        public static HallForm HallForm { get; set; }
        public static MainForm GameForm { get; set; }

        public static PlayerState Player
        {
            get
            {
                if (player == null)
                {
                    player = SaveSystem.Load(SaveSystem.DefaultPath) ?? new PlayerState();
                }
                return player;
            }
        }

        public static bool Save()
        {
            return SaveSystem.Save(Player, SaveSystem.DefaultPath);
        }

        public static void StartGame(HallForm from)
        {
            from.Hide();
            if (GameForm == null || GameForm.IsDisposed)
                GameForm = new MainForm(Player);
            GameForm.RefreshAll();
            GameForm.Show();
            GameForm.Activate();
        }

        public static void BackToHall(MainForm from)
        {
            Save();
            from.Hide();
            if (HallForm == null || HallForm.IsDisposed)
                HallForm = new HallForm();
            HallForm.LoadSummary();
            HallForm.Show();
            HallForm.Activate();
        }
    }
}
