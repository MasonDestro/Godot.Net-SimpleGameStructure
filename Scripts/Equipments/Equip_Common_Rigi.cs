using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class Equip_Common_Rigi : RigidBody3D, IEquipment
{
    [Export]
    PackedScene equipArea;

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
        if (user is Player player)
        {
            foreach (var es in player.GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>())
            {
                if (es.SlotType == EquipSlotType.RightHand)
                {
                    player.EquipedEquipments[es]?.Unequip();

                    var equipInst = equipArea.Instantiate();
                    //Node3DRoot.Instance.AddChild(equipInst);

                    if (equipInst is IEquipment equip)
                        equip.Equip(user, isInit);

                    QueueFree();

                    break;
                }
            }
        }
        else if (user is Enemy enemy)
        {
            foreach (var es in enemy.GetMeta(nameof(EquipSlot), new Array<EquipSlot>()).As<Array<EquipSlot>>())
            {
                if (es.SlotType == EquipSlotType.RightHand)
                {
                    enemy.EquipedEquipments[es]?.Unequip();

                    var equipInst = equipArea.Instantiate();
                    //Node3DRoot.Instance.AddChild(equipInst);

                    if (equipInst is IEquipment equip)
                        equip.Equip(user, isInit);

                    QueueFree();

                    break;
                }
            }
        }
    }

    public void Unequip()
    {

    }


    #endregion
}
