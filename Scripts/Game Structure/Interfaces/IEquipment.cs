using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GlobalTypes;

public interface IEquipment
{
    public string EquipName { get; set; }
    public EquipSlotType equipSlot {  get; set; }
    public Array<SkillBase> Skills { get; set; }

    public void Equip(IResponseToInputAction user, bool isInit = false);

    public void Unequip();
}
