using Godot;
using System;

public partial class Node3DRoot : Node3D
{
	public static Node3DRoot Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
		}
	}
}
