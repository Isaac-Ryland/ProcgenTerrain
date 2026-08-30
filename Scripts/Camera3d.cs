using Godot;
using System;

public partial class Camera3d : Camera3D
{
	public bool debugToggle = false; // Toggle for Debug printouts
	
	[Export] public float mouseSensitivity = 0.01f;
	[Export] public float moveSpeed = 15.0f;
	[Export] public float sprintMultiplier = 5.0f;
	
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
			_yaw -= mouseMotion.Relative.Y * mouseSensitivity;
			_pitch -= mouseMotion.Relative.X * mouseSensitivity;
			
			Rotation = new Vector3(_yaw, _pitch, _roll);
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector3 inputDir = Vector3.Zero;
		
		// Identifies what key has been pressed and applies relevant increments to a 3d vector that dictates the camera's direction
		if (Input.IsKeyPressed(Key.W)) inputDir.Z += 1f;
		if (Input.IsKeyPressed(Key.A)) inputDir.X -= 1f;
		if (Input.IsKeyPressed(Key.S)) inputDir.Z -= 1f;
		if (Input.IsKeyPressed(Key.D)) inputDir.X += 1f;
		if (Input.IsKeyPressed(Key.Space)) inputDir.Y += 1f;
		if (Input.IsKeyPressed(Key.Ctrl)) inputDir.Y -= 1f;
		
		// Checks if camera is actually moving and normalizes the direction to prevent diagonals from moving faster
		// Uses LengthSquared() here because it is more optimised than Length()
		if (inputDir.LengthSquared() > 0f)
			inputDir = inputDir.Normalized();
		
		// Gets the camera-relative direction for X & Z axis, used to make horizontal movements relative to the camera
		Vector3 forward = Transform.Basis.Z;
		Vector3 right = Transform.Basis.X;
		
		// Creates the horizontal movements seperately to avoid horizontal and vertical movement mixing
		Vector3 horizontalMove = (right * inputDir.X + forward * (-inputDir.Z));
		
		// Get the overall direction 3D vector
		Vector3 moveDir = horizontalMove + Vector3.Up * inputDir.Y;
		
		// Update the global position
		float speed = moveSpeed;
		if (Input.IsKeyPressed(Key.Shift))
			speed *= sprintMultiplier;
		GlobalPosition += moveDir * speed * (float)delta;
		
		// Debug Prints
		if (debugToggle) 
		{
			GD.Print("X Input: " + inputDir.X); 
			GD.Print("Y Input: " + inputDir.Y); 
			GD.Print("Z Input: " + inputDir.Z);
			GD.Print("Global Position: " + GlobalPosition);
		}
	}
}
