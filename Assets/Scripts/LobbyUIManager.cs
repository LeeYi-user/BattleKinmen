using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Click : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI modeText;

    private void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color(221f / 255f, 180f / 255f, 151f / 255f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color(203f / 255f, 150f / 255f, 112f / 255f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        nameText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
        countText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
        modeText.color = new Color(125f / 255f, 57f / 255f, 58f / 255f);
    }
}
