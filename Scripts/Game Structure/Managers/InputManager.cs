using Godot;
using System.Collections.Generic;
using System;

public partial class InputManager : Node
{
    #region Data Type

    private struct GetAxisParams
    {
        public string negativeAction;
        public string positiveAction;
    }
    private struct GetVectorParams
    {
        public string negativeX;
        public string positiveX;
        public string negativeY;
        public string positiveY;
        public float? deadzone;
    }

    private class InputActionResponse
    {
        public Dictionary<string, Action<double, GlobalTypes.InputActionType>> IsActionJustPressed = new Dictionary<string, Action<double, GlobalTypes.InputActionType>>();
        public Dictionary<string, Action<double, GlobalTypes.InputActionType>> IsActionPressed = new Dictionary<string, Action<double, GlobalTypes.InputActionType>>();
        public Dictionary<string, Action<double, GlobalTypes.InputActionType>> IsActionJustReleased = new Dictionary<string, Action<double, GlobalTypes.InputActionType>>();
        public Dictionary<GetAxisParams, Action<double, float>> GetAxis = new Dictionary<GetAxisParams, Action<double, float>>();
        public Dictionary<GetVectorParams, Action<double, Vector2>> GetVector = new Dictionary<GetVectorParams, Action<double, Vector2>>();
    }

    private class InputEventResponse
    {
        public Action<double, Vector2> MouseMotionRelative;
    }

    private class InputResponse
    {
        public ulong InstanceID { get; init; }
        public Action OnUnregistered;

        public InputActionResponse _Process = new InputActionResponse();
        public InputActionResponse _PhysicsProcess = new InputActionResponse();
        public InputEventResponse _Input = new InputEventResponse();
        public InputEventResponse _UnhandledInput = new InputEventResponse();


        public InputResponse(ulong instanceID)
        {
            InstanceID = instanceID;
        }
    }
    #endregion

    #region Properties
    public static InputManager Instance { get; private set; }


    private InputResponse activeResponse;
    #endregion

    #region Godot Methods
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        InputActionResponse response = activeResponse?._Process;
        if (response == null)
        {
            return;
        }


        foreach (var kvpair in response.IsActionJustPressed)
        {
            if (Input.IsActionJustPressed(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionJustPressed);
        }

        foreach (var kvpair in response.IsActionPressed)
        {
            if (Input.IsActionPressed(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionPressed);
        }

        foreach (var kvpair in response.IsActionJustReleased)
        {
            if (Input.IsActionJustReleased(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionJustReleased);
        }

        foreach (var kvpair in response.GetAxis)
        {
            var parameters = kvpair.Key;
            kvpair.Value?.Invoke(delta,
                Input.GetAxis(parameters.negativeAction, parameters.positiveAction));
        }

        foreach (var kvpair in response.GetVector)
        {
            var parameters = kvpair.Key;
            kvpair.Value?.Invoke(delta,
                Input.GetVector(parameters.negativeX, parameters.positiveX,
                                parameters.negativeY, parameters.positiveY,
                                parameters.deadzone.HasValue ? parameters.deadzone.Value : -1));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        InputActionResponse response = activeResponse?._PhysicsProcess;
        if (response == null)
        {
            return;
        }


        foreach (var kvpair in response.IsActionJustPressed)
        {
            if (Input.IsActionJustPressed(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionJustPressed);
        }

        foreach (var kvpair in response.IsActionPressed)
        {
            if (Input.IsActionPressed(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionPressed);
        }

        foreach (var kvpair in response.IsActionJustReleased)
        {
            if (Input.IsActionJustReleased(kvpair.Key))
                kvpair.Value?.Invoke(delta, GlobalTypes.InputActionType.IsActionJustReleased);
        }

        foreach (var kvpair in response.GetAxis)
        {
            var parameters = kvpair.Key;
            kvpair.Value?.Invoke(delta,
                Input.GetAxis(parameters.negativeAction, parameters.positiveAction));
        }

        foreach (var kvpair in response.GetVector)
        {
            var parameters = kvpair.Key;
            kvpair.Value?.Invoke(delta,
                Input.GetVector(parameters.negativeX, parameters.positiveX,
                                parameters.negativeY, parameters.positiveY,
                                parameters.deadzone.HasValue ? parameters.deadzone.Value : -1));
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            activeResponse?._UnhandledInput?.MouseMotionRelative?.Invoke(0, eventMouseMotion.Relative);
        }
    }
    #endregion

    #region Public Mehtods

    public void RegisterInputMethods(GlobalTypes.InputActionMessage msg)
    {
        activeResponse?.OnUnregistered?.Invoke();

        activeResponse = new InputResponse(msg.InstanceID);
        activeResponse.OnUnregistered = msg.OnUnregistered;

        foreach (var kvpair in msg.Actions)
        {
            switch (kvpair.Value.ExecuteType)
            {
                case GlobalTypes.InputActionExecuteType._Process:

                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionJustPressed))
                    {
                        activeResponse._Process.IsActionJustPressed[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionPressed))
                    {
                        activeResponse._Process.IsActionPressed[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionJustReleased))
                    {
                        activeResponse._Process.IsActionJustReleased[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.GetAxis))
                    {
                        GetAxisParams axisParams = new()
                        {
                            negativeAction = kvpair.Key.ActionName_NegativeAction_NegativeX,
                            positiveAction = kvpair.Key.PositiveAction_PositiveX
                        };

                        activeResponse._Process.GetAxis[axisParams] = kvpair.Value.FloatMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.GetVector))
                    {
                        GetVectorParams vectorParams = new()
                        {
                            negativeX = kvpair.Key.ActionName_NegativeAction_NegativeX,
                            positiveX = kvpair.Key.PositiveAction_PositiveX,
                            negativeY = kvpair.Key.NegativeY,
                            positiveY = kvpair.Key.PositiveY,
                            deadzone = kvpair.Value.Deadzone
                        };

                        activeResponse._Process.GetVector[vectorParams] = kvpair.Value.Vector2Method;
                    }

                    break;

                case GlobalTypes.InputActionExecuteType._PhysicsProcess:

                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionJustPressed))
                    {
                        activeResponse._PhysicsProcess.IsActionJustPressed[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionPressed))
                    {
                        activeResponse._PhysicsProcess.IsActionPressed[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.IsActionJustReleased))
                    {
                        activeResponse._PhysicsProcess.IsActionJustReleased[kvpair.Key.ActionName_NegativeAction_NegativeX] = kvpair.Value.KeyButtonMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.GetAxis))
                    {
                        GetAxisParams axisParams = new()
                        {
                            negativeAction = kvpair.Key.ActionName_NegativeAction_NegativeX,
                            positiveAction = kvpair.Key.PositiveAction_PositiveX
                        };

                        activeResponse._PhysicsProcess.GetAxis[axisParams] = kvpair.Value.FloatMethod;
                    }
                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.GetVector))
                    {
                        GetVectorParams vectorParams = new()
                        {
                            negativeX = kvpair.Key.ActionName_NegativeAction_NegativeX,
                            positiveX = kvpair.Key.PositiveAction_PositiveX,
                            negativeY = kvpair.Key.NegativeY,
                            positiveY = kvpair.Key.PositiveY,
                            deadzone = kvpair.Value.Deadzone
                        };

                        activeResponse._PhysicsProcess.GetVector[vectorParams] = kvpair.Value.Vector2Method;
                    }

                    break;

                case GlobalTypes.InputActionExecuteType._Input:



                    break;

                case GlobalTypes.InputActionExecuteType._UnhandledInput:

                    if (kvpair.Value.ActionType.HasFlag(GlobalTypes.InputActionType.MouseMotionRelative))
                    {
                        activeResponse._UnhandledInput.MouseMotionRelative = kvpair.Value.Vector2Method;
                    }

                    break;
            }

            kvpair.Value.OnRegister?.Invoke();
        }

    }

    #endregion

}
