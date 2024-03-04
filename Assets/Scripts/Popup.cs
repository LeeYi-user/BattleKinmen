using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI text;

    private bool translating;
    private Color targetColor;
    private float timeLeft;

    // Start is called before the first frame update
    void Start()
    {
        targetColor = new Color(1, 1, 1, 1);
        timeLeft = 0.25f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!translating)
        {
            translating = true;
            StartCoroutine(Translate(new Vector3(0, 75, 0), 0.25f));
        }

        if (timeLeft <= Time.deltaTime)
        {
            text.color = targetColor;
            Destroy(gameObject, 0.75f);
        }
        else
        {
            text.color = Color.Lerp(text.color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }

    private IEnumerator Translate(Vector3 deltaPosition, float duration)
    {
        Vector3 startPosition = rectTransform.localPosition;
        Vector3 endPosition = startPosition + deltaPosition;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, t / duration);
            yield return null;
        }

        rectTransform.localPosition = endPosition;
    }
}
