using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
     
    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "GameScene";

    public static StickyAlmanacManager Instance { get; private set; }

    private Dictionary<string, GameObject> menuMappings;
    private List<GameObject> allMenus;
    private GameObject currentMenu;
    private GameObject previousMenu;

    public Stack<GameObject> menuHistory;

    private int maxHistorySize = 3;

    private void Awake()
    {
        // Remove DontDestroyOnLoad for almanac manager
        if (Instance == null)
        {
            Instance = this;
            InitializeSystem();
        }
        else
        {
            // If there's already an instance in this scene, destroy this one
            Destroy(gameObject);
        }
    }

    private void InitializeSystem()
    {
        InitializeMenus();
        InitializeMenuMappings();
        InitializeNavigation();
        HideAll();
        ShowMenu(topBackground);
    }

    private void InitializeNavigation()
    {
        menuHistory = new Stack<GameObject>();
        currentMenu = null;
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
            {"emailBotButton", emailBot},
            {"homeButton", topBackground}
        };
    }

    public void SelectMenu(GameObject button)
    {
        if (button == null)
        {
            Debug.LogWarning("Button is null in SelectMenu");
            return;
        }
        if (button.name == "goBackButton")
            {
                GoBack();
            }
        else
        {
            if (menuMappings.TryGetValue(button.name, out GameObject targetMenu))
            {
                NavigateToMenu(targetMenu);
            }
            else
            {
                Debug.LogWarning($"No menu mapping found for button: {button.name}");
                NavigateToMenu(topBackground);
            }
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

     private void NavigateToMenu(GameObject targetMenu)
    {
        if (targetMenu == null) return;
        
        // Don't navigate to the same menu
        if (currentMenu == targetMenu) return;

        // Add current menu to history before switching (if not null)
        if (currentMenu != null)
        {
            menuHistory.Push(currentMenu);
            
            // Limit history size to prevent memory issues
            if (menuHistory.Count > maxHistorySize)
            {
                var tempStack = new Stack<GameObject>();
                for (int i = 0; i < maxHistorySize; i++)
                {
                    if (menuHistory.Count > 0)
                        tempStack.Push(menuHistory.Pop());
                }
                menuHistory.Clear();
                while (tempStack.Count > 0)
                {
                    menuHistory.Push(tempStack.Pop());
                }
            }
        }

        // Switch to new menu
        HideAll();
        ShowMenu(targetMenu);
        currentMenu = targetMenu;
    }

    public void GoBack()
    {
        if (menuHistory.Count > 0)
        {
            GameObject previousMenu = menuHistory.Pop();
            
            // Navigate without adding to history (to avoid infinite back-forward loops)
            HideAll();
            ShowMenu(previousMenu);
            currentMenu = previousMenu;
        }
        else
        {
            // No history available, go to main menu
            Debug.Log("No previous menu in history, going to main menu");
            NavigateToMenu(topBackground);
        }
    }

    public void ReturnToGame()
    {
        Time.timeScale = 1f;
        var codexScene = gameObject.scene;
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(codexScene);
    }


    private void OnDestroy()
    {
        // Clear the singleton when this object is destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
}
