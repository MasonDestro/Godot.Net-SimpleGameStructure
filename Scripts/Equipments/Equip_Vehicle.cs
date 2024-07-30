using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class Equip_Vehicle : CharacterBody3D, IEquipment, IResponseToInputAction
{
    #region Field

    [Export]
    float Acceleration = 5;
    [Export]
    float FrictionAcceleration = 1;
    [Export]
    float MaxSpeed = 10;
    [Export]
    Node3D Seat;

    [Export]
    Node3D CameraPivot;
    [Export]
    float CameraRotateSpeed = .01f;
    [Export]
    Camera3D Camera;

    IResponseToInputAction _user;
    Node _userNodeParent;

    InputActionDetail? _userMoveActionDetail;
    InputActionDetail? _userInteractActionDetail;
    InputActionDetail? _userCamRotateActionDetail;

    #endregion


    #region Property

    #endregion


    #region Godot Methods

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
        {
            Vector3 _velocity = Velocity;
            _velocity += (float)delta * Constants.Gravity3D;
            Velocity = _velocity;
        }

        MoveAndSlide();
    }

    #endregion


    #region Character Behaviours

    private void Move(double delta, Vector2 inputVector)
    {
        float velocityValue = Velocity.Length();
        velocityValue = Mathf.Clamp(velocityValue + (Acceleration - FrictionAcceleration) * (float)delta * inputVector.Y, 0, MaxSpeed);

        if (velocityValue != 0)
        {
            RotateObjectLocal(Vector3.Up, inputVector.X * (Mathf.Pi / 180));
        }

        Vector3 _velocity = velocityValue * GlobalBasis.Z;


        Velocity = _velocity;
    }

    private void GetOffVehicle(double delta, InputActionType inputType)
    {
        Unequip();
    }


    private void CameraRotate(double delta, Vector2 mouseMotion)
    {
        if (CameraPivot == null)
            return;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;


        (float Yaw, float Pitch) deltaRotate = (-mouseMotion.X * CameraRotateSpeed, mouseMotion.Y * CameraRotateSpeed);

        float camRotation_X = CameraPivot.Rotation.X;
        deltaRotate.Pitch = Mathf.Clamp(camRotation_X + deltaRotate.Pitch, -(float)Mathf.Pi / 4f, (float)Mathf.Pi / 4f);
        deltaRotate.Pitch -= camRotation_X;

        CameraPivot.RotateY(deltaRotate.Yaw);
        CameraPivot.RotateObjectLocal(Vector3.Right, deltaRotate.Pitch);
    }

    #endregion


    #region IEquipment

    #region Field
    string _equipName;
    EquipSlotType _equipSlotType;
    Array<SkillBase> _skills;
    #endregion

    public string EquipName { get => _equipName; set => _equipName = value; }
    public EquipSlotType equipSlot { get => _equipSlotType; set => _equipSlotType = value; }
    public Array<SkillBase> Skills { get => _skills; set => _skills = value; }

    public void Equip(IResponseToInputAction user, bool isInit = false)
    {
        if (_user != null)
        {
            Unequip();
        }

        _user = user;

        if (user is Player player)
        {
            player.SetPhysicsEnabled(false);

            _userNodeParent = player.GetParent();
            _userNodeParent.RemoveChild(player);
            Seat.AddChild(player);

            player.Position = Vector3.Zero;
            player.Rotation = Vector3.Zero;
        }
        else if (user is Enemy enemy)
        {
            enemy.SetPhysicsEnabled(false);

            _userNodeParent = enemy.GetParent();
            _userNodeParent.RemoveChild(enemy);
            Seat.AddChild(enemy);

            enemy.Position = Vector3.Zero;
            enemy.Rotation = Vector3.Zero;
        }

        _userMoveActionDetail = _user.InputActionMessage.Actions[Constants.GetVector_MoveHorizontal];
        _userInteractActionDetail = _user.InputActionMessage.Actions[Constants.InputAction_Interact];
        _userCamRotateActionDetail = _user.InputActionMessage.Actions[Constants.MouseMotinoRelative];

        _user.InputActionMessage.Actions[Constants.GetVector_MoveHorizontal] = new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._PhysicsProcess,
            ActionType = InputActionType.GetVector,
            Vector2Method = Move
        };
        _user.InputActionMessage.Actions[Constants.InputAction_Interact] = new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = GetOffVehicle
        };
        _user.InputActionMessage.Actions[Constants.MouseMotinoRelative] = new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._UnhandledInput,
            ActionType = InputActionType.MouseMotionRelative,
            Vector2Method = CameraRotate,

            OnRegister = () =>
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                Camera.Current = true;
            }
        };

        if (!isInit)
            _user.RegisterInputActions();
    }

    public void Unequip()
    {
        if (_user == null)
            return;

        if (_user is Player player)
        {
            Seat.RemoveChild(player);
            _userNodeParent.AddChild(player);

            player.GlobalPosition = GlobalPosition + (new Vector3(2, 1.1f, 0)).Rotated(Vector3.Up, Rotation.Y);
            player.GlobalRotation = GlobalRotation;

            player.SetPhysicsEnabled(true);
        }
        else if(_user is Enemy enemy)
        {
            Seat.RemoveChild(enemy);
            _userNodeParent.AddChild(enemy);

            enemy.GlobalPosition = GlobalPosition + (new Vector3(2, 1.1f, 0)).Rotated(Vector3.Up, Rotation.Y);
            enemy.GlobalRotation = GlobalRotation;

            enemy.SetPhysicsEnabled(true);
        }


        if (_userMoveActionDetail.HasValue)
        {
            _user.InputActionMessage.Actions[Constants.GetVector_MoveHorizontal] = _userMoveActionDetail.Value;
        }
        else
        {
            _user.InputActionMessage.Actions.Remove(Constants.GetVector_MoveHorizontal);
        }
        if (_userInteractActionDetail.HasValue)
        {
            _user.InputActionMessage.Actions[Constants.InputAction_Interact] = _userInteractActionDetail.Value;
        }
        else
        {
            _user.InputActionMessage.Actions.Remove(Constants.InputAction_Interact);
        }
        if (_userCamRotateActionDetail.HasValue)
        {
            _user.InputActionMessage.Actions[Constants.MouseMotinoRelative] = _userCamRotateActionDetail.Value;
        }
        else
        {
            _user.InputActionMessage.Actions.Remove(Constants.MouseMotinoRelative);
        }
        _user.RegisterInputActions();

        _user = null;
        _userCamRotateActionDetail = null;
        _userInteractActionDetail = null;
        _userMoveActionDetail = null;

        CameraPivot.Rotation = new Vector3(Mathf.Pi / 6f, 0, 0);
        Velocity = Vector3.Zero;
    }

    #endregion

    #region IResponseToInputAction

    InputActionMessage _iaMsg;
    public InputActionMessage InputActionMessage { 
        get
        {
            if (_iaMsg.InstanceID == 0)
                InitInputActionMessage();
            return _iaMsg;
        } 
        set { _iaMsg = value; }}

    public void RegisterInputActions()
    {
        InputManager.Instance.RegisterInputMethods(InputActionMessage);
    }

    public void InitInputActionMessage()
    {
        DefaultMessage();
    }

    public void DefaultMessage()
    {
        _iaMsg = new()
        {
            InstanceID = GetInstanceId(),
            Actions = new()
        };

        _iaMsg.Actions.Add(Constants.GetVector_MoveHorizontal, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._PhysicsProcess,
            ActionType = InputActionType.GetVector,
            Vector2Method = Move
        });

        _iaMsg.Actions.Add(Constants.MouseMotinoRelative, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._UnhandledInput,
            ActionType = InputActionType.MouseMotionRelative,
            Vector2Method = CameraRotate,

            OnRegister = () =>
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                Camera.Current = true;
            }
        });
    }

    #endregion
}
