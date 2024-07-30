using Godot;
using System;

public partial class Test : Node
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            GD.Print(keyEvent.AsText());
        }
    }
}
