using UnityEngine;
using UnityEngine.InputSystem;

namespace RedicionStudio
{
    public class PlayerInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool aim;
        public bool toggleCamera;
        public bool interact;
        public bool weaponWheel;
        public bool emoteWheel;
        public bool use;
        public bool activateHunterAbility1;
        public bool activateHunterAbility2;
        public bool activateHunterAbility3;
        public bool climb;
        public bool inventorySlot0;
        public bool inventorySlot1;
        public bool inventorySlot2;
        public bool inventorySlot3;
        public bool dropItem;
        public bool fight;
        public bool nextSlot;
        public bool previousSlot;
        public bool pauseMenu;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public PlayerInput playerInput;
        public GameObject settingsWindow;

        private bool inputIsActive = true;

        public bool gamepadConnected
        {
            get
            {
                return Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
            }
        }

        private void Update()
        {
            if (settingsWindow == null)
            {
                var settingsManager = GameObject.FindGameObjectWithTag("GameSettingsManager");
                if (settingsManager != null)
                {
                    settingsWindow = settingsManager.transform.GetChild(1).gameObject;
                }
            }

            if (playerInput == null)
            {
                var inputManager = GameObject.FindGameObjectWithTag("InputManager");
                if (inputManager != null)
                {
                    playerInput = inputManager.GetComponent<PlayerInput>();
                }
            }

            if (settingsWindow == null || playerInput == null)
            {
                return;
            }

            if (settingsWindow.activeSelf && inputIsActive)
            {
                playerInput.DeactivateInput();
                inputIsActive = false;
            }
            else if (!settingsWindow.activeSelf && !inputIsActive)
            {
                playerInput.ActivateInput();
                inputIsActive = true;
            }
        }

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        /*public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }*/

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }

        public void OnWeaponWheel(InputValue value)
        {
            WeaponWheelInput(value.isPressed);
        }

        public void OnEmoteWheel(InputValue value)
        {
            EmoteWheelInput(value.isPressed);
        }

        public void OnUse(InputValue value)
        {
            UseInput(value.isPressed);
        }

        public void OnToggleCamera(InputValue value)
        {
            ToggleCameraInput(value.isPressed);
        }

        public void OnClimb(InputValue value)
        {
            ClimbInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnActivateHunterAbility1(InputValue value)
        {
            ActivateHunterAbility1Input(value.isPressed);
        }

        public void OnActivateHunterAbility2(InputValue value)
        {
            ActivateHunterAbility2Input(value.isPressed);
        }

        public void OnActivateHunterAbility3(InputValue value)
        {
            ActivateHunterAbility3Input(value.isPressed);
        }

        public void OnFight(InputValue value)
        {
            FightInput(value.isPressed);
        }

        public void OnInventorySlot0(InputValue value)
        {
            InventorySlot0Input(value.isPressed);
        }

        public void OnInventorySlot1(InputValue value)
        {
            InventorySlot1Input(value.isPressed);
        }

        public void OnInventorySlot2(InputValue value)
        {
            InventorySlot2Input(value.isPressed);
        }

        public void OnInventorySlot3(InputValue value)
        {
            InventorySlot3Input(value.isPressed);
        }

        public void OnDropItem(InputValue value)
        {
            DropItemInput(value.isPressed);
        }

        public void OnNextSlot(InputValue value)
        {
            NextSlotInput(value.isPressed);
        }

        public void OnPreviousSlot(InputValue value)
        {
            PreviousSlotInput(value.isPressed);
        }

        public void OnPauseMenu(InputValue value)
        {
            PauseMenuInput(value.isPressed);
        }

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        public void WeaponWheelInput(bool newWeaponWheelState)
        {
            weaponWheel = newWeaponWheelState;
        }

        public void EmoteWheelInput(bool newEmoteWheelState)
        {
            emoteWheel = newEmoteWheelState;
        }

        public void UseInput(bool newUseState)
        {
            use = newUseState;
        }

        public void ToggleCameraInput(bool newToggleCameraState)
        {
            toggleCamera = newToggleCameraState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void ClimbInput(bool newClimbState)
        {
            climb = newClimbState;
        }

        public void ActivateHunterAbility1Input(bool newActivateHunterAbility1State)
        {
            activateHunterAbility1 = newActivateHunterAbility1State;
        }

        public void ActivateHunterAbility2Input(bool newActivateHunterAbility2State)
        {
            activateHunterAbility2 = newActivateHunterAbility2State;
        }

        public void ActivateHunterAbility3Input(bool newActivateHunterAbility3State)
        {
            activateHunterAbility3 = newActivateHunterAbility3State;
        }

        public void FightInput(bool newFightState)
        {
            fight = newFightState;
        }

        public void InventorySlot0Input(bool newInventorySlot0State)
        {
            inventorySlot0 = newInventorySlot0State;
        }

        public void InventorySlot1Input(bool newInventorySlot1State)
        {
            inventorySlot1 = newInventorySlot1State;
        }

        public void InventorySlot2Input(bool newInventorySlot2State)
        {
            inventorySlot2 = newInventorySlot2State;
        }

        public void InventorySlot3Input(bool newInventorySlot3State)
        {
            inventorySlot3 = newInventorySlot3State;
        }

        public void DropItemInput(bool newDropItemState)
        {
            dropItem = newDropItemState;
        }

        public void NextSlotInput(bool newNextSlotState)
        {
            nextSlot = newNextSlotState;
        }

        public void PreviousSlotInput(bool newPreviousSlotState)
        {
            previousSlot = newPreviousSlotState;
        }

        public void PauseMenuInput(bool newPauseMenuState)
        {
            pauseMenu = newPauseMenuState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}