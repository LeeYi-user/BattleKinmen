using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Enter_start_area : MonoBehaviour
{
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        // �ھڶ���H����ܥX���I
        if (InitScene.team == "Red")
        {
            // ���a�ݩ�����A�N��ͦ��b�����X���I
            Debug.Log("����");
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-5f, 5f);
            pos = new Vector3(x, 2, y);
            transform.position = pos;
        }
        else if (InitScene.team == "Blue")
        {
            // ���a�ݩ��Ŷ��A�N��ͦ��b�Ŷ��X���I
            Debug.Log("����");
            float x = Random.Range(15f, 25f);
            float y = Random.Range(-5f, 5f);
            pos = new Vector3(x, 2, y);
            transform.position = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}