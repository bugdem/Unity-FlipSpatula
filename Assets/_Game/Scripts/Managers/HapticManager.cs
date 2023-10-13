using UnityEngine;
using ClocknestGames.Library.Utils;
using MoreMountains.NiceVibrations;
using System.Collections;

namespace ClocknestGames.Game.Core
{
    [System.Serializable]
    public class HapticPresetFile
    {
        public bool IsEnabled = true;
        public string Name;
        public TextAsset AHAPFile;
        public MMNVAndroidWaveFormAsset WaveFormAsset;
        public MMNVRumbleWaveFormAsset RumbleWaveFormAsset;
        public float HapticTime = 10f;
    }

    [System.Serializable]
    public class HapticPresetBaseSetting
    {
        public bool IsEnabled = true;
        public HapticTypes Type = HapticTypes.LightImpact;
    }

    [System.Serializable]
    public class HapticPresetSetting : HapticPresetBaseSetting
    {
        public float Intensity = 1f;
        public float Sharpness = 1f;
        public float Duration = 1f;
        public bool HapticOnOldPhones = true;
    }

    public class HapticManager : PersistentSingleton<HapticManager>
    {
        [Header("Settings")]
        public bool IsEnabled = true;

        [Header("Haptics")]
        public HapticPresetBaseSetting OnKilled;
        public HapticPresetBaseSetting OnItemPicked;
        public HapticPresetBaseSetting OnStuck;
        public HapticPresetBaseSetting OnSliced;
        public HapticPresetBaseSetting OnScrapeStart;
        public HapticPresetBaseSetting OnFlip;
        public HapticPresetBaseSetting OnBreakable;
        public HapticPresetBaseSetting OnSuccess;
        public HapticPresetBaseSetting OnFail;
        public HapticPresetFile OnBasket;
        public HapticPresetFile OnScrapeDefault;
        public HapticPresetFile OnScrapeLevelFinish;

        protected bool _isScrapeActive;
        protected float _scrapeTime;
        protected Coroutine _scrapeCoroutine;

        public void HapticOnCharacterKilled()
        {
            HapticSingle(OnKilled);
        }

        public void HapticOnItemPicked()
        {
            HapticSingle(OnItemPicked);
        }

        public void HapticOnStuck()
        {
            HapticSingle(OnStuck);
        }

        public void HapticOnSliced()
        {
            HapticSingle(OnSliced);
        }

        public void HapticOnScrapeStart(SurfaceScrapeType surfaceType)
        {
            // HapticSingle(OnScrapeStart);

            HapticOnScrape(surfaceType);
        }

        public void HapticOnFlip()
        {
            HapticSingle(OnFlip);
        }

        public void HapticOnSuccess()
        {
            HapticSingle(OnSuccess);
        }

        public void HapticOnFail()
        {
            HapticSingle(OnFail);
        }

        public void HapticOnBasket()
        {
            HapticAHAP(OnBasket);
        }

        public void HapticOnBreakable()
        {
            HapticSingle(OnBreakable);
        }

        public void HapticOnScrape(SurfaceScrapeType surfaceType)
        {
            StartCoroutine(IHapticOnScrape(surfaceType));
        }

        public IEnumerator IHapticOnScrape(SurfaceScrapeType surfaceType)
        {
            var hapticPreset = GetHapticPreset(surfaceType);

            _isScrapeActive = true;
            _scrapeTime = hapticPreset.HapticTime;

            if (_scrapeCoroutine != null)
                StopCoroutine(_scrapeCoroutine);

            HapticAHAP(hapticPreset);

            while (_isScrapeActive && _scrapeTime > 0f)
            {
                _scrapeTime -= Time.deltaTime;
                yield return null;
            }

            // If we are still scraping, restart scrape haptic.
            if (_isScrapeActive)
                HapticOnScrape(surfaceType);
        }

        protected HapticPresetFile GetHapticPreset(SurfaceScrapeType surfaceType)
        {
            if (surfaceType == SurfaceScrapeType.LevelFinish)
                return OnScrapeLevelFinish;

            return OnScrapeDefault;
        }

        public void StopHapticScrape()
        {
            _isScrapeActive = false;

            MMVibrationManager.StopAllHaptics();
        }

        private void HapticContinous(HapticPresetSetting hapticSetting)
        {
            if (!IsEnabled || !hapticSetting.IsEnabled) return;

            // MMVibrationManager.StopAllHaptics();
            MMVibrationManager.ContinuousHaptic(hapticSetting.Intensity, hapticSetting.Sharpness, hapticSetting.Duration, hapticSetting.Type, this, true, oldiOSRegularVibrate: hapticSetting.HapticOnOldPhones);
        }

        private void HapticSingle(HapticPresetBaseSetting hapticSetting)
        {
            if (!IsEnabled || !hapticSetting.IsEnabled) return;

            // MMVibrationManager.StopAllHaptics();
            MMVibrationManager.Haptic(hapticSetting.Type);
        }

        // Warning: TransientHaptic causing a lag on call, interestingly. So dont use this.
        private void HapticTransient(HapticPresetSetting hapticSetting)
        {
            if (!IsEnabled || !hapticSetting.IsEnabled) return;

            // MMVibrationManager.StopAllHaptics();
            MMVibrationManager.TransientHaptic(hapticSetting.Intensity, hapticSetting.Sharpness);
        }

        private void HapticAHAP(HapticPresetFile preset)
        {
            if (!IsEnabled || !preset.IsEnabled) return;

            MMVibrationManager.AdvancedHapticPattern(preset.AHAPFile.text,
                                                     preset.WaveFormAsset?.WaveForm.Pattern, preset.WaveFormAsset?.WaveForm.Amplitudes, -1,
                                                     preset.RumbleWaveFormAsset?.WaveForm.Pattern, preset.RumbleWaveFormAsset?.WaveForm.LowFrequencyAmplitudes,
                                                     preset.RumbleWaveFormAsset?.WaveForm.HighFrequencyAmplitudes, -1,
                                                     HapticTypes.LightImpact, this);
        }
    }
}