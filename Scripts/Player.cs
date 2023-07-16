using Godot;
using HorrorGame.Scripts.Contracts;
using HorrorGame.Scripts.Enums;

namespace HorrorGame.Scripts;

public partial class Player : CharacterBody3D, IDetectable
{
	private const float Speed = 5.0f;
	private const float CrouchSpeed = 2.0f;
	private const float JumpVelocity = 4.5f;
	private const float MouseSensitivity = 3.0f;
	public float CurrentSpeed { get; set; }

	public Camera3D Camera3D { get; set; }
	public bool IsCrouch { get; private set; }
	public bool IsFlashlightOn { get; private set; }
	public AnimationPlayer AnimationPlayer { get; set; }

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		CurrentSpeed = Speed;
		Camera3D = GetNode<Camera3D>("Camera3D");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
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
			velocity.X = direction.X * CurrentSpeed;
			velocity.Z = direction.Z * CurrentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, CurrentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, CurrentSpeed);
		}

		HandleCrouching();
		HandleFlashlight();
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

	private void HandleCrouching()
	{
		if (Input.IsActionPressed(EInputKeyboard.Crouch.ToString()))
		{
			if (!IsCrouch)
			{
				AnimationPlayer.Play("Crouch");
				IsCrouch = true;
				CurrentSpeed = CrouchSpeed;
			}
		} else
		{
			var spaceState = GetWorld3D().DirectSpaceState;
			var result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D
				{
					From = Position,
					To = new Vector3(Position.X, Position.Y + 2f, Position.Z),
					Exclude = { GetRid() }
				});
			if (IsCrouch && result.Count <= 0)
			{
				AnimationPlayer.Play("UnCrouch");
				IsCrouch = false;
				CurrentSpeed = Speed;
			}
		}
	}

	private void HandleFlashlight()
	{
		if (Input.IsActionJustPressed(EInputKeyboard.Flashlight.ToString()))
		{
			AnimationPlayer.Play(IsFlashlightOn ? "FlashlightHide" : "FlashlightShow");

			IsFlashlightOn = !IsFlashlightOn;
		}
	}
}