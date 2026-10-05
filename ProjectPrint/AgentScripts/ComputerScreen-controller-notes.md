# Computer shop controller notes

For the full action/control inventory and game-wide input wiring, see [Input-controls-reference.md](Input-controls-reference.md).

## Unity wiring

- `Assets/Scenes/MainScene.unity` contains the Computer-related references and an active `EventSystem` with `InputSystemUIInputModule` and navigation events enabled.
- The Computer is defined by `Assets/Furniture/Computer/Computer.prefab`. Its `ComputerScreen` GameObject contains the `[ComputerUI]` child, which has `ComputerScreen.cs` attached.
- The `ComputerScreen` component serializes three shop pages and three matching shop inventories, plus `ShopTabButton` and `ShopProductCard` prefab references.
- Each generated product card has a root Button, a `ProductLabel`, and a nested `BuyButton`. The nested buy button is disabled when the current balance cannot afford the product. Ten placeholder cards are appended after real products and should not be included in controller product selection.

## Existing input conventions

- `Assets/InputSystem_Actions.inputactions` has a UI `Navigate` Vector2 action bound to Gamepad left stick and D-pad (and other configured sticks/devices). `ShopController` and `BoxScreen` read it in `Update` and use a repeat delay for vertical index movement.
- The UI map's `Submit` action now also explicitly binds Gamepad south (A), and `Cancel` explicitly binds Gamepad east (B), in addition to retaining their generic usage bindings for other supported devices.
- Dedicated UI actions `Buy` and `Back` provide the Computer shop with purpose-specific controls. They currently default to Gamepad south (A) and Gamepad east (B), respectively; rebind these actions in `Assets/InputSystem_Actions.inputactions` without changing `ComputerScreen.cs`.
- `UIButton2` is Gamepad south (A); `UIButton1` is Gamepad east (B); `UIButton3` is Gamepad west.
- `Previous` is bound to Gamepad left shoulder (LB) and keyboard alternatives. `Next` is bound to Gamepad right shoulder (RB) and keyboard alternatives. Their Gamepad D-pad bindings were removed so D-pad left/right remain available to `Navigate`; ComputerScreen changes tabs with LB/RB only on Gamepad.
- Existing UI scripts invoke a selected Button's `onClick` when their controller action is pressed. Shop purchases must still honor the nested buy button's `interactable` state.

## Implemented shop behavior

- While the shop page is active, `Navigate` moves a clamped selection through stocked products in a three-column, row-major grid, with a 0.15-second repeat delay. Left/right move between columns on the same row; up/down move between rows in the same column. In an incomplete final row, vertical movement selects its last available card. The generated card root is made selectable at runtime and highlighted; its automatic UI navigation is disabled so controller selection follows this script's product index.
- The selected row is scrolled fully into view and existing ScrollRect inertia is stopped during controller correction. `Buy` invokes the selected card's nested buy button only when that button is interactable. `Back` closes the shop and clears EventSystem selection.
- LB/RB switch tabs using `Previous`/`Next`; D-pad directions and left-stick directions move among products only.

## Validation

- After editing `ComputerScreen.cs`, check IDE diagnostics and build `Assembly-CSharp.csproj` from the repository root. Controller behavior still needs an in-Unity playtest with a Gamepad, including an unaffordable item, page switching, and closing with B.