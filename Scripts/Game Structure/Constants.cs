using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalTypes;

public static class Constants
{
    #region Private Field

    static float _gravity3DValue = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    static Vector3 _gravity3DVector = ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3();

    #endregion


    #region Public Property

    public static Vector3 Gravity3D { get { return _gravity3DValue * _gravity3DVector; } }

    #endregion

    #region Input Action

    public static InputActionParams InputAction_Attack = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = "Attack"
    };

    public static InputActionParams InputAction_Interact = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = "Interact"
    };

    public static InputActionParams InputAction_Esc = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = "Esc"
    };

    public static InputActionParams InputAction_SpeedUp = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = "SpeedUp"
    };

    public static InputActionParams InputAction_UseSkill = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = "UseSkill"
    };

    //public static InputActionParams InputAction_Jump = new InputActionParams
    //{
    //    ActionName_NegativeAction_NegativeX = "Jump"
    //};

    //public static InputActionParams InputAction_Crouch = new InputActionParams
    //{
    //    ActionName_NegativeAction_NegativeX = "Crouch"
    //};

    public static InputActionParams GetAxis_MoveVertical = new InputActionParams
    {
        PositiveAction_PositiveX = "Jump",
        ActionName_NegativeAction_NegativeX = "Crouch"
    };

    public static InputActionParams GetVector_MoveHorizontal = new InputActionParams
    {
        PositiveAction_PositiveX = "MoveLeft",
        ActionName_NegativeAction_NegativeX = "MoveRight",
        PositiveY = "MoveForward",
        NegativeY = "MoveBackward"
    };

    public static InputActionParams MouseMotinoRelative = new InputActionParams
    {
        ActionName_NegativeAction_NegativeX = InputActionType.MouseMotionRelative.ToString()
    };

    #endregion
}
