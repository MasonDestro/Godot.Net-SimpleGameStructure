using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public static class GlobalTypes
{
    #region Input Action

    public enum InputActionExecuteType
    {
        _Process,
        _PhysicsProcess,
        _Input,
        _UnhandledInput
    }

    [Flags]
    public enum InputActionType
    {
        None = 0,
        IsActionJustPressed = 1,
        IsActionPressed = 2,
        IsActionJustReleased = 4,
        GetAxis = 8,
        GetVector = 16,
        MouseMotionRelative = 32
    }

    public struct InputActionParams
    {
        public string ActionName_NegativeAction_NegativeX;
        public string PositiveAction_PositiveX;
        public string NegativeY;
        public string PositiveY;
    }

    public struct InputActionDetail
    {
        public InputActionExecuteType ExecuteType;
        public InputActionType ActionType;
        public float? Deadzone;

        public Action<double, InputActionType> KeyButtonMethod;
        public Action<double, float> FloatMethod;
        public Action<double, Vector2> Vector2Method;

        public Action OnRegister;
    }

    public struct InputActionMessage
    {
        public ulong InstanceID;
        public Dictionary<InputActionParams, InputActionDetail> Actions;
    }

    #endregion

    #region Equipment

    public enum EquipSlotType
    {
        None,
        LeftHand,
        RightHand,
        Back
    }

    #endregion
}
