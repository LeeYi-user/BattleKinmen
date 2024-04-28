using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor;
    private bool translating;

    private void Update()
    {
        if (GameManager.Instance.popups.Contains(null))
        {
            Destroy(gameObject);
        }

        if (text.text == "")
        {
            return;
        }

        TextFade();

        if (translating)
        {
            return;
        }
        
        StartCoroutine(Translate(new Vector3(0, 75, 0), 0.25f));
    }

    private void TextFade()
    {
        if (pause)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            if (phase == 0)
            {
                Color color = text.color;
                color.a = 1;
                targetColor = color;
                timeLeft = 0.25f;
            }
            else if (phase == 1)
            {
                text.color = targetColor;
                StartCoroutine(Pause(1.5f));
            }
            else if (phase == 2)
            {
                Color color = text.color;
                color.a = 0;
                targetColor = color;
                timeLeft = 0.25f;
            }
            else if (phase == 3)
            {
                if (GameManager.Instance.popups.Contains(text.text))
                {
                    GameManager.Instance.popups.Clear();
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

    private IEnumerator Translate(Vector3 deltaPosition, float duration)
    {
        translating = true;

        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = startPosition + deltaPosition;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, t / duration);
            yield return null;
        }

        transform.localPosition = endPosition;
    }
}
