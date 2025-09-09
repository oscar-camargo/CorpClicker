using UnityEngine;
using System.Collections.Generic;

public class StickyAlmanac : MonoBehaviour
{

    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject topBackground;
    [SerializeField] private GameObject LoreButton;
    [SerializeField] private GameObject MechanicsButton;
    [SerializeField] private GameObject UpgradesButton;
    [SerializeField] private GameObject AchievementsButton;
    [SerializeField] private GameObject LoreBackground;
    [SerializeField] private GameObject CompanyLoreButton;
    [SerializeField] private GameObject YouLoreButton;
    [SerializeField] private GameObject kpiLoreButton;
    [SerializeField] private GameObject MechanicsBackground;
    [SerializeField] private GameObject clickingMechanicsButton;
    [SerializeField] private GameObject moraleMechanicsButton;
    [SerializeField] private GameObject reputationMechanicsButton;
    [SerializeField] private GameObject mailingMechanicsButton;
    [SerializeField] private GameObject UpgradesBackground;
    [SerializeField] private GameObject calendarPadding;
    [SerializeField] private GameObject emailSignature;
    [SerializeField] private GameObject stapleStorm3000;
    [SerializeField] private GameObject emailAttachments;
    [SerializeField] private GameObject ergonomicMouse;
    [SerializeField] private GameObject emailBot;

    private List<GameObject> allButtons;

    private void Start()
    {
        allButtons = new List<GameObject>
    {
        backButton,
        topBackground,
        LoreButton,
        MechanicsButton,
        UpgradesButton,
        AchievementsButton,
        LoreBackground,
        CompanyLoreButton,
        YouLoreButton,
        kpiLoreButton,
        MechanicsBackground,
        clickingMechanicsButton,
        moraleMechanicsButton,
        reputationMechanicsButton,
        mailingMechanicsButton,
        UpgradesBackground,
        calendarPadding,
        emailSignature,
        stapleStorm3000,
        emailAttachments,
        ergonomicMouse,
        emailBot,
    };
}

    public void HideAll()
    {
        foreach (var button in allButtons) {
            button.SetActive(false);
        };
    }

    public void ShowMenu(GameObject menu)
    {

    }
}
