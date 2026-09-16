// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using StarterAssets;

namespace RedicionStudio
{
    public class EmoteWheel : NetworkBehaviour
    {
        [Header("Ui")]
        public GameObject EmoteWheelUi;
        RectTransform rectTransform;
        private static bool isEmoteWheelActive = false;
        public bool inEmoteWheel = false;
        bool hasClosedEmoteWheel = true;
        public GameObject lineUiElement;
        [Header("Emotes")]
        public List<EmoteWheelItem> emotes;
        public TMPro.TMP_Text currentEmoteName;
        public TMPro.TMP_Text currentEmoteInfo;
        public bool isPlayingAnimation = false;
        [Header("Areas")]
        [HideInInspector]
        public bool Top;
        [HideInInspector]
        public bool Down;
        [HideInInspector]
        public bool Right;
        [HideInInspector]
        public bool Left;

        private RedicionStudio.PlayerInputs _input;
        private RoomManager _roomManager;
        private MainMenuManager _MainMenuManager;

        private void Start()
        {
            foreach (EmoteWheelItem emoteItem in emotes)
            {
                emoteItem.Load();
            }
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();
            if (_roomManager == null)
                _roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
            if (_MainMenuManager == null)
                _MainMenuManager = GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManager>();

            if (_input != null && _roomManager != null && _MainMenuManager != null && _input.emoteWheel && !BSystem.BSystemUI.Instance.Active && !RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop && GetComponent<Health>().isDeath == false && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().usesParachute && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().isAiming && !RedicionStudio.InventorySystem.PlayerInventoryModule.inWeaponWheel && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().chatWindow.isChatOpen && _roomManager.MatchRunning && !_MainMenuManager.pauseMenuActive)
            {
                if (!isEmoteWheelActive)
                {
                    inEmoteWheel = !inEmoteWheel;
                    if (inEmoteWheel)
                    {
                        isEmoteWheelActive = true;
                        if (BSystem.BSystem.inMenu)
                        {
                            BSystem.BSystem.inMenu = false;
                            BSystem.BSystemUI.Instance.SetActive(false);

                        }
                        EmoteWheelUi.SetActive(true);
                        TPCameraController.LockCursor(false);
                    }
                    else
                    {
                        EmoteWheelUi.SetActive(false);
                        TPCameraController.LockCursor(true);
                        isEmoteWheelActive = false;
                    }
                }
                if (isEmoteWheelActive)
                {
                    hasClosedEmoteWheel = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    if (Top)
                    {
                        currentEmoteName.text = emotes[0].EmoteName;
                        currentEmoteInfo.text = emotes[0].InfoText;
                        foreach (EmoteWheelItem emoteItem in emotes)
                        {
                            if (emoteItem.EmoteName != emotes[0].EmoteName)
                                emoteItem.Deselect();
                        }
                        emotes[0].Select();
                    }
                    else if (Down)
                    {
                        currentEmoteName.text = emotes[1].EmoteName;
                        currentEmoteInfo.text = emotes[1].InfoText;
                        foreach (EmoteWheelItem emoteItem in emotes)
                        {
                            if (emoteItem.EmoteName != emotes[1].EmoteName)
                                emoteItem.Deselect();
                        }
                        emotes[1].Select();
                    }
                    else if (Right)
                    {
                        currentEmoteName.text = emotes[2].EmoteName;
                        currentEmoteInfo.text = emotes[2].InfoText;
                        foreach (EmoteWheelItem emoteItem in emotes)
                        {
                            if (emoteItem.EmoteName != emotes[2].EmoteName)
                                emoteItem.Deselect();
                        }
                        emotes[2].Select();
                    }
                    else if (Left)
                    {
                        currentEmoteName.text = emotes[3].EmoteName;
                        currentEmoteInfo.text = emotes[3].InfoText;
                        foreach (EmoteWheelItem emoteItem in emotes)
                        {
                            if (emoteItem.EmoteName != emotes[3].EmoteName)
                                emoteItem.Deselect();
                        }
                        emotes[3].Select();
                    }
                }
            }
            if (!_input.emoteWheel)
            {
                EmoteWheelUi.SetActive(false);
                if (!BSystem.BSystemUI.Instance.Active && !RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop && !RedicionStudio.InventorySystem.PlayerInventoryModule.inWeaponWheel && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().chatWindow.isChatOpen && _roomManager.MatchRunning && !_MainMenuManager.pauseMenuActive)
                {
                    TPCameraController.LockCursor(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                isEmoteWheelActive = false;
                inEmoteWheel = false;
                if (hasClosedEmoteWheel == false)
                {
                    hasClosedEmoteWheel = true;
                    if (Top)
                    {
                        isPlayingAnimation = false;
                        StopCoroutine(EndEmote(0, ""));
                        GetComponent<Animator>().ResetTrigger(emotes[0].EmoteAnimationTriggerName);
                        PlayEmote(emotes[0].EmoteAnimationTriggerName, emotes[0].EmoteAnimationLength, emotes[0].isOnlyUpperBodyAnimation);
                    }
                    else if (Down)
                    {
                        isPlayingAnimation = false;
                        StopCoroutine(EndEmote(0, ""));
                        GetComponent<Animator>().ResetTrigger(emotes[1].EmoteAnimationTriggerName);
                        PlayEmote(emotes[1].EmoteAnimationTriggerName, emotes[1].EmoteAnimationLength, emotes[1].isOnlyUpperBodyAnimation);
                    }
                    else if (Right)
                    {
                        isPlayingAnimation = false;
                        StopCoroutine(EndEmote(0, ""));
                        GetComponent<Animator>().ResetTrigger(emotes[2].EmoteAnimationTriggerName);
                        PlayEmote(emotes[2].EmoteAnimationTriggerName, emotes[2].EmoteAnimationLength, emotes[2].isOnlyUpperBodyAnimation);
                    }
                    else if (Left)
                    {
                        isPlayingAnimation = false;
                        StopCoroutine(EndEmote(0, ""));
                        GetComponent<Animator>().ResetTrigger(emotes[3].EmoteAnimationTriggerName);
                        PlayEmote(emotes[3].EmoteAnimationTriggerName, emotes[3].EmoteAnimationLength, emotes[3].isOnlyUpperBodyAnimation);
                    }
                    Left = false;
                    Right = false;
                    Down = false;
                    Top = false;
                }
            }
        }

        public void PlayEmote(string _animationTriggerName, float _animationLength, bool _isOnlyUpperBodyAnimation)
        {
            isPlayingAnimation = true;
            if (!_isOnlyUpperBodyAnimation)
                BlockPlayer(true, false);
            else
                GetComponent<Animator>().SetLayerWeight(2, 1);
            if (hasAuthority)
                CmdPlayEmote(_animationTriggerName, _isOnlyUpperBodyAnimation);

            StartCoroutine(EndEmote(_animationLength, _animationTriggerName));
        }

        [Command]
        void CmdPlayEmote(string _animationTriggerName, bool _isOnlyUpperBodyAnimation)
        {
            RpcPlayEmote(_animationTriggerName, _isOnlyUpperBodyAnimation);
        }

        [ClientRpc]
        void RpcPlayEmote(string _animationTriggerName, bool _isOnlyUpperBodyAnimation)
        {
            if (_isOnlyUpperBodyAnimation)
                GetComponent<Animator>().SetLayerWeight(2, 1);
            GetComponent<Animator>().SetTrigger(_animationTriggerName);
        }

        IEnumerator EndEmote(float _animationLength, string _animationTriggerName)
        {
            yield return new WaitForSeconds(_animationLength);

            GetComponent<Animator>().SetLayerWeight(2, 0);
            isPlayingAnimation = false;
            GetComponent<Animator>().ResetTrigger(_animationTriggerName);
            BlockPlayer(false, false);
            StopCoroutine(EndEmote(0, ""));
        }

        public void CancelAnimation()
        {
            isPlayingAnimation = false;
            GetComponent<Animator>().Play("Idle Walk Run Blend");
            BlockPlayer(false, false);
        }

        void BlockPlayer(bool block, bool blockCamera = true)
        {
            GetComponent<ThirdPersonController>().BlockPlayer(hasAuthority ? block : false, blockCamera);
            GetComponent<CharacterController>().enabled = !block;
        }
    }

    [System.Serializable]
    public class EmoteWheelItem
    {
        public string EmoteName;
        [Space]
        public string EmoteAnimationTriggerName;
        public float EmoteAnimationLength = 1;
        [Space]
        [TextArea]
        public string InfoText;
        [Space]
        public bool isOnlyUpperBodyAnimation = false;
        [Space]
        [Header("Ui")]
        public Sprite EmoteIcon;
        public UnityEngine.UI.Image EmoteButtonImage;
        public UnityEngine.UI.Image EmoteButtonIconImage;
        public Color ButtonDeselectedColor;
        public Color ButtonSelectedColor;

        public void Load()
        {
            EmoteButtonIconImage.sprite = EmoteIcon;
        }

        public void Deselect()
        {
            EmoteButtonImage.color = ButtonDeselectedColor;
        }

        public void Select()
        {
            EmoteButtonImage.color = ButtonSelectedColor;
        }
    }
}