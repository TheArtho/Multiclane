//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Game/Input/Console Input Actions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @ConsoleInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @ConsoleInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Console Input Actions"",
    ""maps"": [
        {
            ""name"": ""Console"",
            ""id"": ""7ff4e204-bc1c-4803-ae95-05a8531080d4"",
            ""actions"": [
                {
                    ""name"": ""Toggle"",
                    ""type"": ""Button"",
                    ""id"": ""ec23fc90-37ea-4e22-a20a-9d161ab1dcfd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Send"",
                    ""type"": ""Button"",
                    ""id"": ""83e26584-5e03-4278-89a7-ac5089da6404"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""aaee4137-95ef-4f9d-a847-2e87d184917d"",
                    ""path"": ""<Keyboard>/f1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Toggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ba110ba0-530f-4085-8184-54b7b12cf56d"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Send"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Console
        m_Console = asset.FindActionMap("Console", throwIfNotFound: true);
        m_Console_Toggle = m_Console.FindAction("Toggle", throwIfNotFound: true);
        m_Console_Send = m_Console.FindAction("Send", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

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

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Console
    private readonly InputActionMap m_Console;
    private List<IConsoleActions> m_ConsoleActionsCallbackInterfaces = new List<IConsoleActions>();
    private readonly InputAction m_Console_Toggle;
    private readonly InputAction m_Console_Send;
    public struct ConsoleActions
    {
        private @ConsoleInputActions m_Wrapper;
        public ConsoleActions(@ConsoleInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Toggle => m_Wrapper.m_Console_Toggle;
        public InputAction @Send => m_Wrapper.m_Console_Send;
        public InputActionMap Get() { return m_Wrapper.m_Console; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ConsoleActions set) { return set.Get(); }
        public void AddCallbacks(IConsoleActions instance)
        {
            if (instance == null || m_Wrapper.m_ConsoleActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_ConsoleActionsCallbackInterfaces.Add(instance);
            @Toggle.started += instance.OnToggle;
            @Toggle.performed += instance.OnToggle;
            @Toggle.canceled += instance.OnToggle;
            @Send.started += instance.OnSend;
            @Send.performed += instance.OnSend;
            @Send.canceled += instance.OnSend;
        }

        private void UnregisterCallbacks(IConsoleActions instance)
        {
            @Toggle.started -= instance.OnToggle;
            @Toggle.performed -= instance.OnToggle;
            @Toggle.canceled -= instance.OnToggle;
            @Send.started -= instance.OnSend;
            @Send.performed -= instance.OnSend;
            @Send.canceled -= instance.OnSend;
        }

        public void RemoveCallbacks(IConsoleActions instance)
        {
            if (m_Wrapper.m_ConsoleActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IConsoleActions instance)
        {
            foreach (var item in m_Wrapper.m_ConsoleActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_ConsoleActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public ConsoleActions @Console => new ConsoleActions(this);
    public interface IConsoleActions
    {
        void OnToggle(InputAction.CallbackContext context);
        void OnSend(InputAction.CallbackContext context);
    }
}
