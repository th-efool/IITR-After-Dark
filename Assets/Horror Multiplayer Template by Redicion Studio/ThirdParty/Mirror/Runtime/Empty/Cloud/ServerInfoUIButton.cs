using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ServerInfoUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Events")]
    public UnityEvent onPointerEnterEvent;
    public UnityEvent onPointerExitEvent;
    public UnityEvent onPointerClickEvent;

    [Space]
    public GameObject[] buttonHoverGameObjects;
    public GameObject[] buttonSelectedGameObjects;

    private Transform contentParent;

    private void Start()
    {
        contentParent = transform.parent;
    }

    public void Deselect()
    {
        foreach (GameObject element in buttonSelectedGameObjects)
            element.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach(GameObject element in buttonHoverGameObjects)
            element.SetActive(true);

        onPointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (GameObject element in buttonHoverGameObjects)
            element.SetActive(false);

        onPointerExitEvent.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (Transform child in contentParent)
        {
            ServerInfoUIButton entry = child.GetComponent<ServerInfoUIButton>();
            if (entry != null && entry != this)
            {
                entry.Deselect();
            }
        }

        foreach (GameObject element in buttonSelectedGameObjects)
            element.SetActive(true);

        onPointerClickEvent.Invoke();
    }
}
