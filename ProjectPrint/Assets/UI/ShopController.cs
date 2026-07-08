using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

enum ShopType
{
    Filament,
    Model,
    Printer,
    Other
}

public class ShopController : MonoBehaviour
{
    [SerializeField] Shop FilamentShop;
    [SerializeField] Shop ModelShop;
    [SerializeField] Shop PrinterShop;
    [SerializeField] Shop OthersShop;
    [SerializeField] GameObject highlight;
    [SerializeField] float controllerDelay = 0.15f;
    float lastMove;
    InputAction DPad;
    InputAction UIButton;
    ShopType currentShop;
    int currentIndex;

    private void Awake()
    {
        DPad = InputSystem.actions.FindAction("Navigate");
        UIButton = InputSystem.actions.FindAction("UIButton2");
        currentShop = ShopType.Filament;
    }
    private void OnEnable()
    {
        Shop s = GetCurrentShop();
        s.GenerateButtons();
        Button button = s.Buttons[currentIndex];
        highlight.transform.position = button.transform.position;
    }

    private void Update()
    {
        Vector2 dpadInput = DPad.ReadValue<Vector2>();
        HandleShopSwitching(dpadInput);
        HandleProductSwitching(dpadInput);
        HandleControllerButton();
    }

    private void HandleControllerButton()
    {
        if (UIButton.WasPressedThisFrame())
        {
            GetCurrentShop().Buttons[currentIndex].onClick.Invoke();
        }
    }

    private void HandleProductSwitching(Vector2 dpadInput)
    {
        if (Time.time - lastMove < controllerDelay) return;
        if (dpadInput.y < -0.5f)
        {
            currentIndex = Math.Min(currentIndex + 1, GetCurrentShop().Buttons.Count - 1);
            highlight.transform.position = GetCurrentShop().Buttons[currentIndex].transform.position;
            lastMove = Time.time;
        }
        else if (dpadInput.y > 0.5f)
        {
            currentIndex = Math.Max(currentIndex - 1, 0);
            highlight.transform.position = GetCurrentShop().Buttons[currentIndex].transform.position;
            lastMove = Time.time;
        }
    }

    private Shop GetCurrentShop()
    {
        switch (currentShop)
        {
            case ShopType.Filament:
                return FilamentShop;
            case ShopType.Model:
                return ModelShop;
            case ShopType.Printer:
                return PrinterShop;
            case ShopType.Other:
                return OthersShop;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleShopSwitching(Vector2 dpadInput)
    {
        if (Time.time - lastMove < controllerDelay) return;
        if (dpadInput.x > 0.5f)
        {
            if (currentShop == ShopType.Filament)
            {
                currentShop = ShopType.Printer;
            }
            else if (currentShop == ShopType.Printer)
            {
                currentShop = ShopType.Other;
            }
            lastMove = Time.time;
            if (currentIndex >= GetCurrentShop().Buttons.Count)
            {
                currentIndex = GetCurrentShop().Buttons.Count - 1;
            }
            highlight.transform.position = GetCurrentShop().Buttons[currentIndex].transform.position;
        }
        else if (dpadInput.x < -0.5f)
        {
            if (currentShop == ShopType.Other)
            {
                currentShop = ShopType.Printer;
            }
            else if (currentShop == ShopType.Printer)
            {
                currentShop = ShopType.Filament;
            }
            lastMove = Time.time;
            if (currentIndex >= GetCurrentShop().Buttons.Count)
            {
                currentIndex = GetCurrentShop().Buttons.Count - 1;
            }
            highlight.transform.position = GetCurrentShop().Buttons[currentIndex].transform.position;
        }
    }
}
