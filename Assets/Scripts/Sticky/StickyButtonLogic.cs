using UnityEngine;
using System.Collections.Generic;

public class StickyButtonLogic : MonoBehaviour
{
    public void OnButtonClick()
    {
        if (StickyAlmanacManager.Instance != null)
        {
            StickyAlmanacManager.Instance.selectMenu(gameObject);
        }
        else
        {
            Debug.LogError("AlmanacManager instance not found!");
        }
    }
}
