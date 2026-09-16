// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class KillerUIItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Killer UI Item Button")]
        public RedicionStudio.InventorySystem.ItemSO itemSO;
        public string uniqueName;
        public int killerID;
        public int price;
        public Sprite killerSprite;
        [Space, TextArea(1, 32)]
        public string description;
        [Space]
        [HideInInspector] public TMPro.TMP_Text descriptionText;
        public TMPro.TMP_Text nameText;
        public TMPro.TMP_Text priceText;
        public GameObject priceTextContent;
        public UnityEngine.UI.Image killerImage;
        [Space]

        [Header("Events")]
        public UnityEvent onPointerEnterEvent;
        public UnityEvent onPointerExitEvent;
        public UnityEvent onPointerClickEvent;
        [Space]

        [Header("Dependencies")]
        [HideInInspector] public KillerSelectorManager killerSelectorManager;
        [HideInInspector] public GameObject localPlayer;

        public GameObject buttonHoverGameObject;
        public GameObject buttonSelectedGameObject;

        [HideInInspector] public bool isItemInInventory = false;

        [Header("Preview Model")]
        public GameObject previewModelPrefab;
        public GameObject instantiatedPreviewModel;
        public Transform previewModelPosition;

        public void SetUpItem()
        {
            //descriptionText.text = description;
            nameText.text = uniqueName;
            priceText.text = price.ToString();
            killerImage.sprite = killerSprite;
            GetComponent<RectTransform>().localScale = Vector3.one;
            previewModelPosition = GameObject.FindGameObjectWithTag("KillerSelectionPreviewModelPosition").transform;

            killerSelectorManager.CheckItemInInventory(killerID, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            buttonHoverGameObject.SetActive(true);

            if (instantiatedPreviewModel == null)
            {
                if (killerSelectorManager.instantiatedKillerPreviewModels.Count > 0)
                {
                    foreach (GameObject model in killerSelectorManager.instantiatedKillerPreviewModels)
                    {
                        Destroy(model);
                    }

                    killerSelectorManager.instantiatedKillerPreviewModels.Clear();
                }

                instantiatedPreviewModel = Instantiate(previewModelPrefab, previewModelPosition);
                killerSelectorManager.instantiatedKillerPreviewModels.Add(instantiatedPreviewModel);
            }
            descriptionText.text = description;

            onPointerEnterEvent.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonHoverGameObject.SetActive(false);

            onPointerExitEvent.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            killerSelectorManager.CheckItemInInventory(killerID, true);

            onPointerClickEvent.Invoke();
        }

        public void CheckItem()
        {
            if (isItemInInventory)
            {
                priceTextContent.SetActive(false);
            }
            if (localPlayer.GetComponent<Player>().killerId == killerID)
            {
                SelectKiller();
            }
        }

        public void SelectKiller()
        {
            priceTextContent.SetActive(false);
            foreach (KillerUIItemButton uIButton in killerSelectorManager.killerSelectionButtons)
            {
                uIButton.buttonSelectedGameObject.SetActive(false);
            }
            buttonSelectedGameObject.SetActive(true);
            if (instantiatedPreviewModel == null)
            {
                instantiatedPreviewModel = Instantiate(previewModelPrefab, previewModelPosition);
                killerSelectorManager.instantiatedKillerPreviewModels.Add(instantiatedPreviewModel);
            }
            descriptionText.text = description;
        }
    }
}