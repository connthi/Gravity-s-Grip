// Hand-written input wrapper using the programmatic InputSystem API.
// No JSON parsing, no interface implementation — just reliable action maps.
using UnityEngine.InputSystem;

public class PlayerInputActions
{
    public PlayerActions Player { get; }
    public UIActions     UI     { get; }

    public PlayerInputActions()
    {
        // ── Player map ────────────────────────────────────────────────────────
        var pm = new InputActionMap("Player");

        var move = pm.AddAction("Move", InputActionType.Value);
        move.expectedControlType = "Vector2";
        move.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        var look = pm.AddAction("Look", InputActionType.Value);
        look.expectedControlType = "Vector2";
        look.AddBinding("<Mouse>/delta");

        var jump = pm.AddAction("Jump", InputActionType.Button);
        jump.AddBinding("<Keyboard>/space");

        var interact = pm.AddAction("Interact", InputActionType.Button);
        interact.AddBinding("<Keyboard>/e");

        var drop = pm.AddAction("Drop", InputActionType.Button);
        drop.AddBinding("<Keyboard>/q");

        var toggleTorch = pm.AddAction("ToggleTorch", InputActionType.Button);
        toggleTorch.AddBinding("<Keyboard>/f");

        var throwAction = pm.AddAction("Throw", InputActionType.Button);
        throwAction.AddBinding("<Mouse>/leftButton");

        Player = new PlayerActions(pm);

        // ── UI map ────────────────────────────────────────────────────────────
        var um = new InputActionMap("UI");

        var pause = um.AddAction("Pause", InputActionType.Button);
        pause.AddBinding("<Keyboard>/escape");

        UI = new UIActions(um);
    }

    public class PlayerActions
    {
        private readonly InputActionMap _map;
        public InputAction Move        { get; }
        public InputAction Look        { get; }
        public InputAction Jump        { get; }
        public InputAction Interact    { get; }
        public InputAction Drop        { get; }
        public InputAction ToggleTorch { get; }
        public InputAction Throw       { get; }

        public PlayerActions(InputActionMap map)
        {
            _map        = map;
            Move        = map["Move"];
            Look        = map["Look"];
            Jump        = map["Jump"];
            Interact    = map["Interact"];
            Drop        = map["Drop"];
            ToggleTorch = map["ToggleTorch"];
            Throw       = map["Throw"];
        }

        public void Enable()  => _map.Enable();
        public void Disable() => _map.Disable();
    }

    public class UIActions
    {
        private readonly InputActionMap _map;
        public InputAction Pause { get; }

        public UIActions(InputActionMap map)
        {
            _map  = map;
            Pause = map["Pause"];
        }

        public void Enable()  => _map.Enable();
        public void Disable() => _map.Disable();
    }
}
