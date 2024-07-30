using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISkill
{
    public string SkillName { get; set; }

    public IResponseToInputAction User { get; set; }
    public IEquipment Equipment { get; set; }

    public void Use();
}
