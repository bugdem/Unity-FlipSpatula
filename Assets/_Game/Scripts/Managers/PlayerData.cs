using System;
using System.Collections.Generic;

namespace ClocknestGames.Game.Core
{
    [Serializable]
    public enum ItemType : byte
    {
        Gold = 1,
        Key = 2,
        Fuel = 3,
        Scaler = 4
    }

    [Serializable]
    public enum UpgradeType : byte
    {
        Fuel = 0,
        Scale = 1,
        Speed = 2
    }

    [Serializable]
    public class PlayerData
    {
        public int LevelId;
        public int LevelIndex;
        public string LevelName;
        public List<PlayerItem> Items;
        public List<PlayerUpgrade> Upgrades;
        public bool HapticEnabled;
    }

    [Serializable]
    public class PlayerItem
    {
        public ItemType Type;
        public uint Quantity;
    }

    [Serializable]
    public class PlayerUpgrade
    {
        public UpgradeType Type;
        public uint CurrentLevel = 1;
    }
}