using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using System;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; set; }

    public EventHandler Test;

    public EventHandler<OnMenuToggledEventArgs> OnMenuToggled;
    public class OnMenuToggledEventArgs : EventArgs
    {
        public Menu menu;
        public bool opened;
    }

    [SerializeField] private MenuSOList _menuSOList;

    public int OpenMenuCount => menuStack.Count;

    private Stack<Menu> menuStack = new();

    private void Awake()
    {
        Instance = this;

        if (SceneManager.GetActiveScene().name == "MainMenu")
            MainMenu.ShowWithFeedback();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {
        if ((InputController.Instance.WasPressed_Escape || InputController.Instance.WasPressed_BuildMenu || InputController.Instance.WasPressed_CharacterMenu) && menuStack.Count > 0)
        {
            menuStack.Peek().OnBackPressed();
        }
        else if (InputController.Instance.WasPressed_BuildMenu && menuStack.Count == 0 && !QuantumConsole.Instance.IsOpen)
        {
            BuildMenu2.Show();
        }
        else if (InputController.Instance.WasPressed_CharacterMenu && menuStack.Count == 0 && !QuantumConsole.Instance.IsOpen)
        {
            CharacterMenu.Show();
        }
    }

    public void CreateInstance<T>() where T : Menu
    {
        var prefab = _menuSOList.GetPrefab<T>();

        Instantiate(prefab, transform);
    }

    public void OpenMenu(Menu instance)
    {
        if (instance == null)
        {
            print("Menu instance is null!");
        }

        // De-activate top menu
        if (menuStack.Count > 0)
        {
            if (instance.DisableMenusUnderneath)
            {
                foreach (var menu in menuStack)
                {
                    if (menu.DontHide)
                        continue;

                    menu.gameObject.SetActive(false);

                    if (menu.DisableMenusUnderneath)
                        break;
                }
            }

            var topCanvas = instance.GetComponent<Canvas>();
            var previousCanvas = menuStack.Peek().GetComponent<Canvas>();
            topCanvas.sortingOrder = previousCanvas.sortingOrder + 1;
        }

        menuStack.Push(instance);

        OnMenuToggled?.Invoke(this, new OnMenuToggledEventArgs { menu = instance, opened = true });
    }

    public void CloseMenu(Menu menu)
    {
        if (menuStack.Count == 0)
        {
            Debug.LogErrorFormat(menu, "{0} cannot be closed because menu stack is empty", menu.GetType());
            return;
        }

        if (menuStack.Peek() != menu)
        {
            Debug.LogErrorFormat(menu, "{0} cannot be closed because it is not on top of stack", menu.GetType());
            return;
        }
        CloseTopMenu();

        OnMenuToggled?.Invoke(this, new OnMenuToggledEventArgs { menu = menu, opened = false });
    }

    public void CloseTopMenu()
    {
        var instance = menuStack.Pop();

        if (instance.DestroyWhenClosed)
            Destroy(instance.gameObject);
        else
            instance.gameObject.SetActive(false);

        // Re-activate top menu
        // If a re-activated menu is an overlay we need to activate the menu under it
        foreach (var menu in menuStack)
        {
            menu.gameObject.SetActive(true);

            if (menu.DisableMenusUnderneath)
                break;
        }
    }

    public bool IsTopMenu(Menu menu)
    {
        return menuStack.Count != 0 && menuStack.Peek() == menu;
    }
}

