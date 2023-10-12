using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    public Image healthBar;
    public float maxHealth = 100f;
    public float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        healthBar = GameObject.Find("Health").GetComponent<Image>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // Test
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthBar)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}
