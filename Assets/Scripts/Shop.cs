using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Shop : NetworkBehaviour
{
    public static Shop Instance;

    [HideInInspector] public NetworkVariable<int> teamCash = new NetworkVariable<int>(1000, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameObject shopMenu;

    [Header("Categories")]
    public Image bodyCategory;
    public Image weaponCategory;
    public Image skillCategory;
    public Image teamCategory;

    [Header("Areas")]
    public GameObject bodyArea;
    public GameObject weaponArea;
    public GameObject skillArea;
    public GameObject teamArea;

    [System.Serializable]
    private struct Item
    {
        public string name;
        public ShopItem shopItem;
    }

    [Header("In Container")]
    [SerializeField] private Item[] _items;
    public Dictionary<string, ShopItem> items = new();

    [Header("Other")]
    public TextMeshProUGUI cashCounter;
    public Button backButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (Item item in _items)
        {
            items[item.name] = item.shopItem;
        }
    }

    private void Update()
    {
        if (MainSceneManager.Instance.start < 2 || MainSceneManager.Instance.gameover || MainSceneManager.disconnecting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!shopMenu.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                MainSceneManager.Instance.gamingScreen.SetActive(false);
                shopMenu.SetActive(true);
            }
            else
            {
                BackButtonClick();
            }
        }
    }

    public void BodyCategoryClick()
    {
        float[] colors = { 1, 0, 0, 0 };
        SwitchCategories(colors);
    }

    public void WeaponCategoryClick()
    {
        float[] colors = { 0, 1, 0, 0 };
        SwitchCategories(colors);
    }

    public void SkillCategoryClick()
    {
        float[] colors = { 0, 0, 1, 0 };
        SwitchCategories(colors);
    }

    public void TeamCategoryClick()
    {
        float[] colors = { 0, 0, 0, 1 };
        SwitchCategories(colors);
    }

    public void SwitchCategories(float[] colors)
    {
        bodyCategory.color = new Color(166f / 255f, 112f / 255f, 78f / 255f, colors[0]);
        weaponCategory.color = new Color(166f / 255f, 112f / 255f, 78f / 255f, colors[1]);
        skillCategory.color = new Color(166f / 255f, 112f / 255f, 78f / 255f, colors[2]);
        teamCategory.color = new Color(166f / 255f, 112f / 255f, 78f / 255f, colors[3]);

        bool[] actives = { false, false, false, false };

        for (int i = 0; i < 4; i++)
        {
            if (colors[i] == 1)
            {
                actives[i] = true;
                break;
            }
        }

        SwitchAreas(actives);
    }

    public void SwitchAreas(bool[] actives)
    {
        bodyArea.SetActive(actives[0]);
        weaponArea.SetActive(actives[1]);
        skillArea.SetActive(actives[2]);
        teamArea.SetActive(actives[3]);
    }

    public void BackButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainSceneManager.Instance.gamingScreen.SetActive(true);
        shopMenu.SetActive(false);
    }
}
