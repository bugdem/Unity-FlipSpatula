using ClocknestGames.Library.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ClocknestGames.Game.Core
{
    public class UpgradeButton : MonoBehaviour, EventListener<UpgradeEvent>
                                                ,EventListener<PickableItemEvent>
    {
        public UpgradeType UpgradeType;
        public ItemType CostItemType = ItemType.Gold;
        public uint CostForStart = 150;
        public uint CostIncreaseOnEachUpgrade = 200;
        public float AbilityIncreaseOnEachUpgrade = .1f;
        public int MaxLevel = 100;

        public Button Button;
        public TMPro.TextMeshProUGUI CostText;
        public TMPro.TextMeshProUGUI LevelText;

        private Tweener _buttonTweener;

        private void Start()
        {
            CheckUpgradable();
        }

        public void OnButtonClicked()
        {
            var nextLevel = ProgressManager.Instance.GetPlayerUpgrade(UpgradeType) + 1;
            var nextLevelCost = CostForStart + (nextLevel - 1) * CostIncreaseOnEachUpgrade;
            LevelManager.Instance.GainUpgrade(new UpgradeEvent(UpgradeType, CostItemType, nextLevelCost));

            _buttonTweener?.Complete();
            _buttonTweener = Button.GetComponent<RectTransform>().DOPunchScale(Vector3.one * .1f, .2f);
        }

        public void CheckUpgradable()
        {
            var currentLevel = ProgressManager.Instance.GetPlayerUpgrade(UpgradeType);
            var nextLevelCost = CostForStart + currentLevel * CostIncreaseOnEachUpgrade;
            var playerItemQuantity = ProgressManager.Instance.GetItemQuantity(CostItemType); // + LevelManager.Instance.GetPickedItemCountThisLevel(CostItemType);

            // If player dont have enough currency, disable button
            if (currentLevel >= MaxLevel || playerItemQuantity < nextLevelCost)
            {
                Button.interactable = false;
                CostText.color = new Color(109f/255f, 109f/255f, 109f/255f);
            }
            else
            {
                Button.interactable = true;
                CostText.color = Color.white;
            }

            CostText.SetText(nextLevelCost > 99999 ? Mathf.FloorToInt(nextLevelCost / 1000f).ToString() + "K" : nextLevelCost.ToString());
            if (currentLevel >= MaxLevel)
                LevelText.SetText($"LEVEL MAX");
            else
                LevelText.SetText($"LEVEL {currentLevel}");
        }

        public float GetAbilityIncrease()
        {
            var currentUpgradeLevel = ProgressManager.Instance.GetPlayerUpgrade(UpgradeType);
            return (currentUpgradeLevel - 1) * AbilityIncreaseOnEachUpgrade;
        }

        private void OnEnable()
        {
            this.EventStartListening<UpgradeEvent>();
            this.EventStartListening<PickableItemEvent>();
        }

        private void OnDisable()
        {
            this.EventStopListening<UpgradeEvent>();
            this.EventStopListening<PickableItemEvent>();
        }

        public void OnCGEvent(UpgradeEvent currentEvent)
        {
            CheckUpgradable();
        }

        public void OnCGEvent(PickableItemEvent currentEvent)
        {
            CheckUpgradable();
        }
    }
}