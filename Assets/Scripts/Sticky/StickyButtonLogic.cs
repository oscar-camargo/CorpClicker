using UnityEngine;
using System.Collections.Generic;

public class StickyButtonLogic : MonoBehaviour
{
    public void OnButtonClick()
    {
        if (StickyAlmanacManager.Instance != null)
        {
            Debug.Log(gameObject.name);
            StickyAlmanacManager.Instance.SelectMenu(gameObject);
        }
        else
        {
            Debug.LogError("AlmanacManager instance not found!");
        }
    }

    public void closeSticky()
    {
        StickyAlmanacManager.Instance.ReturnToGame();
    }

    public void openSticky()
    {
        StickyAlmanacManager.Instance.ReturnToGame();
    }
}
