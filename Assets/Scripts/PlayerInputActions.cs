// Auto-generated equivalent of what Unity's Input System code generator produces.
// If you later use Generate C# Class on the .inputactions asset, delete this file.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection2
{
    public InputActionAsset asset { get; }

    public PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""a1b2c3d4-0001-0001-0001-000000000001"",
            ""actions"": [
                { ""name"": ""Move"",        ""type"": ""Value"",  ""expectedControlType"": ""Vector2"", ""initialStateCheck"": true },
                { ""name"": ""Look"",        ""type"": ""Value"",  ""expectedControlType"": ""Vector2"", ""initialStateCheck"": true },
                { ""name"": ""Jump"",        ""type"": ""Button"", ""expectedControlType"": ""Button"" },
                { ""name"": ""Interact"",    ""type"": ""Button"", ""expectedControlType"": ""Button"" },
                { ""name"": ""Drop"",        ""type"": ""Button"", ""expectedControlType"": ""Button"" },
                { ""name"": ""ToggleTorch"", ""type"": ""Button"", ""expectedControlType"": ""Button"" },
                { ""name"": ""Throw"",       ""type"": ""Button"", ""expectedControlType"": ""Button"" }
            ],
            ""bindings"": [
                { ""name"": ""WASD"", ""path"": ""2DVector"", ""action"": ""Move"", ""isComposite"": true },
                { ""name"": ""up"",    ""path"": ""<Keyboard>/w"", ""action"": ""Move"", ""isPartOfComposite"": true },
                { ""name"": ""down"",  ""path"": ""<Keyboard>/s"", ""action"": ""Move"", ""isPartOfComposite"": true },
                { ""name"": ""left"",  ""path"": ""<Keyboard>/a"", ""action"": ""Move"", ""isPartOfComposite"": true },
                { ""name"": ""right"", ""path"": ""<Keyboard>/d"", ""action"": ""Move"", ""isPartOfComposite"": true },
                { ""path"": ""<Mouse>/delta"",       ""action"": ""Look"" },
                { ""path"": ""<Keyboard>/space"",    ""action"": ""Jump"" },
                { ""path"": ""<Keyboard>/e"",        ""action"": ""Interact"" },
                { ""path"": ""<Keyboard>/q"",        ""action"": ""Drop"" },
                { ""path"": ""<Keyboard>/f"",        ""action"": ""ToggleTorch"" },
                { ""path"": ""<Mouse>/leftButton"",  ""action"": ""Throw"" }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""a1b2c3d4-0002-0001-0001-000000000001"",
            ""actions"": [
                { ""name"": ""Pause"", ""type"": ""Button"", ""expectedControlType"": ""Button"" }
            ],
            ""bindings"": [
                { ""path"": ""<Keyboard>/escape"", ""action"": ""Pause"" }
            ]
        }
    ],
    ""controlSchemes"": []
}");

        // Player map
        var playerMap = asset.FindActionMap("Player", throwIfNotFound: true);
        Player = new PlayerActions(playerMap);

        // UI map
        var uiMap = asset.FindActionMap("UI", throwIfNotFound: true);
        UI = new UIActions(uiMap);
    }

    public void Dispose() => asset.Dispose();

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action) => asset.Contains(action);

    public System.Collections.Generic.IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public void Enable()  => asset.Enable();
    public void Disable() => asset.Disable();

    IEnumerable<InputBinding> IInputActionCollection2.bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        => asset.FindAction(actionNameOrId, throwIfNotFound);

    public int FindBinding(InputBinding bindingMask, out InputAction action)
        => asset.FindBinding(bindingMask, out action);

    // ── Player action map ─────────────────────────────────────────────────────

    public PlayerActions Player { get; }

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
            _map       = map;
            Move        = map.FindAction("Move",        throwIfNotFound: true);
            Look        = map.FindAction("Look",        throwIfNotFound: true);
            Jump        = map.FindAction("Jump",        throwIfNotFound: true);
            Interact    = map.FindAction("Interact",    throwIfNotFound: true);
            Drop        = map.FindAction("Drop",        throwIfNotFound: true);
            ToggleTorch = map.FindAction("ToggleTorch", throwIfNotFound: true);
            Throw       = map.FindAction("Throw",       throwIfNotFound: true);
        }

        public void Enable()  => _map.Enable();
        public void Disable() => _map.Disable();
    }

    // ── UI action map ─────────────────────────────────────────────────────────

    public UIActions UI { get; }

    public class UIActions
    {
        private readonly InputActionMap _map;

        public InputAction Pause { get; }

        public UIActions(InputActionMap map)
        {
            _map  = map;
            Pause = map.FindAction("Pause", throwIfNotFound: true);
        }

        public void Enable()  => _map.Enable();
        public void Disable() => _map.Disable();
    }
}
