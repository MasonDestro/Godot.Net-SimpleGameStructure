using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class Equip_Spear_Area : Area3D, IEquipment
{
    [Export]
    string equipRigiPath;
    [Export]
    AnimationPlayer animPlayer;

    IResponseToInputAction _user;
    InputActionDetail? _userAttackActionDetail;
    EquipSlot _slot;

    bool _isAttacking;
    Array<Node3D> _NodesEntered;

    #region Godot Methods

    public override void _EnterTree()
    {
        
    }

    public override void _Ready()
    {
        _NodesEntered = new Array<Node3D>();

        BodyEntered += OnBodyEntered;

        animPlayer.AnimationFinished += (StringName animName) =>
        {
            if (animName == "Attack")
            {
                _isAttacking = false;
            }
        };
    }

    #endregion


    #region Private Methods
    private void Attack(double delta, InputActionType inputType)
    {
        if (!animPlayer.IsPlaying())
        {
            animPlayer.Play("Attack");
            _isAttacking = true;

            foreach (var node in _NodesEntered)
            {
                if ((node is IResponseToInputAction ir2ia && ir2ia != _user) && node is Enemy enemy)
                {
                    GD.Print("Hit Enemy");

                    enemy.Die();
                }
            }
        }
    }

    private void OnBodyEntered(Node3D node3D)
    {
        if (!_isAttacking)
        {
            _NodesEntered.Add(node3D);
            return;
        }

        if ((node3D is IResponseToInputAction ir2ia && ir2ia != _user) && node3D is Enemy enemy)
        {
            GD.Print("Hit Enemy");

            enemy.Die();
        }
    }

    private void OnBodyExited(Node3D node3D)
    {
        _NodesEntered.Remove(node3D);
    }

    #endregion


    #region IEquipment

    #region Field
    string _equipName;
    EquipSlotType _equipSlotType;
    Array<SkillBase> _skills;
    #endregion

    [Export]
    public string EquipName { get => _equipName; set => _equipName = value; }
    [Export]
    public EquipSlotType equipSlot { get => _equipSlotType; set => _equipSlotType = value; }
    public Array<SkillBase> Skills { get => _skills; set => _skills = value; }


    public void Equip(IResponseToInputAction user, bool isInit = false)
    {
        _user = user;

        if (user is Player player)
        {
            foreach (var es in player.GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>())
            {
                if (es.SlotType == equipSlot)
                {
                    _slot = es;

                    GetParent()?.RemoveChild(this);
                    es.AddChild(this);

                    Position = Vector3.Zero;
                    Rotation = Vector3.Zero;

                    player.EquipedEquipments[es] = this;
                    animPlayer.Play("Idel");

                    break;
                }
            }
        }
        else if (user is Enemy enemy)
        {
            foreach (var es in enemy.GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>())
            {
                if (es.SlotType == equipSlot)
                {
                    _slot = es;

                    GetParent()?.RemoveChild(this);
                    es.AddChild(this);

                    Position = Vector3.Zero;
                    Rotation = Vector3.Zero;

                    enemy.EquipedEquipments[es] = this;
                    animPlayer.Play("Idel");

                    break;
                }
            }
        }

        if (_user.InputActionMessage.Actions.ContainsKey(Constants.InputAction_Attack))
            _userAttackActionDetail = _user.InputActionMessage.Actions[Constants.InputAction_Attack];

        _user.InputActionMessage.Actions[Constants.InputAction_Attack] = new InputActionDetail
        {
            ExecuteType = InputActionExecuteType._Process,
            ActionType = InputActionType.IsActionJustPressed,
            KeyButtonMethod = Attack
        };

        if (!isInit)
            _user.RegisterInputActions();
    }

    public void Unequip()
    {
        if (_userAttackActionDetail.HasValue)
        {
            _user.InputActionMessage.Actions[Constants.InputAction_Attack] = _userAttackActionDetail.Value;
        }
        else
        {
            _user.InputActionMessage.Actions.Remove(Constants.InputAction_Attack);
        }

        if (_user is Player player)
        {
            player.EquipedEquipments[_slot] = null;
        }
        else if (_user is Enemy enemy)
        {
            enemy.EquipedEquipments[_slot] = this;
        }

        _user = null;
        _userAttackActionDetail = null;
        _slot = null;

        var equipInst = ResourceLoader.Load<PackedScene>(equipRigiPath).Instantiate<Equip_Common_Rigi>();
        Node3DRoot.Instance.AddChild(equipInst);

        equipInst.GlobalTransform = GlobalTransform;

        QueueFree();
    }


    #endregion
}
