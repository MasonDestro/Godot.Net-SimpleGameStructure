using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class Equip_Drone : CharacterBody3D, IEquipment, IResponseToInputAction
{
    #region Field

    Node3D _originParent;

    IResponseToInputAction _user;
    InputActionDetail? _userUseSkillActionDetail;

    Node3D _target;
    InputActionDetail? _targetEscActionDetail;

    bool _isDeployed;
    bool _isUsing;

    #endregion

    #region Property

    [Export]
    float Speed = 8;

    [Export]
    Node3D CameraPivot;
    [Export]
    float CameraRotateSpeed = .01f;
    [Export]
    Camera3D Camera;

    [Export]
    AnimationPlayer animPlayer;
    [Export]
    CollisionShape3D collisionShape;

    #endregion

    #region Godot Methods

    public override void _Ready()
    {
        InitInputActionMessage();

        animPlayer.AnimationFinished += (StringName animName) =>
        {
            var gt = GlobalTransform;

            _originParent = GetParent<Node3D>();
            _originParent.RemoveChild(this);
            Node3DRoot.Instance.AddChild(this);

            GlobalTransform = gt;
            collisionShape.Disabled = false;

            RegisterInputActions();
        };

        collisionShape.Disabled = _user != null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target != null)
        {
            Vector2 fakeMouseMotion = Vector2.Zero;

            Vector3 drone2Target = (_target.GlobalPosition - GlobalPosition).Normalized();
            Vector3 droneForward = GlobalBasis.Z.Normalized();

            drone2Target.Y = droneForward.Y;
            float lengthSquared = (drone2Target - droneForward).LengthSquared();
            float radian = Mathf.Acos((2 - lengthSquared) / 2);

            if (drone2Target.Cross(droneForward).Y > 0)
            {
                radian *= -1;
            }

            fakeMouseMotion.X = -radian / CameraRotateSpeed;


            Vector3 cam2Target = (_target.GlobalPosition - Camera.GlobalPosition).Normalized();
            Vector3 camForward = (-Camera.GlobalBasis.Z).Normalized();

            lengthSquared = (cam2Target - camForward).LengthSquared();
            radian = Mathf.Acos((2 - lengthSquared) / 2);

            if (cam2Target.Cross(camForward).Y > 0)
            {
                radian *= -1;
            }

            fakeMouseMotion.Y = radian / CameraRotateSpeed;

            CameraRotate(0, fakeMouseMotion);
        }

        if (_user == null && _target == null && !_isUsing && !IsOnFloor())
        {
            Vector3 _velocity = Velocity;
            _velocity += (float)delta * Constants.Gravity3D;
            Velocity = _velocity;
        }

        MoveAndSlide();
    }

    #endregion

    #region Character Behaviours

    private void DeployDrone(double delta, InputActionType inputType)
    {
        if (!_isDeployed)
            animPlayer.Play("Drone Deploy");
        else
            RegisterInputActions();

        _isDeployed = true;
    }

    private void Move_Horizontal(double delta, Vector2 inputVector)
    {
        inputVector = inputVector.Normalized();
        Vector3 _moveDirection = inputVector.X * GlobalBasis.X + inputVector.Y * GlobalBasis.Z;

        Vector3 _velocity = Velocity;

        _velocity.X = _moveDirection.X * Speed;
        _velocity.Z = _moveDirection.Z * Speed;

        Velocity = _velocity;
    }
    private void Move_Vertical(double delta, float axisValue)
    {
        Vector3 _velocity = Velocity;

        _velocity.Y = axisValue * Speed;

        Velocity = _velocity;
    }

    private void UseSkill(double delta, InputActionType inputType)
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        //*

        var mousePos = Camera.GetViewport().GetMousePosition();

        var fromVector = Camera.ProjectRayOrigin(mousePos);
        var toVector = fromVector + 100 * Camera.ProjectRayNormal(mousePos);

        /*/

        Vector3 fromVector = Camera.GlobalPosition;
        Vector3 toVector = fromVector - Camera.GlobalBasis.Z.Normalized() * 100;

        //*/

        var query = PhysicsRayQueryParameters3D.Create(fromVector, toVector, CollisionMask);
        var result = spaceState.IntersectRay(query);

        if (result.Count == 0)
            return;

        var colliderObject = InstanceFromId(result["collider_id"].AsUInt64());
        if (colliderObject is IResponseToInputAction ir2ia && colliderObject is not Player)
        {

            _target = colliderObject as Node3D;

            if (ir2ia.InputActionMessage.Actions.ContainsKey(Constants.InputAction_Esc))
                _targetEscActionDetail = ir2ia.InputActionMessage.Actions[Constants.InputAction_Esc];

            ir2ia.InputActionMessage.Actions[Constants.InputAction_Esc] = new ()
            {
                ExecuteType = InputActionExecuteType._Process,
                ActionType = InputActionType.IsActionJustPressed,
                KeyButtonMethod = (double delta, InputActionType inputType) =>
                {
                    RegisterInputActions();

                    if (_targetEscActionDetail == null)
                    {
                        ir2ia.InputActionMessage.Actions.Remove(Constants.InputAction_Esc);
                    }
                    else
                    {
                        ir2ia.InputActionMessage.Actions[Constants.InputAction_Esc] = _targetEscActionDetail.Value;
                    }

                    _target = null;
                    _targetEscActionDetail = null;
                }
            };

            ir2ia.RegisterInputActions();
        }
    }

    private void QuitControl(double delta, InputActionType inputType)
    {
        _user.RegisterInputActions();

        Velocity = Vector3.Zero;
    }

    private void CameraRotate(double delta, Vector2 mouseMotion)
    {
        if (CameraPivot == null)
            return;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
            return;


        (float Yaw, float Pitch) deltaRotate = (-mouseMotion.X * CameraRotateSpeed, mouseMotion.Y * CameraRotateSpeed);

        float camRotation_X = CameraPivot.Rotation.X;
        deltaRotate.Pitch = Mathf.Clamp(camRotation_X + deltaRotate.Pitch, 0, (float)Mathf.Pi / 2f);
        deltaRotate.Pitch -= camRotation_X;

        this.RotateY(deltaRotate.Yaw);
        CameraPivot.RotateObjectLocal(Vector3.Right, deltaRotate.Pitch);
    }

    #endregion


    #region Private Methods

    private void OnUnregistered()
    {
        _isUsing = false;
    }

    #endregion


    #region IEquipment

    #region Field
    string _equipName;
    EquipSlotType _equipSlot;
    Array<SkillBase> _skills;
    #endregion

    public string EquipName { get => _equipName; set => _equipName = value; }
    public EquipSlotType equipSlot { get => _equipSlot; set => _equipSlot = value; }

    public Array<SkillBase> Skills { get => _skills; set => _skills = value; }

    public void Equip(IResponseToInputAction user, bool isInit = false)
    {
        _user = user;

        if (user is Player player)
        {
            foreach (var es in player.GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>())
            {
                if (es.SlotType == EquipSlotType.Back)
                {
                    player.EquipedEquipments[es]?.Unequip();

                    collisionShape.Disabled = true;

                    GetParent().RemoveChild(this);
                    es.AddChild(this);

                    Position = Vector3.Zero;
                    RotationDegrees = new Vector3(-90, 0, 0);

                    player.EquipedEquipments[es] = this;

                    break;
                }
            }
        }

        if (_user.InputActionMessage.Actions.ContainsKey(Constants.InputAction_UseSkill))
            _userUseSkillActionDetail = _user.InputActionMessage.Actions[Constants.InputAction_UseSkill];

        _user.InputActionMessage.Actions[Constants.InputAction_UseSkill] = new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustReleased,
            KeyButtonMethod = DeployDrone
        };

        if(!isInit)
            _user.RegisterInputActions();
    }

    public void Unequip()
    {
        Transform3D gt = GlobalTransform;

        GetParent().RemoveChild(this);
        Node3DRoot.Instance.AddChild(this);

        GlobalTransform = gt;

        collisionShape.Disabled = false;

        if (_userUseSkillActionDetail.HasValue)
        {
            _user.InputActionMessage.Actions[Constants.InputAction_UseSkill] = _userUseSkillActionDetail.Value;
        }
        else
        {
            _user.InputActionMessage.Actions.Remove(Constants.InputAction_UseSkill);
        }

        _user = null;
        _userUseSkillActionDetail = null;

        _isDeployed = false;
        _isUsing = false;

        Velocity = Vector3.Zero;
    }


    #endregion


    #region IResponseToInputAction

    InputActionMessage _iaMsg;
    public InputActionMessage InputActionMessage { get => _iaMsg; set => _iaMsg = value; }

    public void RegisterInputActions()
    {
        InputManager.Instance.RegisterInputMethods(InputActionMessage);

        Input.MouseMode = Input.MouseModeEnum.Captured;
        Camera.Current = true;

        _isUsing = true;
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
            Actions = new(),
            OnUnregistered = OnUnregistered
        };

        _iaMsg.Actions.Add(Constants.InputAction_Esc, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = QuitControl
        });

        _iaMsg.Actions.Add(Constants.InputAction_UseSkill, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustReleased,
            KeyButtonMethod = UseSkill
        });

        _iaMsg.Actions.Add(Constants.GetAxis_MoveVertical, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._PhysicsProcess,
            ActionType = InputActionType.GetAxis,
            FloatMethod = Move_Vertical
        });

        _iaMsg.Actions.Add(Constants.GetVector_MoveHorizontal, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._PhysicsProcess,
            ActionType = InputActionType.GetVector,
            Vector2Method = Move_Horizontal
        });

        _iaMsg.Actions.Add(Constants.MouseMotinoRelative, new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._UnhandledInput,
            ActionType = InputActionType.MouseMotionRelative,
            Vector2Method = CameraRotate
        });
    }


    #endregion
}
