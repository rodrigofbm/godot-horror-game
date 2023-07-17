using System.Collections.Generic;
using System.Linq;
using Godot;
using HorrorGame.Scripts.Contracts;
using HorrorGame.Scripts.EnemyStates;

namespace HorrorGame.Scripts;

public partial class Enemy : CharacterBody3D
{
	public float PatrolSpeed { get; private set; }  = 2f;
	public  float HuntingSpeed { get; private set; }  = 2.5f;
	public  float ChasingSpeed { get; private set; }  = 3.5f;
	public IState CurrentState { get; private set; }
	public NavigationAgent3D NavigationAgent { get; private set; }
	public Player Player { get; private set; }
	public int CurrentWaitPointIndex { get; set; }
	public IEnumerable<Marker3D> WayPoints { get; private set; } = new List<Marker3D>();
	public Timer Timer { get; private set; }
	public bool IsWaitingTimeOut { get; set; }
	public Vector3 LastLookingAtDirection { get; private set; }
	public bool IsPlayerInEarshotFar { get; set; }
	public bool IsPlayerInEarshotClose { get; set; }
	public bool IsPlayerInSightFar { get; set; }
	public bool IsPlayerInSightClose { get; set; }
	public Node3D Target { get; set; }

	public Marker3D HeadPosition { get; private set; }
	//TODO: para debug
	public Label PlayerInLabel { get; set; }
	public Label EnemyState { get; set; }
	
	public override void _Ready()
	{
		LastLookingAtDirection = GlobalPosition;
		Player = GetTree().GetNodesInGroup("Player").FirstOrDefault() as Player;
		PlayerInLabel = GetTree().GetNodesInGroup("PlayerInLabel").FirstOrDefault() as Label;
		EnemyState = GetTree().GetNodesInGroup("EnemyState").FirstOrDefault() as Label;
		NavigationAgent = GetNode<NavigationAgent3D>("NavigationAgent");
		Timer = GetNode<Timer>("Timer");
		HeadPosition = GetNode<Marker3D>("HeadPos");
		WayPoints = GetTree().GetNodesInGroup("EnemyWaypoint").Select(x => x as Marker3D);
		ChangeState(new PatrolState());
	}

	public override void _Process(double delta)
	{
		CurrentState.Update(delta);
	}

	public void ChangeState(IState state)
	{
		CurrentState?.Exit();
		CurrentState = state;
		CurrentState.Enter(this);
		EnemyState.Text = state.ToString();
	}

	public void HandleMovement(float speed)
	{
		var targetPos = NavigationAgent.GetNextPathPosition();
		var direction = GlobalPosition.DirectionTo(targetPos);
		var lookingAt = LastLookingAtDirection.Lerp(targetPos, 0.1f);
		LookAt(new Vector3(lookingAt.X, GlobalPosition.Y, lookingAt.Z), Vector3.Up);
		LastLookingAtDirection = lookingAt;
		Velocity = direction * speed;
		MoveAndSlide();
	}
	
	public void CheckForPlayer()
	{
		if (!IsPlayerInEarshotFar) return;
		
	    var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(
            new PhysicsRayQueryParameters3D
            {
                From = HeadPosition.GlobalPosition,
                To = Player.Camera3D.GlobalPosition,
                Exclude = { GetRid() }
            });

        if (result.Keys.Count <= 0) return;
        var node = (Node3D)result["collider"];
        if (node is not IDetectable detectable) return;

        PlayerInLabel.Text = string.Empty;
        Target = detectable as Node3D;
        if (IsPlayerInEarshotClose)
        {
            ChangeState(new ChasingState());
            PlayerInLabel.Text = "EarshotClose";
        }
    	
        if (IsPlayerInEarshotFar && !IsPlayerInEarshotClose)
        {
            if (detectable is Player player)
            {
                if (!player.IsCrouch)
                {
                    ChangeState(new HuntingState());
                    PlayerInLabel.Text = "EarshotFar";
                }
            }
            else
            {
                ChangeState(new HuntingState());
                PlayerInLabel.Text = "EarshotFar";
            } 
        }
    	
        if (IsPlayerInSightClose)
        {
	        if (detectable is Player player)
	        {
		        if (!player.IsInDarkness)
		        {
			        ChangeState(new ChasingState());
			        PlayerInLabel.Text = "SightClose";
		        }
	        }
	        else
	        {
		        ChangeState(new ChasingState());
		        PlayerInLabel.Text = "SightClose";
	        } 
        }
    	
        if (IsPlayerInSightFar && !IsPlayerInSightClose)
        {
            ChangeState(new HuntingState());
            PlayerInLabel.Text = "InSightFar";
        }
    }
	
	private void OnWaitTimerTimeout()
	{
		IsWaitingTimeOut = true;
	}

	private void OnCloseHearingBodyEntered(Node3D node)
	{
		IsPlayerInEarshotClose = true;
	}
	
	private void OnCloseHearingBodyExited(Node3D node)
	{
		IsPlayerInEarshotClose = false;
	}
	
	private void OnFarHearingBodyEntered(Node3D node)
	{
		IsPlayerInEarshotFar = true;
	}
	
	private void OnFarHearingBodyExited(Node3D node)
	{
		IsPlayerInEarshotFar = false;
	}
	
	private void OnCloseSightBodyEntered(Node3D node)
	{
		IsPlayerInSightClose = true;
	}
	
	private void OnCloseSightBodyExited(Node3D node)
	{
		IsPlayerInSightClose = false;
	}
	
	private void OnFarSightBodyEntered(Node3D node)
	{
		IsPlayerInSightFar = true;
	}
	
	private void OnFarSightBodyExited(Node3D node)
	{
		IsPlayerInSightFar = false;
	}
}