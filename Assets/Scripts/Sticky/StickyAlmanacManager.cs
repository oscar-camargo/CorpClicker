using UnityEngine;
using System.Collections.Generic;

public class StickyAlmanacManager : MonoBehaviour
{
    
    [Header("Menus")]
    [SerializeField] private GameObject topBackground;
    [SerializeField] private GameObject LoreBackground;
    [SerializeField] private GameObject MechanicsBackground;
    [SerializeField] private GameObject UpgradesBackground;
    [SerializeField] private GameObject CompanyLoreBackground;
    [SerializeField] private GameObject YouLoreBackground;
    [SerializeField] private GameObject kpiLoreBackground;
    [SerializeField] private GameObject clickingMechanicsBackground;
    [SerializeField] private GameObject moraleMechanicsBackground;
    [SerializeField] private GameObject reputationMechanicsBackground;
    [SerializeField] private GameObject mailingMechanicsBackground;
    [SerializeField] private GameObject calendarPadding;
    [SerializeField] private GameObject emailSignature;
    [SerializeField] private GameObject stapleStorm3000;
    [SerializeField] private GameObject emailAttachments;
    [SerializeField] private GameObject ergonomicMouse;
    [SerializeField] private GameObject emailBot;
    [SerializeField] private GameObject AchievementsBackground;

    public static StickyAlmanacManager Instance { get; private set; }

    private Dictionary<string, GameObject> menuMappings;
    private List<GameObject> allMenus;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSystem()
    {
        InitializeMenus();
        InitializeMenuMappings();
        HideAll();
        ShowMenu(topBackground);
    }

    private void InitializeMenus()
    {
        allMenus = new List<GameObject>
        {
            topBackground,
            LoreBackground,
            MechanicsBackground,
            UpgradesBackground,
            AchievementsBackground,
            CompanyLoreBackground,
            YouLoreBackground,
            kpiLoreBackground,
            clickingMechanicsBackground,
            moraleMechanicsBackground,
            reputationMechanicsBackground,
            mailingMechanicsBackground,
            calendarPadding,
            emailSignature,
            stapleStorm3000,
            emailAttachments,
            ergonomicMouse,
            emailBot
        };
    }

    private void InitializeMenuMappings()
    {
        menuMappings = new Dictionary<string, GameObject>
        {
            {"mainBackground", topBackground},
            {"LoreButton", LoreBackground},
            {"MechanicsButton", MechanicsBackground},
            {"UpgradesButton", UpgradesBackground},
            {"AchievementsButton", AchievementsBackground},
            {"companyButton", CompanyLoreBackground},
            {"youButton", YouLoreBackground},
            {"whatsKPIbutton", kpiLoreBackground},
            {"Clicking", clickingMechanicsBackground},
            {"moraleButton", moraleMechanicsBackground},
            {"reputationButton", reputationMechanicsBackground},
            {"mailsbutton", mailingMechanicsBackground},
            {"paddingButton", calendarPadding},
            {"emailSignatureButton", emailSignature},
            {"stapleButton", stapleStorm3000},
            {"attachmentsbutton", emailAttachments},
            {"ergonomicbutton", ergonomicMouse},
            {"emailBotButton", emailBot}
        };
    }

    public void selectMenu(GameObject button)
    {
        HideAll();
        if (menuMappings.TryGetValue(button.name, out GameObject targetMenu))
        {
            ShowMenu(targetMenu);
        }
        else
        {
            Debug.LogWarning($"No menu mapping found for button: {button.name}");
            // Fallback to main menu
            ShowMenu(topBackground);
        }
    }

    public void HideAll()
    {
        foreach (GameObject menu in allMenus) {
            menu.SetActive(false);
        };
    }

    public void ShowMenu(GameObject menu)
    {
        menu.SetActive(true);
    }
}
