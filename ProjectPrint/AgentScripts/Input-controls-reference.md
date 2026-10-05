# Input Controls Reference

This reference covers the project-wide Input System asset, its code consumers, and remaining legacy input paths. It distinguishes configured bindings from behavior that is actually implemented.

## Input Setup

- `Assets/InputSystem_Actions.inputactions` is the project-wide action asset, assigned as `com.unity.input.settings.actions` in `ProjectSettings/EditorBuildSettings.asset`.
- The asset contains `Player` and `UI` maps, with Keyboard&Mouse, Gamepad, Joystick, Touch, and XR control schemes. Project-wide actions are automatically enabled in Play Mode by the Input System.
- `PlayerInteractions` also explicitly enables its serialized asset's `Player` map in `OnEnable` and disables it in `OnDisable`. Other gameplay scripts retrieve actions with `InputSystem.actions.FindAction(...)` and poll in `Update`; no `PlayerInput` component or direct `Keyboard.current`/`Gamepad.current` polling was found.
- `ScreenManager.CurrentState` gates player movement, look, and world interactions to `PlayMode`. `Esc`, `Tab`, and the menu-interaction action are checked before that gate; pause button actions are checked in `Pause`.
- `MainScene` and `MenuScene` each contain an `EventSystem` with `InputSystemUIInputModule` and navigation events enabled. Both modules reference a separate serialized UI action asset, not `InputSystem_Actions.inputactions`. The custom `UI` map is still used directly by the scripts listed below.
- `ProjectSettings/ProjectSettings.asset` sets `activeInputHandler: 2` (Both), so the new Input System and legacy Input Manager are enabled.

## Player Map

| Action | Default controls | Use |
| --- | --- | --- |
| `Move` | Gamepad left stick; keyboard WASD/arrows; joystick stick; XR primary 2D axis | `PlayerMover` reads a Vector2 and moves the CharacterController relative to the player. |
| `Look` | Gamepad right stick; pointer delta; joystick hat switch | `CameraMover` applies yaw and clamped pitch in Play Mode. |
| `Attack` | Mouse left; Enter; Gamepad west and right trigger; touch tap; joystick trigger; XR primary action | Looked up by `PlayerInteractions` but currently not used to perform an attack. |
| `Interact` | E; Gamepad south (A) | In Play Mode, interacts with the highlighted object using `ControlBinding.E`, subject to a 0.1-second debounce. |
| `Crouch` | C; Gamepad east (B) | Configured but no gameplay consumer found. |
| `Jump` | Space; Gamepad south (A); XR secondary button | Configured but no gameplay consumer found; `PlayerMover.jumpHeight` is not currently used. |
| `Previous` | 1, Q; Gamepad left shoulder (LB) | `ComputerScreen` changes to the previous shop tab. D-pad left is not bound here. |
| `Next` | 2, E; Gamepad right shoulder (RB) | `ComputerScreen` changes to the next shop tab. D-pad right is not bound here. E also triggers `Interact` in Play Mode. |
| `Sprint` | Left Shift; Gamepad left-stick press; XR trigger | Configured but no gameplay consumer found. |
| `RightClick` | Mouse right; Gamepad right trigger | `PlayerInteractions` picks up an eligible highlighted object on press. `ItemHolder` reads the same action while held to position/drop a held item. |
| `FButton` | F; Gamepad east (B) | Sends `ControlBinding.F` to the highlighted object in Play Mode; used for contextual actions such as unpacking. |
| `MoveObject` | Mouse right; Gamepad right trigger | Starts moving a highlighted movable object when not carrying one; `ItemHolder` reads it to finish placement. Shares controls with `RightClick`. |
| `MenuButton` | Mouse left; Gamepad west (X) | Sends `ControlBinding.MENU` to the highlighted object in Play Mode; used to open contextual screens such as the computer or printer. |
| `RotateObject` | Mouse wheel down/up; Gamepad LB/RB | `ItemHolder` reads a signed axis and rotates a moving object in 15-degree increments, throttled to 0.1 seconds. |
| `Test` | J | Configured but no consumer found. |

## UI Map

| Action | Default controls | Use |
| --- | --- | --- |
| `Navigate` | Gamepad D-pad, left/right sticks; keyboard WASD/arrows; joystick stick | `ComputerScreen` uses four-way navigation in the 3-column product grid. `BoxScreen` and `ShopController` also read this action for their custom navigation. |
| `Submit` | Generic `*/{Submit}`; Gamepad south (A) | Configured UI submit action. Project scripts do not poll it directly; the scene UI modules reference a different action asset. |
| `Cancel` | Generic `*/{Cancel}`; Gamepad east (B) | Configured UI cancel action. Project scripts do not poll it directly; the Computer shop uses its separate `Back` action. |
| `Point` | Mouse, pen, touch positions | Configured pointer-position action; no project script polls this action directly. |
| `Click` | Mouse left, pen tip, touch press, XR trigger | Configured UI click action; no project script polls this action directly. |
| `RightClick` | Mouse right | Configured UI pointer action. A second action with this name exists in the Player map; gameplay callers use unqualified lookups. |
| `MiddleClick` | Mouse middle | Configured but no project script polls it directly. |
| `ScrollWheel` | Mouse scroll | Configured UI scroll action; no project script polls it directly. |
| `TrackedDevicePosition` | XR controller device position | Configured for tracked UI; no project script polls it directly. |
| `TrackedDeviceOrientation` | XR controller device rotation | Configured for tracked UI; no project script polls it directly. |
| `Esc` | Escape; Gamepad start | `PlayerInteractions` calls `ScreenManager.EscButtonInteraction()` to open/close the current screen or pause menu. |
| `Tab` | Tab; Gamepad north (Y) | Also calls `ScreenManager.EscButtonInteraction()`; it is not a separate tab-cycle action. |
| `CreateSave` | K | `SaveSystem` creates a save. A `Ctrl` modifier check is present but commented out, so Ctrl is not required. |
| `LoadSave` | L | `SaveSystem` loads the current profile. A `Ctrl` modifier check is present but commented out, so Ctrl is not required. |
| `Ctrl` | Left Ctrl | Looked up by `SaveSystem`, but the only condition using it is commented out. |
| `UIButton1` | Gamepad east (B) | In Pause, closes the pause menu. In `BoxScreen` non-list mode, invokes Send Order. |
| `UIButton2` | Gamepad south (A) | In Pause, saves. In `BoxScreen`, selects a listed button or invokes Add Label. Used by the older `ShopController` to buy its selected product. |
| `UIButton3` | Gamepad west (X) | In Pause, loads the current profile. In `BoxScreen` non-list mode, invokes Add Product. `Printer` retrieves these actions but does not use them. |
| `Buy` | Gamepad south (A) | `ComputerScreen` invokes the selected card's nested Buy button only when it is interactable/affordable. Rebind this action to change the shop purchase control. |
| `Back` | Gamepad east (B) | `ComputerScreen` closes the shop. Rebind this action to change the shop back control. |

## Other Runtime Input

- `BoxScreen` uses `Navigate.x` to enter/leave its button-list mode and `Navigate.y` to move a highlighted index, with a 0.3-second movement delay. In list mode A invokes the selected button; outside list mode B/A/X invoke Send Order/Add Label/Add Product.
- The older `ShopController` component is present in `MainScene`. It uses `Navigate.x` to switch between its shop types, `Navigate.y` to select products, and A (`UIButton2`) to invoke the selected purchase button. The Computer shop has its own controls in `ComputerScreen`.
- `SaveSystem` also exposes save/load UI methods, so UI buttons can call save/load without keyboard input.
- `Printer` retrieves `UIButton1/2/3` in `Awake`, but no use of those fields was found in the rest of the class.

## Legacy Input

- The legacy `InputManager.asset` still defines keyboard/joystick Horizontal and Vertical axes, Fire1/2/3, Jump, mouse X/Y/scroll axes, joystick axes/buttons, and legacy Submit/Cancel/debug buttons.
- Main gameplay code uses the new action asset. Remaining direct legacy polling is in `Assets/Performance/PerfTest.cs` (P finds child printers; O runs its test) and Unity's bundled TextMesh Pro example scripts under `Assets/TextMesh Pro/Examples & Extras/Scripts/` (mouse, keyboard modifier, axis, and touch demonstrations). These example inputs are not the main game's player controls.

## Control Overlaps To Keep In Mind

- Gamepad A is shared across Player `Interact`/`Jump`, `UIButton2`, UI `Submit`, and the shop-specific `Buy`. These actions can all receive the same press; the consumer scripts' game-state conditions determine which effects occur.
- Gamepad B is shared across Player `Crouch`/`FButton`, `UIButton1`, UI `Cancel`, and shop-specific `Back`; the actions are not mutually exclusive at the Input System level.
- Gamepad LB/RB are `Previous`/`Next` for shop tabs and negative/positive `RotateObject` inputs. Mouse right and Gamepad right trigger each bind both Player `RightClick` and `MoveObject`.
- `RightClick` exists in both maps. Prefer map-qualified lookups such as `FindAction("Player/RightClick")` or `FindAction("UI/RightClick")` when adding code to avoid ambiguous name resolution.