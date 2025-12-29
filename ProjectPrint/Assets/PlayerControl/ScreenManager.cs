using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public enum GameState
{
    PlayMode,
    Pause,
    Shop,
    Object,
    Box,
    Profile
}

public class ScreenManager : MonoBehaviour
{
    [SerializeField] GameObject PauseCanvas;
    [SerializeField] GameObject ShopCanvas;
    [SerializeField] GameObject BoxCanvas;
    [SerializeField] GameObject ProfileCanvas;
    public static ScreenManager Instance;
    private static GameState currentState = GameState.PlayMode;
    public static GameState CurrentState => currentState;
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
        PauseCanvas.SetActive(false);
        ShopCanvas.SetActive(false);
        BoxCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //BoxCanvas.SetActive(currentState == GameState.Box);
    }

    public void TabButtonInteraction()
    {
        if (currentState == GameState.PlayMode)
        {
            OpenShop();
        }
        else
        {
            EscButtonInteraction();
        }
    }

    public void EscButtonInteraction()
    {
        switch (currentState)
        {
            case GameState.PlayMode:
                OpenPauseMenu();
                break;
            case GameState.Object:
                CloseObject();
                break;
            case GameState.Shop:
                CloseShop();
                break;
            case GameState.Pause:
                ClosePause();
                break;
            case GameState.Box:
                CloseBox();
                break;
            case GameState.Profile:
                CloseProfile();
                break;
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

    public void ClosePause()
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

    public void OpenBox()
    {
        currentState = GameState.Box;
        BoxCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseBox()
    {
        if (currentState != GameState.Box) return;
        currentState = GameState.PlayMode;
        BoxCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        BoxScreen.Instance.CloseBox();
    }
    public void OpenProfile()
    {
        if (currentState != GameState.Pause) return;

        currentState = GameState.Profile;
        ProfileCanvas.SetActive(true);
        PauseCanvas.SetActive(false);
    }
    public void CloseProfile()
    {
        if (currentState != GameState.Profile) return;
        currentState = GameState.Pause;
        ProfileCanvas.SetActive(false);
        PauseCanvas.SetActive(true);
    }
    public void ExitGame()
    {
        SaveSystem.CreateSave();
        SceneManager.LoadScene("MenuScene");
    }
}
