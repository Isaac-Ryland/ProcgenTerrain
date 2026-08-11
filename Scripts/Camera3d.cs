using Godot;
using System;

public partial class Camera3d : Camera3D
{
	[Export] public float MouseSensitivity = 0.01f; // Placeholder value for now
	[Export] public float MoveSpeed = 5.0f; // Placeholder value for now
	
	private float _yaw = 0f;
	private float _pitch = 0f;
	private float _roll = 0f; // considering adding rolling binds as well, likely Q/E. For now is just here to remove a magic number
	
	public override void _Ready()
	{
		// Kidnapping the mouse to stop it from being shown and leaving the window
		Input.MouseMode = Input.MouseModeEnum.Captured; // Yoink :>
		
		// Initialising rotation
		_yaw = Rotation.Y;
		_pitch = Rotation.X;
		// _roll = Rotation.Z;
	}
	
	public override void _Input(InputEvent @event)
	{
		// Press Escape to unlock mouse from window
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
		
		// Mouse look, only if the mouse is captured
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			_yaw -= mouseMotion.Relative.Y * MouseSensitivity;
			_pitch -= mouseMotion.Relative.X * MouseSensitivity;
			
			Rotation = new Vector3(_yaw, _pitch, _roll);
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector3 InputDir = Vector3.Zero;
		
		// Identifies what key has been pressed and applies relevant increments to a 3d vector that dictates the camera's direction
		if (Input.IsKeyPressed(Key.W)) InputDir.Z += 1f;
		if (Input.IsKeyPressed(Key.A)) InputDir.X -= 1f;
		if (Input.IsKeyPressed(Key.S)) InputDir.Z -= 1f;
		if (Input.IsKeyPressed(Key.D)) InputDir.X += 1f;
		if (Input.IsKeyPressed(Key.Space)) InputDir.Y += 1f;
		if (Input.IsKeyPressed(Key.Ctrl)) InputDir.Y -= 1f;
		
		// Checks if camera is actually moving and normalizes our direction to prevent diagonals from moving faster
		// Uses LengthSquared() here because it is more optimised than Length()
		if (InputDir.LengthSquared() > 0f)
			InputDir = InputDir.Normalized();
		
		// Gets the camera-relative direction for X & Z axis, used to make horizontal movements relative to the camera
		Vector3 Forward = Transform.Basis.Z;
		Vector3 Right = Transform.Basis.X;
		
		// 
		Vector3 horizontalMove = (Right * InputDir.X + Forward * (-InputDir.Z));
		
		// Get the overall direction 3D vector
		Vector3 MoveDir = horizontalMove + Vector3.Up * InputDir.Y;
		
		// Update the global position
		GlobalPosition += MoveDir * MoveSpeed * (float)delta;
	}
}
