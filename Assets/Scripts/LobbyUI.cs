using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LobbyUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI modeText;

    public void OnPointerClick(PointerEventData eventData)
    {
        nameText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
        countText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
        modeText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color(221f / 255f, 180f / 255f, 151f / 255f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color(203f / 255f, 150f / 255f, 112f / 255f);
    }
}
