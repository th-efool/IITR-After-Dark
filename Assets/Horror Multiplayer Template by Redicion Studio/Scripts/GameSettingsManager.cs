// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

namespace RedicionStudio
{
    public class GameSettingsManager : MonoBehaviour
    {
        // Graphics Settings
        public Button[] resolutionButtons;
        public Button[] qualityButtons;
        public Button[] fullscreenButtons;
        public Button[] antiAliasingButtons;
        public Button[] textureQualityButtons;
        public Button[] shadowsButtons;
        public GameObject vsyncCheckmark;
        public GameObject showFPSCheckmark;
        public GameObject enableFirstPersonModeCheckmark;

        // General Settings
        public Slider volumeSlider;
        public Slider cameraSensitivitySliderKeyboard;
        public Slider cameraSensitivitySliderController;

        public AudioMixer audioMixer;

        private Resolution[] resolutions;

        // UI
        public GameObject gameSettingsUI;

        void Start()
        {
            if (Application.isBatchMode)
                return;

            // Initialize resolutions
            resolutions = Screen.resolutions;

            // Load saved settings
            LoadSettings();
        }

        public void SetResolution(int resolutionIndex)
        {
            int buttonIndex = new int();
            foreach (Button button in resolutionButtons)
            {
                if (button.GetComponent<GameSettingsResolutionButton>().resolutionIndex.Equals(resolutionIndex))
                {
                    GameSettingsResolutionButton resolutionButton = button.GetComponent<GameSettingsResolutionButton>();
                    buttonIndex = resolutionButton.buttonIndex;
                    Screen.SetResolution(resolutionButton.widthResolution, resolutionButton.heightResolution, Screen.fullScreenMode);
                }
            }
            UpdateButtonSelection(resolutionButtons, buttonIndex);
            SaveSettings();
        }

        public void SelectResolutionButton(int resolutionIndex)
        {
            foreach (Button button in resolutionButtons)
            {
                if (button.GetComponent<GameSettingsResolutionButton>().buttonIndex.Equals(resolutionIndex))
                {
                    GameSettingsResolutionButton resolutionButton = button.GetComponent<GameSettingsResolutionButton>();
                    Screen.SetResolution(resolutionButton.widthResolution, resolutionButton.heightResolution, Screen.fullScreenMode);
                }
            }
            UpdateButtonSelection(resolutionButtons, resolutionIndex);
            SaveSettings();
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            UpdateButtonSelection(qualityButtons, qualityIndex);
            PlayerPrefs.SetInt("Quality", qualityIndex); // Save Quality setting
            SaveSettings();
        }

        public void SetFullscreenMode(int modeIndex)
        {
            switch (modeIndex)
            {
                case 0:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                case 2:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 3:
                    Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                    break;
            }
            UpdateButtonSelection(fullscreenButtons, modeIndex);
            SaveSettings(); // Save the fullscreen mode change
        }

        public void SetAntiAliasing(int aaIndex)
        {
            QualitySettings.antiAliasing = aaIndex == 0 ? 0 : (int)Mathf.Pow(2, aaIndex);
            UpdateButtonSelection(antiAliasingButtons, aaIndex);
            PlayerPrefs.SetInt("AntiAliasing", aaIndex); // Save AntiAliasing setting
            SaveSettings();
        }

        public void SetTextureQuality(int textureQualityIndex)
        {
            QualitySettings.globalTextureMipmapLimit = textureQualityIndex;
            UpdateButtonSelection(textureQualityButtons, textureQualityIndex);
            PlayerPrefs.SetInt("TextureQuality", textureQualityIndex); // Save TextureQuality setting
            SaveSettings();
        }

        public void SetShadows(int shadowsIndex)
        {
            switch (shadowsIndex)
            {
                case 0:
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    break;
                case 1:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.shadowCascades = 2;
                    break;
                case 2:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.shadowDistance = 100f;
                    QualitySettings.shadowCascades = 2;
                    break;
                case 3:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.shadowDistance = 150f;
                    QualitySettings.shadowCascades = 4;
                    break;
            }
            UpdateButtonSelection(shadowsButtons, shadowsIndex);
            PlayerPrefs.SetInt("Shadows", shadowsIndex); // Save Shadows setting
            SaveSettings();
        }

        public void SetVSync()
        {
            if (QualitySettings.vSyncCount == 1)
            {
                QualitySettings.vSyncCount = 0;
                vsyncCheckmark.SetActive(false);
            }
            else
            {
                QualitySettings.vSyncCount = 1;
                vsyncCheckmark.SetActive(true);
            }
            PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount); // Save VSync setting
            SaveSettings();
        }

        public void SetVSyncValue(int value)
        {
            QualitySettings.vSyncCount = value;
            vsyncCheckmark.SetActive(value == 1);
            PlayerPrefs.SetInt("VSync", value); // Save VSync setting
            SaveSettings();
        }

        public void SetShowFPS()
        {
            if (GetComponent<ShowFPS>().enabled)
            {
                GetComponent<ShowFPS>().enabled = false;
                showFPSCheckmark.SetActive(false);
            }
            else
            {
                GetComponent<ShowFPS>().enabled = true;
                showFPSCheckmark.SetActive(true);
            }
            PlayerPrefs.SetInt("ShowFPS", GetComponent<ShowFPS>().enabled ? 1 : 0); // Save ShowFPS setting
            SaveSettings();
        }

        public void SetShowFPSValue(int value)
        {
            ShowFPS showFPSComponent = GetComponent<ShowFPS>();
            bool enableFPS = (value == 1);

            showFPSComponent.enabled = enableFPS;
            showFPSCheckmark.SetActive(enableFPS);
            PlayerPrefs.SetInt("ShowFPS", value); // Save ShowFPS setting
            SaveSettings();
        }

        public void SetVolume()
        {
            float volumeInDb = Mathf.Log10(volumeSlider.value) * 20;
            audioMixer.SetFloat("volume", volumeInDb);
            PlayerPrefs.SetFloat("Volume", volumeInDb); // Save the volume slider value
        }

        public void SetSensitivity(bool isKeyboardSlider)
        {
            if (isKeyboardSlider)
            {
                float sensitivityKeyboard = Mathf.Clamp(cameraSensitivitySliderKeyboard.value, 0.1f, 5f);
                PlayerPrefs.SetFloat("CameraSensitivity", sensitivityKeyboard);
                cameraSensitivitySliderKeyboard.value = sensitivityKeyboard;
                cameraSensitivitySliderController.value = sensitivityKeyboard;
            }
            else
            {
                float sensitivityController = Mathf.Clamp(cameraSensitivitySliderController.value, 0.1f, 5f);
                PlayerPrefs.SetFloat("CameraSensitivity", sensitivityController);
                cameraSensitivitySliderController.value = sensitivityController;
                cameraSensitivitySliderKeyboard.value = sensitivityController;
            }
        }

        public void SetEnableFirstPersonMode()
        {
            if (enableFirstPersonModeCheckmark.gameObject.activeInHierarchy)
            {
                enableFirstPersonModeCheckmark.SetActive(false);
            }
            else
            {
                enableFirstPersonModeCheckmark.SetActive(true);
            }
            PlayerPrefs.SetInt("EnableFirstPersonMode", enableFirstPersonModeCheckmark.gameObject.activeInHierarchy ? 1 : 0); // Save EnableFirstPersonMode setting
            SaveSettings();
        }

        public void SetEnableFirstPersonModeValue(int value)
        {
            bool _enableFirstPersonModeCheckmark = (value == 1);

            enableFirstPersonModeCheckmark.SetActive(_enableFirstPersonModeCheckmark);
            PlayerPrefs.SetInt("EnableFirstPersonMode", value); // Save EnableFirstPersonMode setting
            SaveSettings();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Resolution", GetSelectedIndex(resolutionButtons));
            PlayerPrefs.SetInt("FullscreenMode", (int)Screen.fullScreenMode); // Save the current fullscreen mode
            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey("Resolution"))
            {
                int resolutionIndex = PlayerPrefs.GetInt("Resolution");
                SelectResolutionButton(resolutionIndex);
            }
            else
            {
                PlayerPrefs.SetInt("Resolution", 3);
                SelectResolutionButton(3);
            }

            if (PlayerPrefs.HasKey("Quality"))
            {
                int qualityIndex = PlayerPrefs.GetInt("Quality");
                SetQuality(qualityIndex);
            }
            else
            {
                PlayerPrefs.SetInt("Quality", 3);
                SetQuality(3);
            }

            if (PlayerPrefs.HasKey("FullscreenMode"))
            {
                int fullscreenModeIndex = PlayerPrefs.GetInt("FullscreenMode");
                Screen.fullScreenMode = (FullScreenMode)fullscreenModeIndex; // Apply the saved fullscreen mode
                UpdateButtonSelection(fullscreenButtons, fullscreenModeIndex); // Update the UI to reflect the saved state
            }
            else
            {
                PlayerPrefs.SetInt("FullscreenMode", (int)FullScreenMode.ExclusiveFullScreen);
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                UpdateButtonSelection(fullscreenButtons, 0); // Default to Exclusive Fullscreen
            }

            if (PlayerPrefs.HasKey("CameraSensitivity"))
            {
                float sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat("CameraSensitivity"), 0.1f, 5f);
                cameraSensitivitySliderKeyboard.value = sensitivity;
                cameraSensitivitySliderController.value = sensitivity;
            }
            else
            {
                float defaultSensitivity = 1f;
                PlayerPrefs.SetFloat("CameraSensitivity", defaultSensitivity);
                cameraSensitivitySliderKeyboard.value = defaultSensitivity;
                cameraSensitivitySliderController.value = defaultSensitivity;
            }

            if (PlayerPrefs.HasKey("AntiAliasing"))
            {
                int aaIndex = PlayerPrefs.GetInt("AntiAliasing");
                SetAntiAliasing(aaIndex);
            }
            else
            {
                PlayerPrefs.SetInt("AntiAliasing", 3);
                SetAntiAliasing(3);
            }

            if (PlayerPrefs.HasKey("TextureQuality"))
            {
                int textureQualityIndex = PlayerPrefs.GetInt("TextureQuality");
                SetTextureQuality(textureQualityIndex);
            }
            else
            {
                PlayerPrefs.SetInt("TextureQuality", 0);
                SetTextureQuality(0);
            }

            if (PlayerPrefs.HasKey("Shadows"))
            {
                int shadowsIndex = PlayerPrefs.GetInt("Shadows");
                SetShadows(shadowsIndex);
            }
            else
            {
                PlayerPrefs.SetInt("Shadows", 3);
                SetShadows(3);
            }

            if (PlayerPrefs.HasKey("VSync"))
            {
                int vsyncValue = PlayerPrefs.GetInt("VSync");
                SetVSyncValue(vsyncValue);
            }
            else
            {
                PlayerPrefs.SetInt("VSync", 1);
                SetVSyncValue(1);
            }

            if (PlayerPrefs.HasKey("ShowFPS"))
            {
                int showFPSValue = PlayerPrefs.GetInt("ShowFPS");
                SetShowFPSValue(showFPSValue);
            }
            else
            {
                PlayerPrefs.SetInt("ShowFPS", 0);
                SetShowFPSValue(0);
            }

            if (PlayerPrefs.HasKey("Volume"))
            {
                float savedVolume = PlayerPrefs.GetFloat("Volume");
                volumeSlider.value = savedVolume;
                // Update the volume in the AudioMixer
                audioMixer.SetFloat("volume", savedVolume);
            }
            else
            {
                PlayerPrefs.SetFloat("Volume", 0.5f);
                float savedVolume = 0.5f;
                volumeSlider.value = savedVolume;
                // Update the volume in the AudioMixer
                audioMixer.SetFloat("volume", savedVolume);
            }

            if (PlayerPrefs.HasKey("EnableFirstPersonMode"))
            {
                int EnableFirstPersonModeValue = PlayerPrefs.GetInt("EnableFirstPersonMode");
                SetEnableFirstPersonModeValue(EnableFirstPersonModeValue);
            }
            else
            {
                PlayerPrefs.SetInt("EnableFirstPersonMode", 0);
                SetEnableFirstPersonModeValue(0);
            }
        }

        private void UpdateButtonSelection(Button[] buttons, int selectedIndex)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                Image buttonImage = buttons[i].GetComponent<Image>();
                Transform checkmark = buttons[i].transform.Find("Checkmark");

                if (i == selectedIndex)
                {
                    buttonImage.color = new Color(1, 0, 0, 0.4f); // Red with 100 transparency
                    checkmark.gameObject.SetActive(true);
                }
                else
                {
                    buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.4f); // Grey with 100 transparency
                    checkmark.gameObject.SetActive(false);
                }
            }
        }

        private int GetSelectedIndex(Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                Transform checkmark = buttons[i].transform.Find("Checkmark");
                if (checkmark.gameObject.activeSelf)
                {
                    return i;
                }
            }
            return 0;
        }

        #region Debug
        public int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == width && resolutions[i].height == height)
                {
                    return i;
                }
            }
            return -1; // Return -1 if the resolution is not found
        }
        #endregion
    }
}