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
    Printer,
    Box,
    Profile,
    Focus,
    Unfocus
}

public class ScreenManager : MonoBehaviour
{
    [SerializeField] GameObject PauseCanvas;
    [SerializeField] GameObject ShopCanvas;
    [SerializeField] GameObject BoxCanvas;
    [SerializeField] GameObject ProfileCanvas;
    [SerializeField] GameObject PrinterCanvas;
    [SerializeField] GameObject ComputerCanvas;
    [SerializeField] GameObject Crosshair;
    public static ScreenManager Instance;
    private static GameState currentState = GameState.PlayMode;
    public static GameState CurrentState => currentState;
    static bool focusOpened = false;
    static bool focusOn = false;
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
        PrinterCanvas.SetActive(false);
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
            case GameState.Printer:
                ClosePrinter();
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
            case GameState.Focus:
                CloseFocus();
                break;
        }
    } 

    private void SetPlayMode()
    {
        currentState = GameState.PlayMode;
        PauseCanvas.SetActive(false);
        ShopCanvas.SetActive(false);
        BoxCanvas.SetActive(false);
        PrinterCanvas.SetActive(false);
        Crosshair.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void OpenPauseMenu()
    {
        if (currentState != GameState.PlayMode) return;
        currentState = GameState.Pause;
        //Debug.Log("Pause");
        PauseCanvas.SetActive(true);
        Crosshair.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePause()
    {
        if (currentState != GameState.Pause) return;

        SetPlayMode();
        //Debug.Log("Play");
        PauseCanvas.SetActive(false);
        Crosshair.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void CloseShop()
    {
        if (currentState != GameState.Shop) return;

        SetPlayMode();
        ShopCanvas.SetActive(false);
        Crosshair.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OpenShop()
    {
        if (currentState == GameState.Shop) return;
        currentState = GameState.Shop;
        ShopCanvas.SetActive(true);
        Crosshair.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void OpenPrinter(Printer printer)
    {
        currentState = GameState.Printer;
        Instance.PrinterCanvas.SetActive(true);
        Instance.Crosshair.SetActive(false);
        PrinterScreen.AssignPrinter(printer);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public static void ClosePrinter()
    {
        if (currentState != GameState.Printer) return;
        PrinterScreen.RemovePrinter();
        Instance.PrinterCanvas.SetActive(false);
        Instance.Crosshair.SetActive(true);
        Instance.SetPlayMode();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenBox()
    {
        currentState = GameState.Box;
        BoxCanvas.SetActive(true);
        Crosshair.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseBox()
    {
        if (currentState != GameState.Box) return;
        SetPlayMode();
        BoxCanvas.SetActive(false);
        Crosshair.SetActive(true);
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
    public static void OpenFocus()
    {
        if (currentState == GameState.Focus) return;
        currentState = GameState.Focus;
        focusOpened = true;
        Instance.ComputerCanvas.SetActive(focusOn && focusOpened);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public static void CloseFocus()
    {
        if (currentState != GameState.Focus) return;
        currentState = GameState.Unfocus;
        Instance.ComputerCanvas.SetActive(false);
        Instance.Crosshair.SetActive(true);
        focusOn = false;
        focusOpened = false;
        CameraMover.SetTargetPosition(Vector3.zero, Quaternion.identity);
        Computer.StopComputer();
    }
    public static void FocusOn()
    {
        focusOn = true;
        Instance.ComputerCanvas.SetActive(focusOn && focusOpened);
        Instance.Crosshair.SetActive(!(focusOn && focusOpened));
    }
    public static void CloseUnfocus()
    {
        if (currentState != GameState.Unfocus) return;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Instance.SetPlayMode();
    }
    public void ExitGame()
    {
        SaveSystem.CreateSave();
        SceneManager.LoadScene("MenuScene");
    }
}
