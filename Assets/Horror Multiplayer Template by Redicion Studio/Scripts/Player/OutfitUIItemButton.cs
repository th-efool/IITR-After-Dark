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
    public class OutfitUIItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Outfit UI Item Button")]
        public RedicionStudio.InventorySystem.ItemSO itemSO;
        public string uniqueName;
        public int outfitID;
        public int price;
        public Sprite outfitSprite;
        [Space, TextArea(1, 32)]
        public string description;
        [Space]
        [HideInInspector] public TMPro.TMP_Text descriptionText;
        public TMPro.TMP_Text nameText;
        public TMPro.TMP_Text priceText;
        public GameObject priceTextContent;
        public UnityEngine.UI.Image outfitImage;
        [Space]

        [Header("Events")]
        public UnityEvent onPointerEnterEvent;
        public UnityEvent onPointerExitEvent;
        public UnityEvent onPointerClickEvent;
        [Space]

        [Header("Dependencies")]
        [HideInInspector] public OutfitManager outfitManager;
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
            outfitImage.sprite = outfitSprite;
            GetComponent<RectTransform>().localScale = Vector3.one;
            GameObject _localPlayer;

            _localPlayer = outfitManager.GetLocalPlayer();

            previewModelPosition = _localPlayer.transform;

            outfitManager.CheckItemInInventory(outfitID, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            buttonHoverGameObject.SetActive(true);

            if (instantiatedPreviewModel == null)
            {
                if (outfitManager.instantiatedOutfitPreviewModels.Count > 0)
                {
                    foreach (GameObject model in outfitManager.instantiatedOutfitPreviewModels)
                    {
                        Destroy(model);
                    }

                    outfitManager.instantiatedOutfitPreviewModels.Clear();
                }

                outfitManager.TogglePlayerModel(false);

                instantiatedPreviewModel = Instantiate(previewModelPrefab, previewModelPosition);
                outfitManager.instantiatedOutfitPreviewModels.Add(instantiatedPreviewModel);
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
            outfitManager.CheckItemInInventory(outfitID, true);

            onPointerClickEvent.Invoke();
        }

        public void CheckItem()
        {
            if (isItemInInventory)
            {
                priceTextContent.SetActive(false);
            }
            if (localPlayer.GetComponent<Player>().outfitId == outfitID)
            {
                SelectOutfit();
            }
        }

        public void SelectOutfit()
        {
            GameObject _localPlayer;

            _localPlayer = outfitManager.GetLocalPlayer();

            previewModelPosition = _localPlayer.transform;

            priceTextContent.SetActive(false);
            foreach (OutfitUIItemButton uIButton in outfitManager.outfitSelectionButtons)
            {
                uIButton.buttonSelectedGameObject.SetActive(false);
            }
            buttonSelectedGameObject.SetActive(true);
            descriptionText.text = description;
            if (outfitManager.instantiatedOutfitPreviewModels.Count > 0)
            {
                foreach (GameObject model in outfitManager.instantiatedOutfitPreviewModels)
                {
                    Destroy(model);
                }

                outfitManager.instantiatedOutfitPreviewModels.Clear();
            }
            outfitManager.ShowOutfit(outfitID);
        }
    }
}