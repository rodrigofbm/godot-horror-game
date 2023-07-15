using System.Collections.Generic;
using System.Linq;
using Godot;
using HorrorGame.Scripts.Contracts;
using HorrorGame.Scripts.EnemyStates;

namespace HorrorGame.Scripts;

public partial class Enemy : CharacterBody3D
{
	public IState CurrentState { get; private set; }
	public NavigationAgent3D NavigationAgent { get; private set; }
	public Player Player { get; private set; }
	public int CurrentWaitPointIndex { get; set; }
	public IEnumerable<Marker3D> WayPoints { get; private set; } = new List<Marker3D>();
	public float PatrolSpeed { get; private set; } = 2f;
	public Timer Timer { get; private set; }
	public bool IsWaitingTimeOut { get; set; }
	
	public override void _Ready()
	{
		Player = GetTree().GetNodesInGroup("Player").FirstOrDefault() as Player;
		NavigationAgent = GetNode<NavigationAgent3D>("NavigationAgent");
		Timer = GetNode<Timer>("Timer");
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
	}

	public void HandleMovement()
	{
		var targetPos = NavigationAgent.GetNextPathPosition();
		var direction = GlobalPosition.DirectionTo(targetPos);
		Velocity = direction * PatrolSpeed;
		MoveAndSlide();
	}
	
	private void OnWaitTimerTimeout()
	{
		IsWaitingTimeOut = true;
	}
}