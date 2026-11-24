using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LootSearch.Core
{
    public enum ItemShape
    {
        Gem,
        Coin,
        Ring,
        Key,
        Bottle,
        Star,
        Book,
        Crown,
        Gear,
        Fish,
        Scroll
    }

    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    public enum PetKind
    {
        Dog = 0,
        Cat = 1,
        Dragon = 2
    }

    public sealed class Item
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public ItemShape Shape { get; set; }
        public Rarity Rarity { get; set; }
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public bool FromAdmin { get; set; }
    }

    public sealed class Pet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public PetKind Kind { get; set; }
        public string Name { get; set; } = "";
        public int Serial { get; set; }
        public int ValueBonusPercent { get; set; }
        public int SpeedBonusPercent { get; set; }
    }

    public static class NameBook
    {
        public static string ShapeName(ItemShape shape)
        {
            switch (shape)
            {
                case ItemShape.Gem: return "宝石";
                case ItemShape.Coin: return "古币";
                case ItemShape.Ring: return "戒指";
                case ItemShape.Key: return "铜钥匙";
                case ItemShape.Bottle: return "药瓶";
                case ItemShape.Star: return "星辰";
                case ItemShape.Book: return "古书";
                case ItemShape.Crown: return "王冠";
                case ItemShape.Gear: return "齿轮";
                case ItemShape.Fish: return "鱼骨";
                case ItemShape.Scroll: return "卷轴";
                default: return "物品";
            }
        }

        public static string RarityName(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare: return "精良";
                case Rarity.Epic: return "史诗";
                case Rarity.Legendary: return "传说";
                default: return "普通";
            }
        }

        public static string PetName(PetKind kind, int serial)
        {
            string baseName;
            switch (kind)
            {
                case PetKind.Dragon: baseName = "小龙"; break;
                case PetKind.Cat: baseName = "小猫"; break;
                default: baseName = "小狗"; break;
            }
            return baseName + " #" + serial;
        }
    }

    public sealed class MapDefinition
    {
        public string Id { get; init; } = "zero";
        public string Name { get; init; } = "零号大坝";
        public string Description { get; init; } = "";
        public int UnlockFee { get; init; }
        public double ValueFactor { get; init; } = 1.0;
        public double[] RarityFactors { get; init; } = new[] { 1.0, 1.0, 1.0, 1.0 };
        public int AccentArgb { get; init; }

        public static IReadOnlyList<MapDefinition> All { get; } = new List<MapDefinition>
        {
            new MapDefinition
            {
                Id = "zero", Name = "零号大坝", Description = "基础地图 · 出货概率普通",
                UnlockFee = 0, ValueFactor = 1.0,
                RarityFactors = new[] { 1.0, 1.0, 1.0, 1.0 },
                AccentArgb = unchecked((int)0xFF6FA8FF)
            },
            new MapDefinition
            {
                Id = "longbow", Name = "长弓西谷", Description = "高级地图 · 整体出货概率提升",
                UnlockFee = 90000, ValueFactor = 1.15,
                RarityFactors = new[] { 0.75, 1.40, 1.75, 2.10 },
                AccentArgb = unchecked((int)0xFFBF8CFF)
            }
        };

        public static MapDefinition ById(string id)
        {
            foreach (var map in All)
            {
                if (string.Equals(map.Id, id, StringComparison.OrdinalIgnoreCase))
                    return map;
            }
            return All[0];
        }
    }

    public sealed class BoxTierDefinition
    {
        public string Id { get; init; } = "gray";
        public string Name { get; init; } = "灰箱";
        public int Fee { get; init; }
        public int Columns { get; init; } = 6;
        public int Rows { get; init; } = 4;
        public int ItemCount { get; init; }
        public int ValueMin { get; init; }
        public int ValueMax { get; init; }
        public double SearchSeconds { get; init; } = 1.0;
        public int AccentArgb { get; init; }
        public int[] RarityWeights { get; init; } = new[] { 70, 25, 5, 0 };

        [JsonIgnore]
        public int CellCount => Columns * Rows;

        public static IReadOnlyList<BoxTierDefinition> All { get; } = new List<BoxTierDefinition>
        {
            new BoxTierDefinition
            {
                Id = "gray", Name = "灰箱", Fee = 0,
                ItemCount = 14, ValueMin = 25, ValueMax = 110,
                SearchSeconds = 0.9, AccentArgb = unchecked((int)0xFF8A93A3),
                RarityWeights = new[] { 70, 25, 5, 0 }
            },
            new BoxTierDefinition
            {
                Id = "green", Name = "绿箱", Fee = 1000,
                ItemCount = 13, ValueMin = 45, ValueMax = 120,
                SearchSeconds = 1.1, AccentArgb = unchecked((int)0xFF35C26B),
                RarityWeights = new[] { 55, 32, 12, 1 }
            },
            new BoxTierDefinition
            {
                Id = "blue", Name = "蓝箱", Fee = 5000,
                ItemCount = 11, ValueMin = 280, ValueMax = 620,
                SearchSeconds = 1.4, AccentArgb = unchecked((int)0xFF3C8CFF),
                RarityWeights = new[] { 45, 33, 17, 5 }
            },
            new BoxTierDefinition
            {
                Id = "purple", Name = "紫箱", Fee = 10000,
                ItemCount = 10, ValueMin = 620, ValueMax = 1300,
                SearchSeconds = 1.7, AccentArgb = unchecked((int)0xFFA45BFF),
                RarityWeights = new[] { 35, 32, 23, 10 }
            },
            new BoxTierDefinition
            {
                Id = "red", Name = "红箱", Fee = 30000,
                ItemCount = 9, ValueMin = 1700, ValueMax = 3700,
                SearchSeconds = 2.1, AccentArgb = unchecked((int)0xFFFF4D4D),
                RarityWeights = new[] { 25, 30, 27, 18 }
            }
        };

        public static BoxTierDefinition ById(string id)
        {
            foreach (var tier in All)
            {
                if (string.Equals(tier.Id, id, StringComparison.OrdinalIgnoreCase))
                    return tier;
            }
            return All[0];
        }

        public string FeeText => Fee <= 0 ? "免费" : "¥" + Fee.ToString("N0");
    }

    public sealed class CellState
    {
        public int Index { get; set; }
        public Item Item { get; set; }
        public bool Revealed { get; set; }
    }

    public sealed class BoardState
    {
        public string TierId { get; set; } = "gray";
        public string MapId { get; set; } = "zero";
        public int Columns { get; set; } = 6;
        public int Rows { get; set; } = 4;
        public List<CellState> Cells { get; set; } = new List<CellState>();

        [JsonIgnore]
        public int ItemTotalCount => Cells.Count(c => c.Item != null);

        [JsonIgnore]
        public int FoundCount => Cells.Count(c => c.Item != null && c.Revealed);

        [JsonIgnore]
        public bool IsCleared => ItemTotalCount > 0 && FoundCount >= ItemTotalCount;

        [JsonIgnore]
        public int RevealedValue => Cells.Where(c => c.Item != null && c.Revealed).Sum(c => c.Item.Value);
    }

    public sealed class PlayerState
    {
        public int Money { get; set; } = 1200;
        public List<Item> Bag { get; set; } = new List<Item>();
        public int TotalFound { get; set; }
        public int TotalSold { get; set; }
        public int TotalSpent { get; set; }
        public long TotalEarned { get; set; }
        public string LastTierId { get; set; } = "gray";
        public BoardState SavedBoard { get; set; }
        public string SavedAtUtc { get; set; }

        // 地图
        public List<string> UnlockedMaps { get; set; } = new List<string> { "zero" };
        public string CurrentMapId { get; set; } = "zero";

        // 宠物
        public List<Pet> Pets { get; set; } = new List<Pet>();
        public List<string> EquippedPetIds { get; set; } = new List<string>();
        public int EggsHatched { get; set; }
        public int PetSerial { get; set; }

        // 设置
        public bool AutoSearch { get; set; }
        public bool AutoSell { get; set; }
        public bool AdminMode { get; set; }

        [JsonIgnore]
        public int BagValue => Bag.Sum(i => i.Value);
    }

    public static class BoardGenerator
    {
        public static BoardState Generate(BoxTierDefinition tier, MapDefinition map, double valueBonusFactor, Random rng)
        {
            if (map == null) map = MapDefinition.ById("zero");

            var board = new BoardState
            {
                TierId = tier.Id,
                MapId = map.Id,
                Columns = tier.Columns,
                Rows = tier.Rows
            };

            int cellCount = tier.Columns * tier.Rows;
            int itemCount = Math.Min(tier.ItemCount, cellCount - 2);
            var positions = new HashSet<int>();
            while (positions.Count < itemCount)
                positions.Add(rng.Next(cellCount));

            int[] weights = ScaleWeights(tier.RarityWeights, map.RarityFactors);

            for (int i = 0; i < cellCount; i++)
            {
                var cell = new CellState { Index = i };
                if (positions.Contains(i))
                {
                    var rarity = PickRarity(weights, rng);
                    var shape = (ItemShape)rng.Next(Enum.GetValues(typeof(ItemShape)).Length);
                    int raw = rng.Next(tier.ValueMin, tier.ValueMax + 1);
                    double valued = raw * RarityFactor(rarity) * map.ValueFactor * valueBonusFactor;
                    int value = (int)Math.Round(valued / 5.0, MidpointRounding.AwayFromZero) * 5;
                    if (value < 5) value = 5;
                    cell.Item = new Item
                    {
                        Shape = shape,
                        Rarity = rarity,
                        Name = NameBook.RarityName(rarity) + "·" + NameBook.ShapeName(shape),
                        Value = value
                    };
                }
                board.Cells.Add(cell);
            }

            return board;
        }

        private static int[] ScaleWeights(int[] baseWeights, double[] factors)
        {
            var result = new int[baseWeights.Length];
            for (int i = 0; i < baseWeights.Length; i++)
            {
                double f = i < factors.Length ? factors[i] : 1.0;
                result[i] = Math.Max(0, (int)Math.Round(baseWeights[i] * f));
            }
            if (result.Sum() <= 0) return baseWeights;
            return result;
        }

        private static Rarity PickRarity(int[] weights, Random rng)
        {
            int sum = weights.Sum();
            if (sum <= 0) return Rarity.Common;
            int roll = rng.Next(sum);
            int acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll < acc) return (Rarity)i;
            }
            return Rarity.Common;
        }

        private static double RarityFactor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Rare: return 1.25;
                case Rarity.Epic: return 1.70;
                case Rarity.Legendary: return 2.40;
                default: return 1.0;
            }
        }
    }

    public static class PetCatalog
    {
        public const int EggPrice = 10000;

        public static PetKind RollKind(Random rng)
        {
            int roll = rng.Next(100);
            if (roll < 65) return PetKind.Dog;
            if (roll < 95) return PetKind.Cat;
            return PetKind.Dragon;
        }

        public static string KindName(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Dragon: return "龙";
                case PetKind.Cat: return "猫";
                default: return "狗";
            }
        }

        public static string RarityTag(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Dragon: return "稀有 5%";
                case PetKind.Cat: return "中等 30%";
                default: return "常见 65%";
            }
        }

        public static int ValueBonus(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Dragon: return 30;
                case PetKind.Cat: return 12;
                default: return 5;
            }
        }

        public static int SpeedBonus(PetKind kind)
        {
            switch (kind)
            {
                case PetKind.Dragon: return 25;
                case PetKind.Cat: return 12;
                default: return 5;
            }
        }
    }

    public sealed class GameEngine
    {
        private readonly Random rng;

        public GameEngine(PlayerState player, int? seed = null)
        {
            Player = player ?? new PlayerState();
            rng = seed.HasValue ? new Random(seed.Value) : new Random();
            EnsureDefaults();
            Board = Player.SavedBoard;
        }

        public PlayerState Player { get; }
        public BoardState Board { get; private set; }
        public Random Rng => rng;

        public BoxTierDefinition CurrentTier => Board == null ? null : BoxTierDefinition.ById(Board.TierId);
        public MapDefinition CurrentMap => MapDefinition.ById(Player.CurrentMapId);

        public double PetValueFactor => 1.0 + EquippedPets.Sum(p => p.ValueBonusPercent) / 100.0;
        public int PetSpeedPercent => EquippedPets.Sum(p => p.SpeedBonusPercent);

        public List<Pet> EquippedPets => Player.Pets.Where(p => Player.EquippedPetIds.Contains(p.Id)).ToList();

        private void EnsureDefaults()
        {
            Player.Bag ??= new List<Item>();
            Player.Pets ??= new List<Pet>();
            Player.EquippedPetIds ??= new List<string>();
            Player.UnlockedMaps ??= new List<string>();
            if (Player.UnlockedMaps.Count == 0) Player.UnlockedMaps.Add("zero");
            if (!Player.UnlockedMaps.Contains("zero")) Player.UnlockedMaps.Add("zero");
            if (!Player.UnlockedMaps.Contains(Player.CurrentMapId)) Player.CurrentMapId = "zero";

            var valid = Player.Pets.Where(p => Player.EquippedPetIds.Contains(p.Id)).Select(p => p.Id).ToList();
            if (valid.Count != Player.EquippedPetIds.Count)
                Player.EquippedPetIds = valid;
            if (Player.EquippedPetIds.Count > 3)
                Player.EquippedPetIds = Player.EquippedPetIds.Take(3).ToList();
        }

        public bool IsMapUnlocked(MapDefinition map)
        {
            return map != null && Player.UnlockedMaps.Contains(map.Id);
        }

        public bool UnlockMap(MapDefinition map, out string error)
        {
            error = null;
            if (map == null) { error = "地图不存在"; return false; }
            if (IsMapUnlocked(map)) { error = "这张地图已经解锁了"; return false; }
            if (Player.Money < map.UnlockFee) { error = "资金不足，解锁需要 ¥" + map.UnlockFee.ToString("N0"); return false; }

            Player.Money -= map.UnlockFee;
            Player.TotalSpent += map.UnlockFee;
            Player.UnlockedMaps.Add(map.Id);
            return true;
        }

        public bool SwitchMap(MapDefinition map, out string error)
        {
            error = null;
            if (map == null) { error = "地图不存在"; return false; }
            if (!IsMapUnlocked(map)) { error = "这张地图还没解锁"; return false; }
            Player.CurrentMapId = map.Id;
            return true;
        }

        public bool OpenBox(BoxTierDefinition tier, out string error)
        {
            error = null;
            if (tier == null) { error = "箱子不存在"; return false; }
            if (Player.Money < tier.Fee) { error = "资金不足，先去搜免费灰箱攒钱吧"; return false; }

            Player.Money -= tier.Fee;
            Player.TotalSpent += tier.Fee;
            Player.LastTierId = tier.Id;
            Board = BoardGenerator.Generate(tier, CurrentMap, PetValueFactor, rng);
            Player.SavedBoard = Board;
            return true;
        }

        public Item RevealCell(int index)
        {
            if (Board == null || index < 0 || index >= Board.Cells.Count) return null;
            var cell = Board.Cells[index];
            if (cell.Item == null || cell.Revealed) return null;

            cell.Revealed = true;
            Player.Bag.Add(cell.Item);
            Player.TotalFound += 1;
            return cell.Item;
        }

        public int Sell(string itemId)
        {
            var item = Player.Bag.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return 0;
            Player.Bag.Remove(item);
            Player.Money += item.Value;
            Player.TotalEarned += item.Value;
            Player.TotalSold += 1;
            return item.Value;
        }

        public int SellAll()
        {
            int total = 0;
            foreach (var item in Player.Bag)
            {
                Player.Money += item.Value;
                Player.TotalEarned += item.Value;
                Player.TotalSold += 1;
                total += item.Value;
            }
            Player.Bag.Clear();
            return total;
        }

        public Pet BuyEgg(out string error)
        {
            error = null;
            if (Player.Money < PetCatalog.EggPrice)
            {
                error = "资金不足，宠物蛋要 ¥" + PetCatalog.EggPrice.ToString("N0");
                return null;
            }

            Player.Money -= PetCatalog.EggPrice;
            Player.TotalSpent += PetCatalog.EggPrice;
            Player.EggsHatched += 1;
            Player.PetSerial += 1;

            var kind = PetCatalog.RollKind(rng);
            var pet = new Pet
            {
                Kind = kind,
                Serial = Player.PetSerial,
                Name = NameBook.PetName(kind, Player.PetSerial),
                ValueBonusPercent = PetCatalog.ValueBonus(kind),
                SpeedBonusPercent = PetCatalog.SpeedBonus(kind)
            };
            Player.Pets.Add(pet);
            return pet;
        }

        public bool ToggleEquip(string petId, out string error)
        {
            error = null;
            var pet = Player.Pets.FirstOrDefault(p => p.Id == petId);
            if (pet == null) { error = "没有这只宠物"; return false; }

            if (Player.EquippedPetIds.Contains(petId))
            {
                Player.EquippedPetIds.Remove(petId);
                return true;
            }

            if (Player.EquippedPetIds.Count >= 3)
            {
                error = "最多同时带 3 只宠物，先取消一只";
                return false;
            }

            Player.EquippedPetIds.Add(petId);
            return true;
        }

        // 管理员
        public void GiveAdminMoney(int amount)
        {
            Player.Money += amount;
        }

        public Item AdminCreateItem(ItemShape shape, Rarity rarity, int value)
        {
            if (value < 0) value = 0;
            var item = new Item
            {
                Shape = shape,
                Rarity = rarity,
                Value = value,
                Name = NameBook.RarityName(rarity) + "·" + NameBook.ShapeName(shape),
                FromAdmin = true
            };
            Player.Bag.Add(item);
            return item;
        }

        public void NewGame(int startMoney = 1200)
        {
            Player.Money = startMoney;
            Player.Bag.Clear();
            Player.TotalFound = 0;
            Player.TotalSold = 0;
            Player.TotalSpent = 0;
            Player.TotalEarned = 0;
            Player.LastTierId = "gray";
            Player.SavedBoard = null;
            Player.UnlockedMaps = new List<string> { "zero" };
            Player.CurrentMapId = "zero";
            Player.Pets.Clear();
            Player.EquippedPetIds.Clear();
            Player.EggsHatched = 0;
            Player.PetSerial = 0;
            Board = null;
        }
    }

    public static class SaveSystem
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly string ResolvedPath = ResolveDefaultPath();

        public static string DefaultPath => ResolvedPath;

        private static string ResolveDefaultPath()
        {
            try
            {
                string dir = AppContext.BaseDirectory;
                string probe = Path.Combine(dir, ".loot_write_probe");
                File.WriteAllText(probe, "1");
                File.Delete(probe);
                return Path.Combine(dir, "save.json");
            }
            catch
            {
                try
                {
                    string fallback = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "LootSearch");
                    Directory.CreateDirectory(fallback);
                    return Path.Combine(fallback, "save.json");
                }
                catch
                {
                    return Path.Combine(Path.GetTempPath(), "loot_save.json");
                }
            }
        }

        public static PlayerState Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;
                var state = JsonSerializer.Deserialize<PlayerState>(json, Options);
                if (state == null) return null;
                state.Bag ??= new List<Item>();
                state.Pets ??= new List<Pet>();
                state.EquippedPetIds ??= new List<string>();
                state.UnlockedMaps ??= new List<string> { "zero" };
                if (state.SavedBoard != null)
                {
                    state.SavedBoard.Cells ??= new List<CellState>();
                    if (state.SavedBoard.Cells.Count == 0) state.SavedBoard = null;
                }
                return state;
            }
            catch
            {
                return null;
            }
        }

        public static bool Save(PlayerState state, string path)
        {
            try
            {
                if (state == null) return false;
                state.SavedAtUtc = DateTime.UtcNow.ToString("o");
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonSerializer.Serialize(state, Options));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
