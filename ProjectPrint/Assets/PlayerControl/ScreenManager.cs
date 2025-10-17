using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public enum GameState
{
    PlayMode,
    Pause,
    Shop,
    Object
}

public class ScreenManager : MonoBehaviour
{
    [SerializeField] GameObject PauseCanvas;
    [SerializeField] GameObject ShopCanvas;
    static ScreenManager Instance;
    private static GameState currentState = GameState.PlayMode;
    public static GameState CurrentState => currentState;
    InputAction Escape;
    InputAction Tab;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        Escape = InputSystem.actions.FindAction("Esc");
        Tab = InputSystem.actions.FindAction("Tab");
        PauseCanvas.SetActive(false);
        ShopCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Escape.WasPressedThisFrame())
        {
            if (currentState == GameState.PlayMode)
            {
                OpenPauseMenu();
            }
            else
            {
                CloseShop();
                CloseObject();
                ClosePause();
            }
        }
        if (Tab.WasPressedThisFrame()) 
        {
            if (currentState == GameState.PlayMode) 
            {
                OpenShop();
            }
            else if (currentState == GameState.Shop)
            {
                CloseShop();
            }
        }
    }

    private void OpenPauseMenu()
    {
        if (currentState != GameState.PlayMode) return;
        currentState = GameState.Pause;
        //Debug.Log("Pause");
        PauseCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ClosePause()
    {
        if (currentState != GameState.Pause) return;

        currentState = GameState.PlayMode;
        //Debug.Log("Play");
        PauseCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void CloseShop()
    {
        if (currentState != GameState.Shop) return;

        currentState = GameState.PlayMode;
        ShopCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OpenShop()
    {
        if (currentState == GameState.Shop) return;
        currentState = GameState.Shop;
        ShopCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void OpenObject()
    {
        currentState = GameState.Object;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public static void CloseObject()
    {
        if (currentState != GameState.Object) return;
        currentState = GameState.PlayMode;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
