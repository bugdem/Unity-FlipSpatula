using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClocknestGames.Library.Utils;
using ClocknestGames.Library.Control;

namespace ClocknestGames.Game.Core
{
    public enum LevelEventType : byte
    {
        Opened,
        Started,
        PreCompleted,   // Player starts dancing or blow confetti
        PreFailed,
        Completed,
        Failed
    }

    public struct LevelEvent
    {
        public LevelEventType EventType;
        public Dictionary<ItemType, uint> ItemsGained;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelEvent"/> struct.
        /// </summary>
        /// <param name="eventType">Type of level event.</param>
        /// <param name="itemsGained">Items gained in current level.</param>
        public LevelEvent(LevelEventType eventType, Dictionary<ItemType, uint> itemsGained = null)
        {
            EventType = eventType;
            ItemsGained = itemsGained;
        }
    }

    /// <summary>
    /// An event typically fired when picking an item, letting listeners know what item has been picked
    /// </summary>
    public struct PickableItemEvent
    {
        public Item PickedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickableItemEvent"/> struct.
        /// </summary>
        /// <param name="pickedItem">Picked item.</param>
        public PickableItemEvent(Item pickedItem)
        {
            PickedItem = pickedItem;
        }
    }

    /// <summary>
    /// An event typically fired when upgrading a skill, letting listeners know what upgrade has been gained
    /// </summary>
    public struct UpgradeEvent
    {
        public UpgradeType Type;
        public ItemType CostItemType;
        public uint Cost;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeEvent"/> struct.
        /// </summary>
        /// <param name="type">Upgrade that is gained.</param>
        /// <param name="costItemType">Used item for upgrade.</param>
        /// <param name="cost">Cost of upgrade.</param>
        public UpgradeEvent(UpgradeType type, ItemType costItemType, uint cost)
        {
            Type = type;
            CostItemType = costItemType;
            Cost = cost;
        }
    }

    [System.Serializable]
    public class Item
    {
        public ItemType Type = ItemType.Gold;
        public uint Quantity = 1;
    }

    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Configs")]
        public float CompletedScreenDelay = 2f;
        public float FailedScreenDelay = 2f;

        [Header("Level")]
        public Transform LevelContainer;

        public bool IsLevelCompleted { get; private set; }
        public bool IsLevelSuccess { get; private set; }
        public bool CanGainItems { get; private set; } = true;
        public bool CanGainUpgrades { get; private set; } = true;
        public bool IsLevelActive { get; private set; } = false;


        private Dictionary<ItemType, uint> _pickedItemsThisLevel = new Dictionary<ItemType, uint>();

        private void Start()
        {
            EventManager.TriggerEvent(new LevelEvent(LevelEventType.Opened));
        }

        #region Scene Methods
        public void StartLevel()
        {
            IsLevelActive = true;

            EventManager.TriggerEvent(new LevelEvent(LevelEventType.Started));
        }

        public void RestartLevel(float delay)
        {
            LoadLevel(LevelLoadManager.Instance.GetCurrentSceneName(), delay);
        }

        public void LoadNextLevel(float delay)
        {
            LoadLevel(ProgressManager.Instance.GetLevelToLoad(), delay);
        }

        public void LevelFailed()
        {
            if (IsLevelCompleted)
                return;

            IsLevelSuccess = false;
            IsLevelCompleted = true;

            Debug.Log("Level Failed!");
            EventManager.TriggerEvent(new LevelEvent(LevelEventType.PreFailed));

            StartCoroutine(IShowLevelLoseScreen());
        }

        public void LevelCompleted()
        {
            if (IsLevelCompleted)
                return;

            IsLevelSuccess = true;
            IsLevelCompleted = true;

            Debug.Log("Level Completed!");
            EventManager.TriggerEvent(new LevelEvent(LevelEventType.PreCompleted));

            StartCoroutine(IShowLevelWinScreen());
        }

        private IEnumerator IShowLevelWinScreen()
        {
            yield return new WaitForSecondsRealtime(CompletedScreenDelay);

            CanGainItems = false;

            EventManager.TriggerEvent(new LevelEvent(LevelEventType.Completed, _pickedItemsThisLevel));
        }

        private IEnumerator IShowLevelLoseScreen()
        {
            yield return new WaitForSecondsRealtime(FailedScreenDelay);

            CanGainItems = false;

            EventManager.TriggerEvent(new LevelEvent(LevelEventType.Failed, _pickedItemsThisLevel));
        }

        private void LoadLevel(string levelName, float loadDelay)
        {
            LevelLoadManager.Instance.LoadLevel(levelName, loadDelay);
        }

        public void PickItem(PickableItemEvent pickableItemEvent)
        {
            if (!CanGainItems)
                return;

            if (_pickedItemsThisLevel.ContainsKey(pickableItemEvent.PickedItem.Type))
                _pickedItemsThisLevel[pickableItemEvent.PickedItem.Type] += pickableItemEvent.PickedItem.Quantity;
            else
                _pickedItemsThisLevel.Add(pickableItemEvent.PickedItem.Type, pickableItemEvent.PickedItem.Quantity);

            EventManager.TriggerEvent(pickableItemEvent);
        }

        public void GainUpgrade(UpgradeEvent upgradeEvent)
        {
            if (!CanGainUpgrades)
                return;

            EventManager.TriggerEvent(upgradeEvent);
        }

        public void SpendItem(ItemType itemType, uint quantity)
        {
            if (!_pickedItemsThisLevel.ContainsKey(itemType))
                return;

            var itemQuantity = _pickedItemsThisLevel[itemType];
            itemQuantity = (itemQuantity - quantity).ClampMin((uint)0);
            _pickedItemsThisLevel[itemType] = itemQuantity;
        }

        public uint GetPickedItemCountThisLevel(ItemType itemType)
        {
            if (_pickedItemsThisLevel.ContainsKey(itemType))
                return _pickedItemsThisLevel[itemType];

            return 0;
        }
        #endregion
    }
}