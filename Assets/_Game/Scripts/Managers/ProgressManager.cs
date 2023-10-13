using UnityEngine;
using System.Collections.Generic;
using ClocknestGames.Library.Utils;
using ClocknestGames.Library.Editor;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public class LevelProgress
    {
        [StringInList(typeof(StringInListData), "AllSceneNames")] public string LevelName;
        public int LevelId;
    }

    public class ProgressManager : PersistentSingleton<ProgressManager>, EventListener<LevelEvent>
                                                                        , EventListener<UpgradeEvent>
                                                                        , EventListener<PickableItemEvent>
    {
        public List<LevelProgress> Levels;

        private const string _playerDataPrefKey = "PlayerData";
        private PlayerData _playerData;

        private const int _randomSeed = 17384;

        protected override void Awake()
        {
            base.Awake();
            if (!base._enabled) return;

            _playerData = PlayerPrefs.HasKey(_playerDataPrefKey) ?
                                JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(_playerDataPrefKey))
                                : new PlayerData
                                {
                                    LevelName = Levels[0].LevelName,
                                    LevelId = Levels[0].LevelId,
                                    LevelIndex = 1,
                                    Items = new List<PlayerItem>(),
                                    Upgrades = new List<PlayerUpgrade>(),
                                    HapticEnabled = HapticManager.Instance.IsEnabled
                                };

            // _playerData.Items.Add(new PlayerItem { Type = ItemType.Gold, Quantity = 100000 });

            //_playerData.LevelIndex = 9;
            //_playerData.LevelName = "level_009";
            //_playerData.LevelId = 9;

            //SavePlayerData();
        }

        private void SavePlayerData()
        {
            PlayerPrefs.SetString(_playerDataPrefKey, JsonUtility.ToJson(_playerData));
        }

        public bool GetHapticEnabled()
        {
            return _playerData.HapticEnabled;
        }

        public void SetHapticStatus(bool status)
        {
            _playerData.HapticEnabled = status;

            SavePlayerData();
        }

        public uint GetItemQuantity(ItemType itemType)
        {
            foreach (var item in _playerData.Items)
            {
                if (item.Type == itemType)
                    return item.Quantity;
            }

            return 0;
        }

        public uint GetPlayerUpgrade(UpgradeType upgradeType)
        {
            foreach (var upgrade in _playerData.Upgrades)
            {
                if (upgrade.Type == upgradeType)
                    return upgrade.CurrentLevel;
            }

            return 1;
        }

        public void AddItems(ItemType itemType, uint quantity, bool save)
        {
            bool itemExists = false;
            foreach (var item in _playerData.Items)
            {
                if (item.Type == itemType)
                {
                    item.Quantity += quantity;
                    itemExists = true;

                    break;
                }
            }

            if (!itemExists)
            {
                _playerData.Items.Add(new PlayerItem
                {
                    Type = itemType,
                    Quantity = quantity
                });
            }

            if (save)
                SavePlayerData();
        }

        public void AddItems(Dictionary<ItemType, uint> gainedItems)
        {
            if (gainedItems == null)
                return;

            foreach (var gainedItem in gainedItems)
            {
                AddItems(gainedItem.Key, gainedItem.Value, false);
            }

            SavePlayerData();
        }

        public void GainUpgrade(ItemType itemType, uint itemQuantity, UpgradeType upgradeType)
        {
            bool upgradeExists = false;
            foreach (var upgrade in _playerData.Upgrades)
            {
                if (upgrade.Type == upgradeType)
                {
                    upgrade.CurrentLevel++;
                    upgradeExists = true;

                    break;
                }
            }

            if (!upgradeExists)
            {
                _playerData.Upgrades.Add(new PlayerUpgrade { Type = upgradeType, CurrentLevel = 2 });
            }

            foreach (var item in _playerData.Items)
            {
                if (item.Type == itemType)
                {
                    item.Quantity = (item.Quantity - itemQuantity).ClampMin((uint)0);
                    break;
                }
            }

            SavePlayerData();
        }

        public int GetCurrentLevelIndex()
        {
            return _playerData.LevelIndex;
        }

        public string GetCurrentLevel()
        {
            return _playerData.LevelName;
        }

        public string GetCurrentLevelName()
        {
            return GetCurrentLevelIndex().ToString();
        }

        public string GetLevelToLoad()
        {
            return GetCurrentLevel();
        }

        public void SetSceneIndexToPrevious()
        {
            SetLevel(_playerData.LevelIndex - 1);
        }

        private void SetLevel(int levelIndex)
        {
            var nextLevel = GetLevel(levelIndex);
            _playerData.LevelIndex = levelIndex;
            _playerData.LevelName = nextLevel.LevelName;
            _playerData.LevelId = nextLevel.LevelId;

            SavePlayerData();
        }

        private LevelProgress GetLevel(int levelIndex, bool randomize = false)
        {
            if (!randomize)
                return Levels[(levelIndex - 1) % Levels.Count];

            if (levelIndex <= Levels.Count)
                return Levels[levelIndex - 1];

            // Randomize levels
            var levelPool = new List<LevelProgress>(Levels);
            int levelCount = levelPool.Count;

            int baseSeed = _randomSeed;
            int seedAddition = Mathf.FloorToInt(levelIndex / (float)levelCount) - 1;
            int seed = baseSeed + seedAddition;
            var rnd = new System.Random(seed);
            int randomMod = levelIndex % levelCount;
            for (int index = 0; index < randomMod; index++)
            {
                var randomIndex = rnd.Next(0, levelPool.Count);
                levelPool.RemoveAt(randomIndex);
            }

            var newLevelIndex = rnd.Next(0, levelPool.Count);
            return levelPool[newLevelIndex];
        }

        private void OnEnable()
        {
            this.EventStartListening<LevelEvent>();
            this.EventStartListening<UpgradeEvent>();
            this.EventStartListening<PickableItemEvent>();
        }

        private void OnDisable()
        {
            this.EventStopListening<LevelEvent>();
            this.EventStopListening<UpgradeEvent>();
            this.EventStopListening<PickableItemEvent>();
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            /*
            if (currentEvent.EventType == LevelEventType.Completed || currentEvent.EventType == LevelEventType.Failed)
            {
                AddItems(currentEvent.ItemsGained);
            }
            */

            if (currentEvent.EventType == LevelEventType.Completed)
            {
                SetLevel(_playerData.LevelIndex + 1);
            }
        }

        public void OnCGEvent(UpgradeEvent currentEvent)
        {
            GainUpgrade(currentEvent.CostItemType, currentEvent.Cost, currentEvent.Type);
        }

        public void OnCGEvent(PickableItemEvent currentEvent)
        {
            AddItems(currentEvent.PickedItem.Type, currentEvent.PickedItem.Quantity, true);
        }
    }
}