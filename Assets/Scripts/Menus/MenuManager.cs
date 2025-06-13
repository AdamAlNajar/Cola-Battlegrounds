using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The class that manages the room switch operation.
public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    void Awake()
    {
        instance = this;
    }
    [SerializeField] Menu[] menus;

    public void OpenMenu(Menu menu)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void OpenMenu(string menuName)
    {
        // Opens the specified meny by menu name set in the script
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
}
