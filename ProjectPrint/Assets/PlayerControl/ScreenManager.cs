using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public enum GameState
{
    PlayMode,
    Pause,
    Shop
}

public class ScreenManager : MonoBehaviour
{
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
        ShopCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Escape.WasPressedThisFrame())
        {
            if (currentState == GameState.PlayMode)
            {
                currentState = GameState.Pause;
                Debug.Log("Pause");
            }
            else
            {
                CloseShop();
                currentState = GameState.PlayMode;
                Debug.Log("Play");
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
}
