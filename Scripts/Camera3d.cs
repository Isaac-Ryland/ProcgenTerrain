using Godot;
using System;

public partial class Camera3d : Camera3D
{
	[Export] public float MouseSensitivity = 1.0f;
	[Export] public float MoveSpeed = 5.0f;
	[Export] public float SprintMultiplier = 3.0f;
	
	private float _yaw = 0f;
	private float _pitch = 0f;
	
	public override void _Ready()
	{
		
	}
	
}
