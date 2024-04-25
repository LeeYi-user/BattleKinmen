using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LobbyUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string id;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI modeText;

    private void Update()
    {
        if (MenuManager.Instance.selectedLobbyId == id)
        {
            nameText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
            countText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
            modeText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
        }
        else
        {
            nameText.color = new Color(1f, 1f, 1f);
            countText.color = new Color(1f, 1f, 1f);
            modeText.color = new Color(1f, 1f, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MenuManager.Instance.selectedLobbyId != id)
        {
            MenuManager.Instance.selectedLobbyId = id;
        }
        else
        {
            MenuManager.Instance.selectedLobbyId = null;
        }
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
