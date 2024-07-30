using Godot;
using Godot.Collections;
using System;
using static GlobalTypes;

public partial class EquipSlot : Node3D
{
	[Export]
	public EquipSlotType SlotType;

    public override void _EnterTree()
    {
        Array<EquipSlot> arr = new Array<EquipSlot>();

        if (Owner.HasMeta(nameof(EquipSlot)))
        {
            arr = Owner.GetMeta(nameof(EquipSlot)).As<Array<EquipSlot>>();
        }

        arr.Add(this);

        Owner.SetMeta(nameof(EquipSlot), arr);
    }

    public override void _ExitTree()
    {
        var arr = Owner.GetMeta(nameof(EquipSlot)).As<Array<EquipSlot>>();
        arr.Remove(this);
        if (arr.Count == 0)
        {
            Owner.RemoveMeta(nameof(EquipSlot));
        }
        else
        {
            Owner.SetMeta(nameof(EquipSlot), arr);
        }
    }
}
