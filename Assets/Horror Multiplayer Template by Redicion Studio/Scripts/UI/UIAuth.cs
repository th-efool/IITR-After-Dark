// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using RedicionStudio.MasterServer;
using TMPro;

namespace RedicionStudio
{
    public class UIAuth : MonoBehaviour
    {

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
        [Header("Content")]
        [SerializeField] private GameObject _registrationContent;
        [SerializeField] private GameObject _loginContent;
        [SerializeField] private GameObject _loadingContent;

        private GameObject _lastContent;

        [Header("Registration")]
        [SerializeField] private TMP_InputField _rUsernameIF;
        [SerializeField] private TMP_InputField _rEmailIF;
        [SerializeField] private TMP_InputField _rPasswordIF;
        [Space]
        [SerializeField] private UIButton _rContinueButton;
        [SerializeField] private UIButton _toLTextButton;

        [Header("Login")]
        [SerializeField] private TMP_InputField _lEmailIF;
        [SerializeField] private TMP_InputField _lPasswordIF;
        [Space]
        [SerializeField] private UIButton _lContinueButton;
        [SerializeField] private UIButton _toRTextButton;

        private void ChangeContent(GameObject gO)
        {
            _registrationContent.SetActive(false);
            _loginContent.SetActive(false);
            _loadingContent.SetActive(false);
            if (gO != null)
            {
                gO.SetActive(true);
            }
        }

        private void Awake()
        {
            ChangeContent(_loginContent);
            MSClient.OnStateChanged += () => {
                switch (MSClient.State)
                {
                    case MSClient.NetworkState.Idle:
                        ChangeContent(_lastContent != null ? _lastContent : _loginContent);
                        break;
                    case MSClient.NetworkState.Pending:
                        ChangeContent(_loadingContent);
                        break;
                    case MSClient.NetworkState.Lobby:
                        ChangeContent(null);
                        break;
                }
            };

            MSClient.OnAuthResponse += (code, token) => {
                if (code != 100 && code != 101)
                {
                    switch (code)
                    {
                        case 202:
                            UIPopup.instance.Show("Outdated version.");
                            break;
                        case 203:
                            UIPopup.instance.Show("Wrong credentials.");
                            break;
                        case 205:
                            UIPopup.instance.Show("This account is already in use.");
                            break;
                        case 207:
                            UIPopup.instance.Show("Account email or name is already in use.");
                            break;
                        case 212:
                            UIPopup.instance.Show("Email is not valid.");
                            break;
                        case 213:
                            UIPopup.instance.Show("Password is not valid.");
                            break;
                        case 214:
                            UIPopup.instance.Show("Username is not valid.");
                            break;
                    }
                }
                else
                {
                    // !
                    //UIPopup.instance.Show("Token: " + token + '.');
                }
            };

            MSClient.OnConnectionFailed += () => {
                UIPopup.instance.Show("Failed to connect to the master server.");
            };

            _rContinueButton.onPressed = () => {
                MSClient.localAuthRequestType = AuthRequestType.AccountCreation;
                MSClient.localUsername = _rUsernameIF.text;
                MSClient.localEmail = _rEmailIF.text;
                MSClient.localPassword = _rPasswordIF.text;
                _lEmailIF.text = _rEmailIF.text;
                _lPasswordIF.text = _rPasswordIF.text;
                FindObjectOfType<MSClient>().Connect();
            };

            _lContinueButton.onPressed = () => {
                MSClient.localAuthRequestType = AuthRequestType.Authorization;
                MSClient.localEmail = _lEmailIF.text;
                MSClient.localPassword = _lPasswordIF.text;
                FindObjectOfType<MSClient>().Connect();
            };

            _toLTextButton.onPressed = () => {
                UIFadeImage.instance.Show(1.0f, () => { ChangeContent(_loginContent); UIFadeImage.instance.Hide(); });
                _lastContent = _loginContent;
            };

            _toRTextButton.onPressed = () => {
                UIFadeImage.instance.Show(1.0f, () => { ChangeContent(_registrationContent); UIFadeImage.instance.Hide(); });
                _lastContent = _registrationContent;
            };
        }
#endif
    }
}