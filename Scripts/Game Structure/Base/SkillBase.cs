using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class SkillBase : Resource
{
    [Export]
    public string SkillName { get; set; }

    public IResponseToInputAction User { get; set; }
    public IEquipment Equipment { get; set; }

    public virtual void Use()
    {

    }
}
