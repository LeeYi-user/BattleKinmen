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
    private int phase;
    private bool pause;

    private void Start()
    {
        targetColor = new Color(1, 1, 1, 1);
        timeLeft = 0.25f;
    }

    private void Update()
    {
        if (text.text == "")
        {
            return;
        }

        if (!translating)
        {
            translating = true;
            StartCoroutine(Translate(new Vector3(0, 75, 0), 0.25f));
        }

        TextFade();
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

    private void TextFade()
    {
        if (pause)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            text.color = targetColor;

            if (phase == 0)
            {
                StartCoroutine(Pause(0.5f));
            }
            else if (phase == 1)
            {
                targetColor = new Color(1, 1, 1, 0);
                timeLeft = 0.25f;
            }
            else if (phase == 2)
            {
                if (MainSceneManager.Instance.popups.Contains(text.text))
                {
                    MainSceneManager.Instance.popups.Clear();
                }
                
                Destroy(gameObject);
            }

            phase++;
        }
        else
        {
            text.color = Color.Lerp(text.color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }

    private IEnumerator Pause(float seconds)
    {
        pause = true;
        yield return new WaitForSeconds(seconds);
        pause = false;
    }
}
