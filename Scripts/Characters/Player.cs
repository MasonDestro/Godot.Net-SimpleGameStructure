using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class Player : CharacterBody3D, IResponseToInputAction
{
    [Export]
    float WalkSpeed = 2;
    [Export]
    float RunSpeed = 5;
    [Export]
    float JumpSpeed = 5;

    [Export]
    Node3D CameraPivot;
    [Export]
    float CameraRotateSpeed = .01f;
    [Export]
    Camera3D Camera;

    [Export]
    CollisionShape3D CollisionShape;


    
    const float InteractDistance = 2.5f;

    bool _physicsEnabled = true;

    float _moveSpeed;

    public System.Collections.Generic.Dictionary<EquipSlot, IEquipment> EquipedEquipments;


    #region Godot Methods

    public override void _Ready()
    {
        EquipedEquipments = new System.Collections.Generic.Dictionary<EquipSlot, IEquipment> ();
        var esArr = GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>();
        foreach (var es in esArr)
        {
            EquipedEquipments.Add(es, null);
        }

        InitInputActionMessage();
        RegisterInputActions();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_physicsEnabled)
            return;

        if (!IsOnFloor())
        {
            Vector3 _velocity = Velocity;
            _velocity += (float)delta * Constants.Gravity3D;
            Velocity = _velocity;
        }

        MoveAndSlide();
    }

    #endregion

    #region Public Method

    public void SetPhysicsEnabled(bool enabled)
    {
        _physicsEnabled = enabled;
        CollisionShape.Disabled = !enabled;
    }

    #endregion


    #region Character Behaviours

    private void Move(double delta, Vector2 inputVector)
    {
        if (inputVector != Vector2.Zero)
        {
            RotateY(CameraPivot.Rotation.Y);
            CameraPivot.RotateY(-CameraPivot.Rotation.Y);
        }

        inputVector = inputVector.Normalized();

        Vector3 _moveDirection = inputVector.X * GlobalBasis.X + inputVector.Y * GlobalBasis.Z;

        Vector3 _velocity = Velocity;

        _velocity.X = _moveDirection.X * _moveSpeed;
        _velocity.Z = _moveDirection.Z * _moveSpeed;

        Velocity = _velocity;
    }

    private void SpeedUp(double delta, InputActionType inputType)
    {
        if (inputType == InputActionType.IsActionJustPressed)
        {
            _moveSpeed = RunSpeed;
        }
        if (inputType == InputActionType.IsActionJustReleased)
        {
            _moveSpeed = WalkSpeed;
        }
    }

    private void Jump(double delta, InputActionType inputType)
    {
        if (inputType == InputActionType.IsActionJustPressed && IsOnFloor())
        {
            Vector3 _velocity = Velocity;
            _velocity.Y = JumpSpeed;

            Velocity = _velocity;
        }
    }

    private void Interact(double delta, InputActionType inputType)
    {
        var mousePos = Camera.GetViewport().GetMousePosition();
        var fromVector = Camera.ProjectRayOrigin(mousePos);
        var toVector = fromVector + Camera.ProjectRayNormal(mousePos) * ((Camera.GlobalPosition - CameraPivot.GlobalPosition).Length() + InteractDistance);

        var query = PhysicsRayQueryParameters3D.Create(fromVector, toVector, CollisionMask);
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        if (result.Count == 0)
            return;

        var colliderObject = InstanceFromId(result["collider_id"].AsUInt64());
        if (colliderObject is IEquipment equipment)
        {
            equipment.Equip(this);
        }
    }

    private void QuitControl(double delta, InputActionType inputType)
    {
        UIManager.Instance.OpenEscPanel(this);
    }

    private void CameraRotate(double delta, Vector2 mouseMotion)
    {
        if (CameraPivot == null)
            return;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;


        (float Yaw, float Pitch) deltaRotate = (-mouseMotion.X * CameraRotateSpeed, mouseMotion.Y * CameraRotateSpeed);

        float camRotation_X = CameraPivot.Rotation.X;
        deltaRotate.Pitch = Mathf.Clamp(camRotation_X+deltaRotate.Pitch, -(float)Mathf.Pi / 4f, (float)Mathf.Pi / 2f);
        deltaRotate.Pitch -= camRotation_X;

        CameraPivot.RotateY(deltaRotate.Yaw);
        CameraPivot.RotateObjectLocal(Vector3.Right, deltaRotate.Pitch);
    }

    #endregion


    #region IResponseToInputAction

    InputActionMessage _iaMsg;
    public InputActionMessage InputActionMessage { get => _iaMsg; set => _iaMsg = value; }

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

        _iaMsg.Actions.Add(Constants.InputAction_Interact, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = Interact
        });

        _iaMsg.Actions.Add(Constants.InputAction_Esc, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = QuitControl
        });

        _iaMsg.Actions.Add(Constants.InputAction_SpeedUp, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed | InputActionType.IsActionJustReleased,
            KeyButtonMethod = SpeedUp
        });

        //_iaMsg.Actions.Add(Constants.InputAction_UseSkill, new InputActionDetail
        //{
        //    ExecuteType = InputActionExecuteType._Process,
        //    ActionType = InputActionType.IsActionJustReleased,
        //    KeyButtonMethod = UseSkill
        //});

        _iaMsg.Actions.Add(new InputActionParams
        {
            ActionName_NegativeAction_NegativeX = "Jump"
        }, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._PhysicsProcess,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = Jump
        });

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
                _moveSpeed = WalkSpeed;
                Camera.Current = true;
            }
        });
    }

    #endregion
}
