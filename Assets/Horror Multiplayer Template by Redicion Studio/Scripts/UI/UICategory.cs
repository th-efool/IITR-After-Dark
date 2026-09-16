// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace RedicionStudio
{
    public class UICategory : MonoBehaviour
    {

        [SerializeField] private GameObject _categoryOptionPrefab;
        [SerializeField] private GameObject _categoryItemPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private Transform _itemContent;
        [SerializeField] private Color _color;
        [SerializeField] private Color _selectedColor;
        public TextMeshProUGUI headerText;
        public UIButton returnButton;

        private struct Option
        {

            public GameObject gO;
            public bool item;
        }

        private Dictionary<string, Option> _options = new Dictionary<string, Option>();

        public System.Action<string> onOptionSelect;
        public System.Action<BSystem.PlaceableSO> onItemSelect;

        public void ClearOptions()
        {
            foreach (Option option in _options.Values)
            {
                Destroy(option.gO);
            }
            _options.Clear();
        }

        public void SelectOption(string categoryName)
        {
            if (categoryName == null)
            {
                foreach (Option option in _options.Values)
                {
                    option.gO.GetComponent<Image>().color = _color;
                }
                return;
            }

            if (_options.TryGetValue(categoryName, out Option value) && !value.item)
            {
                foreach (Option option in _options.Values)
                {
                    option.gO.GetComponent<Image>().color = _color;
                }
                _options[categoryName].gO.GetComponent<Image>().color = _selectedColor;
                onOptionSelect.Invoke(categoryName);
                return;
            }

            onItemSelect.Invoke(BSystem.PlaceableSO.GetPlaceableSO(categoryName));
        }

        public void AddOption(string categoryName, BSystem.PlaceableSO placeableSO)
        {
            GameObject gO;
            if (placeableSO == null)
            {
                gO = Instantiate(_categoryOptionPrefab);
                gO.GetComponent<Image>().color = _color;
                gO.GetComponentInChildren<TextMeshProUGUI>().text = categoryName;
                gO.transform.SetParent(_content, false);
            }
            else
            {
                gO = Instantiate(_categoryItemPrefab);
                gO.GetComponentInChildren<Image>().sprite = placeableSO.sprite;
                gO.GetComponentsInChildren<TextMeshProUGUI>()[0].text = placeableSO.uniqueName;
                gO.GetComponentsInChildren<TextMeshProUGUI>()[1].text = placeableSO.price + "$";
                gO.transform.SetParent(_itemContent, false);
            }
            gO.name = categoryName;
            gO.GetComponent<UIButton>().onPressed = () => { SelectOption(categoryName); };
            _options.Add(categoryName, new Option { gO = gO, item = placeableSO != null });
        }

        private static string EmptySpace(int length)
        {
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                result += ' ';
            }
            return result;
        }

        public void AddOption(MasterServer.InstanceInfo instanceInfo)
        {
            GameObject gO = Instantiate(_categoryOptionPrefab, _content);
            gO.GetComponent<Image>().color = _color;
            foreach (Transform child in gO.transform)
            {
                if (child.gameObject.name == "ServerNameText")
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().text = instanceInfo.uniqueName;
                }
                if (child.gameObject.name == "ServerPlayerCountText")
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().text = "Players: " + instanceInfo.numberOfPlayers + "/5";
                }
                if (child.gameObject.name == "ServerPingText")
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().text = "Ping: " + instanceInfo.ping;
                }
                /*if (child.gameObject.name == "MapNameText")
                {
                    child.GetComponentInChildren<TextMeshProUGUI>().text = "Map: " + "Forgotten Refuge";
                }*/
            }
            /*gO.GetComponentInChildren<TextMeshProUGUI>().text = instanceInfo.uniqueName + EmptySpace(72 - instanceInfo.uniqueName.Length) +
                "Players: " + instanceInfo.numberOfPlayers + "/16" + EmptySpace(72 - instanceInfo.numberOfPlayers.ToString().Length) +
                "Ping: " + instanceInfo.ping;*/
            gO.GetComponent<UIButton>().onPressed = () => { SelectOption(instanceInfo.uniqueName); };
            _options.Add(instanceInfo.uniqueName, new Option { gO = gO, item = false });
        }
    }
}
