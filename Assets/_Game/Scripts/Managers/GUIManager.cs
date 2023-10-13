using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using ClocknestGames.Library.Utils;
using ClocknestGames.Library.Control;
using ClocknestGames.Library.Editor;
using DG.Tweening;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public class TimingFeature
    {
        // [ColorUsage(true, true)]
        public Color Color;
        public float Speed;
        public byte Index { get; set; }
    }

    public class GUIManager : Singleton<GUIManager>, EventListener<PickableItemEvent>
                                                    ,EventListener<LevelEvent>
                                                    ,EventListener<TouchEvent>
                                                    ,EventListener<UpgradeEvent>
                                                    ,EventListener<TouchFirstTapEvent>
                                                    ,EventListener<SpatulaEvent>
    {
        [Header("General UI")]
        // public PostProcessVolume BlurPostProcessVolume;
        public RectTransform TouchToStart;
        public RectTransform HoldAndDragToStart;
        public CGTouchFollowingJoystick Joystick;
        public Sprite EmptyStar;
        public Sprite FilledStar;
        public TMPro.TextMeshProUGUI ScaleText;
        public TMPro.TextMeshProUGUI ScrapePointText;
        public float ScrapeTextDissappearTime = 2.5f;

        [Header("Progress- Text")]
        public RectTransform ProgressContainer;
        public TMPro.TextMeshProUGUI ProgressLevelText;
        public TMPro.TextMeshProUGUI ProgressCountText;
        public TMPro.TextMeshProUGUI LevelText;
        public Image ProgressSliderImage;

        [Header("Progress - Bar")]
        public CanvasGroup ProgressBarContainer;
        public Image ProgressBarForeground;
        public TMPro.TextMeshProUGUI ProgressBarPercentageText;
        public RectTransform ProgressBarFirstStar;
        public RectTransform ProgressBarSecondStar;
        public RectTransform ProgressBarThirdStar;
        public Image ProgressBarFirstStarImage;
        public Image ProgressBarSecondStarImage;
        public Image ProgressBarThirdStarImage;
        public float ProgressBarOneStarPercentage = 30f;
        public float ProgressBarTwoStarPercentage = 50f;
        public float ProgressBarThreeStarPercentage = 80f;

        [Header("Gold")]
        public RectTransform GoldContainer;
        public RectTransform GoldImage;
        public TMPro.TextMeshProUGUI GoldText;

        [Header("Level - Completed")]
        public RectTransform LevelCompletedContainer;
        public RectTransform LevelSuccessContainer;
        public RectTransform LevelFailContainer;
        public RectTransform LevelStarContainer;
        public Image LevelCompletedFirstStar;
        public Image LevelCompletedSecondStar;
        public Image LevelCompletedThirdStar;
        public Button LevelLoseRestartButton;
        public Button LevelWinRestartButton;
        public Button LevelWinNextLevelButton;
        public TMPro.TextMeshProUGUI LevelCompletedText;
        public TMPro.TextMeshProUGUI LevelCompletedGoldText;

        [Header("Charge")]
        public RectTransform ChargeContainer;
        public Image ChargeImage;
        public TMPro.TextMeshProUGUI TapFastText;

        [Header("Timing Cursor")]
        public float TimingAngle = 15f;
        public float TimingSpeed = 15f;
        public float TimingAngleLimit = 50f;
        public bool TimingRandomStartValues = true;
        [Condition("TimingRandomStartValues", false, true)] public float TimingCursorStartAngle = 0f;
        [Condition("TimingRandomStartValues", false, true)] public int TimingDirection = 1;
        public CanvasGroup TimingBarContainer;
        public Image TimingCursor;
        public List<TimingFeature> TimingFeatures;
        public List<Image> TimingImages;

        [Header("Upgrade Buttons")]
        public CanvasGroup UpgradeButtonContainer;
        public UpgradeButton FuelUpgradeButton;
        public UpgradeButton ScaleUpgradeButton;
        public UpgradeButton SpeedUpgradeButton;

        [Header("Navigation")]
        public RectTransform MinimapContainer;
        public TouchUIDetection NavigationTouchDetection;
        public RawImage NavigationImage;
        public RectTransform NavigationBoundsBottomLeft;
        public RectTransform NavigationBoundsTopLeft;
        public RectTransform NavigationBoundsTopRight;
        public RectTransform NavigationBoundsBottomRight;
        public TMPro.TextMeshProUGUI NavigationTimeLeftText;
        public TMPro.TextMeshProUGUI NavigationTimeLeftMainText;
        public TMPro.TextMeshProUGUI NavigationTimeLeftFrictionText;

        [Header("Haptic")]
        public Button HapticButton;
        public Sprite HapticEnabledSprite;
        public Sprite HapticDisabledSprite;

        [Header("Others")]
        public RectTransform ItemUIContainer;
        public ItemUI PointItemUI;

        public float MaxLevelFinishScoreScale { get; private set; } = 1f;
        public bool IsTouchesEnabled { get; private set; } = false;
        public bool IsSkillUpgradeActive { get; private set; } = false;
        public bool IsTimingCursorActive { get; private set; } = false;

        private Tweener _progressTextTweener;
        private Tweener _goldTweener;
        private Tweener _chargeTapFastTweener;

        private int _timingRandomIndex;
        private List<byte> _timingFeatureIndexes;
        private float _scrapeTime;

        private void Start()
        {
            // InitializeTimingImages();
            // InitializeProgressBar();

            UpdateLevelText();
            UpdateProgressText(false);
            UpdateItems(false);
            UpdateLevelProgress();
            InitiateTimingBar();

            /*
            DOTween.Sequence().Append(RocketColorCurrent.transform.DOScale(1.1f, .75f))
                                .Append(RocketColorCurrent.transform.DOScale(1f, .5f))
                                .SetLoops(-1);
            */

            // ShowUpgradeButtons(.25f);

            //DOTween.Sequence().Append(HoldAndDragToStart.DOAnchorPosY(HoldAndDragToStart.anchoredPosition.y + 30f, .65f).SetEase(Ease.InOutCubic))
            //                    .Append(HoldAndDragToStart.DOAnchorPosY(HoldAndDragToStart.anchoredPosition.y, .75f))
            //                    .SetLoops(-1);

            ProgressCountText.gameObject.SetActive(false);
            TouchToStart.gameObject.SetActive(true);
            ScrapePointText.gameObject.SetActive(false);

            // Hide Touch to start screen and start game.
            LevelManager.Instance.StartLevel();

            SetHaptic(ProgressManager.Instance.GetHapticEnabled());
        }

        private void Update()
        {
            if (Spatula.Instance.CurrentStatus == SpatulaStatus.Scraping)
            {
                _scrapeTime = ScrapeTextDissappearTime;

                ScrapePointText.SetText(Spatula.Instance.CurrentSpatulaPoint.ToString());
                ScrapePointText.transform.localScale += Vector3.one * Time.deltaTime;
                if (ScrapePointText.transform.localScale.x > 3f)
                    ScrapePointText.transform.localScale = Vector3.one * 3f;
            }
            else if (_scrapeTime > 0f)
            {
                _scrapeTime -= Time.deltaTime;
                if (_scrapeTime <= 0f)
                {
                    ScrapePointText.gameObject.SetActive(false);
                    ScrapePointText.transform.localScale = Vector3.one;
                }
            }
        }

        private void LateUpdate()
        {
            UpdateLevelProgress();
            UpdateTimingBar();
            UpdateNavigationTimeLeft();
        }

        private void InitializeProgressBar()
        {
            Vector3 startPosition = ProgressBarFirstStar.position;
            Vector3 endPosition = ProgressBarThirdStar.position;
            Vector3 nodePosition = ProgressBarFirstStar.position;

            nodePosition.x = Vector3.Lerp(startPosition, endPosition, ProgressBarOneStarPercentage / (float)100).x;
            ProgressBarFirstStar.position = nodePosition;
            nodePosition.x = Vector3.Lerp(startPosition, endPosition, ProgressBarTwoStarPercentage / (float)100).x;
            ProgressBarSecondStar.position = nodePosition;
            nodePosition.x = Vector3.Lerp(startPosition, endPosition, ProgressBarThreeStarPercentage / (float)100).x;
            ProgressBarThirdStar.position = nodePosition;

            ProgressBarForeground.fillAmount = 0f;
            ProgressBarPercentageText.SetText("%0");
        }

        private void InitializeTimingImages()
        {
            _timingFeatureIndexes = new List<byte>
            {
                3,2,1,0,1,2,3
            };

            for (int index = 0; index < TimingImages.Count; index++)
            {
                TimingImages[index].color = TimingFeatures[_timingFeatureIndexes[index]].Color;
            }
        }

        public void SetProgressBarActive(bool active, float duration = .75f, float delay = 0f)
        {
            float fadeTarget = active ? 1f : 0f;
            ProgressBarContainer.DOFade(fadeTarget, duration).SetDelay(delay);
        }

        public void ProgressNodeChanged(int destroyedCount, int totalDestroyedCount, int totalCount)
        {
            StartCoroutine(IProgressNodeChanged(destroyedCount, totalDestroyedCount, totalCount));
        }

        private IEnumerator IProgressNodeChanged(int destroyedCount, int totalDestroyedCount, int totalCount)
        {
            float duration = .5f;
            float currentT = 0f;
            float startPercentage = totalDestroyedCount / (float)totalCount;
            float targetPercentage = (totalDestroyedCount + destroyedCount) / (float)totalCount;

            bool firstStarReached = false;
            bool secondStarReached = false;
            bool thirdStarReached = false;

            while (currentT < duration)
            {
                currentT += Time.deltaTime;

                float currentPercentage = Mathf.Lerp(startPercentage, targetPercentage, currentT / duration).Clamp(0f, 100f);
                ProgressBarForeground.fillAmount = currentPercentage;
                ProgressBarPercentageText.SetText($"%{Mathf.FloorToInt(currentPercentage * 100)}");

                if (!firstStarReached && currentPercentage * 100f > ProgressBarOneStarPercentage)
                {
                    firstStarReached = true;
                    ProgressBarFirstStarImage.sprite = FilledStar;
                    ProgressBarFirstStarImage.transform.DOPunchScale(Vector3.one * .5f, .3f);
                }

                if (!secondStarReached && currentPercentage * 100f > ProgressBarTwoStarPercentage)
                {
                    secondStarReached = true;
                    ProgressBarSecondStarImage.sprite = FilledStar;
                    ProgressBarSecondStarImage.transform.DOPunchScale(Vector3.one * .5f, .3f);
                }

                if (!thirdStarReached && currentPercentage * 100f > ProgressBarThreeStarPercentage)
                {
                    thirdStarReached = true;
                    ProgressBarThirdStarImage.sprite = FilledStar;
                    ProgressBarThirdStarImage.transform.DOPunchScale(Vector3.one * .5f, .3f);
                }

                yield return null;
            }
        }

        private void InitiateTimingBar()
        {
            for (int index = 0; index < TimingFeatures.Count; index++)
                TimingFeatures[index].Index = (byte)index;
        }

        private void UpdateTimingBar()
        {
            if (!IsTimingCursorActive) return;

            var newAngle = TimingCursor.transform.eulerAngles + Vector3.forward * TimingSpeed * Time.deltaTime * TimingDirection;
            if (newAngle.z >= 180f)
                newAngle.z -= 360f;

            if (newAngle.z >= TimingAngleLimit)
                TimingDirection = -1;
            else if (newAngle.z <= -TimingAngleLimit)
                TimingDirection = 1;

            TimingCursor.transform.eulerAngles = newAngle;
        }

        private void UpdateNavigationTimeLeft()
        {
            /*
            string timeLeft = GameplayController.Instance.NavigationTimeLeft.ToString("F2");
            int indexOfFriction = timeLeft.IndexOf(",");
            if (indexOfFriction == -1)
                indexOfFriction = timeLeft.IndexOf(".");

            NavigationTimeLeftMainText.SetText(timeLeft.Substring(0, indexOfFriction) + ",");
            NavigationTimeLeftFrictionText.SetText(timeLeft.Substring(indexOfFriction + 1));
            */
        }

        public void HideTimingBar(float delay)
        {
            IsTouchesEnabled = false;
            IsTimingCursorActive = false;

            TimingBarContainer.DOFade(0f, .2f).SetDelay(delay);
        }

        public void ShowTimingBar()
        {
            IsTimingCursorActive = true;

            /*
            if (_timingFeatureIndexes == null)
                _timingFeatureIndexes = new List<byte>(TimingImages.Count);
            else
                _timingFeatureIndexes.Clear();

            var rnd = new System.Random(100 + ProgressManager.Instance.GetCurrentLevelIndex() * 1000 + _timingRandomIndex);
            var timingColors = new List<TimingFeature>(TimingFeatures);
            for (int index = 0; index < TimingImages.Count; index ++)
            {
                if (timingColors.Count <= 0)
                    timingColors = new List<TimingFeature>(TimingFeatures);

                var rndIndex = rnd.Next(0, timingColors.Count);
                var timingFeature = timingColors[rndIndex];
                timingColors.RemoveAt(rndIndex);

                _timingFeatureIndexes.Add(timingFeature.Index);

                TimingImages[index].color = timingFeature.Color;
            }

            TimingCursor.transform.rotation = Quaternion.Euler(TimingCursor.transform.eulerAngles.x, TimingCursor.transform.eulerAngles.y, TimingCursorStartAngle);

            _timingRandomIndex++;
            */

            if (TimingRandomStartValues)
            {
                var maxAngle = Mathf.FloorToInt(_timingFeatureIndexes.Count * TimingAngle * .5f);
                var rnd = new System.Random(100 + ProgressManager.Instance.GetCurrentLevelIndex() * 1000 + _timingRandomIndex);
                var rndAngle = rnd.Next(-maxAngle, maxAngle);

                TimingCursor.transform.rotation = Quaternion.Euler(TimingCursor.transform.eulerAngles.x, TimingCursor.transform.eulerAngles.y, rndAngle);
                TimingDirection = rndAngle % 2 == 0 ? 1 : -1;
            }
            else
            {
                TimingCursor.transform.rotation = Quaternion.Euler(TimingCursor.transform.eulerAngles.x, TimingCursor.transform.eulerAngles.y, TimingCursorStartAngle);
            }

            _timingRandomIndex++;            

            TimingBarContainer.DOFade(1f, .2f).OnComplete(() =>
            {
                IsTouchesEnabled = true;
            });
        }

        public void ShowUpgradeButtons(float delay = 0f, float duration = .2f)
        {
            IsSkillUpgradeActive = true;

            UpgradeButtonContainer.gameObject.SetActive(true);
            UpgradeButtonContainer.interactable = true;
            FuelUpgradeButton.CheckUpgradable();
            SpeedUpgradeButton.CheckUpgradable();
            ScaleUpgradeButton.CheckUpgradable();

            UpgradeButtonContainer.DOFade(1f, duration).SetDelay(delay).OnComplete(() =>
            {
                IsTouchesEnabled = true;
            });
        }

        public virtual void ShowItemUI(Vector3 worldPos, int quantity)
        {
            var screenPos = CameraController.Instance.MainCamera.WorldToScreenPoint(worldPos);
            var itemUI = Instantiate(PointItemUI, ItemUIContainer.transform);
            itemUI.transform.position = screenPos;
            itemUI.SetText($"+ {quantity}");
            itemUI.StartMoving();
        }

        public void HideUpgradeButtons(float delay)
        {
            IsTouchesEnabled = false;
            IsSkillUpgradeActive = false;
            UpgradeButtonContainer.interactable = false;
            UpgradeButtonContainer.DOFade(0f, .2f).SetDelay(delay).OnComplete(() =>
            {
                UpgradeButtonContainer.gameObject.SetActive(false);
                ShowTimingBar();
            });
        }

        public void RestartLevel()
        {
            LevelLoseRestartButton.interactable = false;
            LevelManager.Instance.RestartLevel(0f);
        }

        public void RestartLevelAfterCompleted()
        {
            LevelWinRestartButton.interactable = false;
            ProgressManager.Instance.SetSceneIndexToPrevious();
            LevelManager.Instance.LoadNextLevel(0f);
        }

        public void LoadNextLevel()
        {
            LevelWinNextLevelButton.interactable = false;
            LevelManager.Instance.LoadNextLevel(0f);
        }

        private void UpdateLevelText()
        {
            LevelText.SetText("Level " + ProgressManager.Instance.GetCurrentLevelName());
            ProgressLevelText.SetText(ProgressManager.Instance.GetCurrentLevelName());
        }

        private void UpdateLevelProgress()
        {
            SetLevelProgress(0f);
        }

        private void SetLevelProgress(float fillAmount)
        {
            ProgressSliderImage.fillAmount = fillAmount;
        }

        private void UpdateProgressText(bool shake = true)
        {
            ProgressCountText.gameObject.SetActive(false);
            ProgressCountText.SetText("0");

            var isProgressValueChanged = true;

            if (isProgressValueChanged && shake)
            {
                _progressTextTweener?.Complete();
                _progressTextTweener = ProgressCountText.rectTransform.DOShakeScale(.2f, vibrato: 20, strength: .3f);
            }
        }

        private void UpdateGoldItem(bool shake = true)
        {
            var quantity = ProgressManager.Instance.GetItemQuantity(ItemType.Gold); // + LevelManager.Instance.GetPickedItemCountThisLevel(ItemType.Gold);

            GoldText.SetText(quantity > 99999 ? "99999+".ToString() : quantity.ToString());

            if (shake)
            {
                _goldTweener?.Complete();
                _goldTweener = GoldText.rectTransform.DOShakeScale(.2f, vibrato: 20, strength: .3f);
            }
        }

        private void ShowLevelCompletedScreen(Dictionary<ItemType, uint> collectedItems, bool success)
        {
            var goldPicked = collectedItems.ContainsKey(ItemType.Gold) ? collectedItems[ItemType.Gold] : 0;

            LevelCompletedContainer.gameObject.SetActive(true);
            LevelCompletedGoldText.SetText(goldPicked.ToString());
            // BlurPostProcessVolume.enabled = true;
            ProgressContainer.gameObject.SetActive(false);
            ProgressBarContainer.gameObject.SetActive(false);
            LevelText.gameObject.SetActive(true);

            if (success)
            {
                ScaleText.SetText($"{GameplayController.Instance.PointsScale} X");
                LevelCompletedText.SetText("COMPLETED!");

                LevelSuccessContainer.gameObject.SetActive(true);

                StartCoroutine(ISuccessScreen());
            }
            else
            {
                LevelCompletedText.SetText("FAILED!");

                LevelFailContainer.gameObject.SetActive(true);

                StartCoroutine(IFailScreen());
            }
        }

        private IEnumerator ISuccessScreen()
        {
            yield return new WaitForSeconds(.25f);

            // TODO:
            if (true/*ProgressFirstStarReached*/)
            {
                LevelCompletedFirstStar.sprite = FilledStar;
                LevelCompletedFirstStar.rectTransform.DOPunchScale(Vector3.one * .2f, .4f);
            }

            if (true/*ProgressSecondStarReached*/)
            {
                yield return new WaitForSeconds(.3f);
                LevelCompletedSecondStar.sprite = FilledStar;
                LevelCompletedSecondStar.rectTransform.DOPunchScale(Vector3.one * .2f, .4f);
            }

            if (true/*ProgressThirdStarReached*/)
            {
                yield return new WaitForSeconds(.3f);
                LevelCompletedThirdStar.sprite = FilledStar;
                LevelCompletedThirdStar.rectTransform.DOPunchScale(Vector3.one * .2f, .4f);

                yield return new WaitForSeconds(.25f);
                LevelStarContainer.DOPunchScale(Vector3.one * .025f, .4f);
            }

            yield return new WaitForSeconds(.25f);

            var sequence = DOTween.Sequence();
            sequence.Join(LevelCompletedText.rectTransform.DOPunchScale(Vector3.one * .2f, .5f, vibrato: 0, elasticity: 0f));
        }

        private IEnumerator IFailScreen()
        {
            yield return new WaitForSeconds(.25f);

            var sequence = DOTween.Sequence();
            sequence.Join(LevelCompletedText.rectTransform.DOPunchScale(Vector3.one * .2f, .5f, vibrato: 0, elasticity: 0f));
        }

        public void SetChargeActive(bool active, float duration = .75f, float delay = 0f)
        {
            float fadeTarget = active ? 1f : 0f;
            ChargeContainer.gameObject.GetComponent<CanvasGroup>().DOFade(fadeTarget, duration).SetDelay(delay);
        }

        public void SetChargePower(float power)
        {
            //if (power > ChargeImage.fillAmount)
            //{
            //    _chargeTapFastTweener?.Complete();
            //    _chargeTapFastTweener = TapFastText.rectTransform.DOShakeScale(.1f, vibrato: 20, strength: .3f);
            //}

            ChargeImage.fillAmount = power;
        }

        private void UpdateItems(bool shake = true)
        {
            UpdateGoldItem(shake);
        }

        public virtual void OnHapticButtonClicked()
        {
            SetHaptic(!ProgressManager.Instance.GetHapticEnabled());
        }

        protected virtual void SetHaptic(bool status)
        {
            ProgressManager.Instance.SetHapticStatus(status);

            HapticButton.image.sprite = status ? HapticEnabledSprite : HapticDisabledSprite;

            HapticManager.Instance.IsEnabled = status;
        }


        private void OnEnable()
        {
            this.EventStartListening<PickableItemEvent>();
            this.EventStartListening<LevelEvent>();
            this.EventStartListening<TouchEvent>();
            this.EventStartListening<UpgradeEvent>();
            this.EventStartListening<TouchFirstTapEvent>();
            this.EventStartListening<SpatulaEvent>();
        }

        private void OnDisable()
        {
            this.EventStopListening<PickableItemEvent>();
            this.EventStopListening<LevelEvent>();
            this.EventStopListening<TouchEvent>();
            this.EventStopListening<UpgradeEvent>();
            this.EventStopListening<TouchFirstTapEvent>();
            this.EventStopListening<SpatulaEvent>();
        }

        #region Events
        public void OnCGEvent(PickableItemEvent currentEvent)
        {
            UpdateItems();
        }

        public void OnCGEvent(LevelEvent currentEvent)
        {
            switch (currentEvent.EventType)
            {
                case LevelEventType.PreCompleted:
                    {
                        SetLevelProgress(1f);
                    }
                    break;
                case LevelEventType.Completed:
                    {
                        ShowLevelCompletedScreen(currentEvent.ItemsGained, true);
                    }
                    break;
                case LevelEventType.Failed:
                    {
                        ShowLevelCompletedScreen(currentEvent.ItemsGained, false);
                    }
                    break;
            }
        }

        public void OnCGEvent(TouchEvent currentEvent)
        {
            if (!IsTouchesEnabled) return;

            if (currentEvent.NewState)
            {
                if (IsSkillUpgradeActive)
                {
                    HideUpgradeButtons(.2f);
                }
                else if (IsTimingCursorActive)
                {
                    HideTimingBar(.5f);

                    var angleLimitMax = _timingFeatureIndexes.Count * TimingAngle * .5f;
                    var angle = TimingCursor.transform.eulerAngles.z;
                    if (angle >= 180f)
                        angle -= 360f;
                    for (int index = 0; index < _timingFeatureIndexes.Count; index++)
                    {
                        angleLimitMax -= TimingAngle;
                        if (angleLimitMax < angle)
                        {
                            var speed = TimingFeatures[_timingFeatureIndexes[index]].Speed;
                            Debug.Log("SPEED: " + speed.ToString() + ", INDEX: " + index);

                            break;
                        }
                    }
                }
            }
        }

        public void OnCGEvent(UpgradeEvent currentEvent)
        {
            UpdateItems();
        }

        public void OnCGEvent(TouchFirstTapEvent eventType)
        {
            TouchToStart.gameObject.SetActive(false);
            LevelText.gameObject.SetActive(false);
        }

        public void OnCGEvent(SpatulaEvent currentEvent)
        {
            switch (currentEvent.EventType)
            {
                case SpatulaEventType.ScrapeStarted:
                    {
                        if (_scrapeTime <= 0f)
                        {
                            ScrapePointText.gameObject.SetActive(true);
                            ScrapePointText.transform.localScale = Vector3.one;
                        }

                        _scrapeTime = ScrapeTextDissappearTime;
                    }
                    break;
            }
        }
        #endregion
    }
}