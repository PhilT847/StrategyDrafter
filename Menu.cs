using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    public ControlledUI Master_UI;

    public bool MenuActive;

    public abstract void OpenMenu();

    public abstract void CloseMenu();

    private void Awake()
    {
        Master_UI = GetComponentInParent<ControlledUI>();

        gameObject.SetActive(false);
    }
}
