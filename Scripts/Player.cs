using Godot;
using HorrorGame.Scripts.Enums;

namespace HorrorGame.Scripts;

public partial class Player : CharacterBody3D
{
	private const float Speed = 5.0f;
	private const float JumpVelocity = 4.5f;
	private const float MouseSensitivity = 3.0f;
	public Camera3D Camera3D { get; set; }

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		Camera3D = GetNode<Camera3D>("Camera3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var inputDir = Input.GetVector(EInputKeyboard.MoveLeft.ToString(), EInputKeyboard.MoveRight.ToString(), EInputKeyboard.MoveForward.ToString(), EInputKeyboard.MoveBackward.ToString());

		var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion mouseEventMouseMotion)
		{
			// Horizontal look
			var yOffset = Rotation.Y - mouseEventMouseMotion.Relative.X / 1000f * MouseSensitivity;
			Rotation = new Vector3(Rotation.X, yOffset, Rotation.Z);
			
			// Vertical look
			var xOffset = Camera3D.Rotation.X - mouseEventMouseMotion.Relative.Y / 1000f * MouseSensitivity;
			Camera3D.Rotation = new Vector3(Mathf.Clamp(xOffset, -2f, 2f), Camera3D.Rotation.Y, Camera3D.Rotation.Z);
		}
	}
}