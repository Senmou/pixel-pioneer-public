﻿using UnityEngine;

public abstract class Menu<T> : Menu where T : Menu<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        Instance = null;
    }

    protected static void Open()
    {
        if (Instance == null)
            MenuManager.Instance.CreateInstance<T>();
        else
            Instance.gameObject.SetActive(true);

        MenuManager.Instance.OpenMenu(Instance);
    }

    protected static void Close()
    {
        if (Instance == null)
        {
            //Debug.LogErrorFormat("Trying to close menu {0} but Instance is null", typeof(T));
            return;
        }

        MenuManager.Instance.CloseMenu(Instance);
    }

    public override void OnBackPressed()
    {
        Close();
    }
}

public abstract class Menu : MonoBehaviour
{
    [Tooltip("Destroy the Game Object when menu is closed (reduces memory usage)")]
    public bool DestroyWhenClosed = true;

    [Tooltip("Disable menus that are under this one in the stack")]
    public bool DisableMenusUnderneath = true;

    public bool DontHide = false;

    public abstract void OnBackPressed();
}
