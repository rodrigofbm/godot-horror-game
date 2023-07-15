using System.Linq;
using Godot;
using HorrorGame.Scripts.Contracts;

namespace HorrorGame.Scripts.EnemyStates;

public class PatrolState : IState
{
    private Enemy _enemy;
    
    public void Enter(CharacterBody3D character)
    {
        _enemy = (Enemy)character;
        _enemy.NavigationAgent.TargetPosition = _enemy.WayPoints.ToArray()[_enemy.CurrentWaitPointIndex].GlobalPosition;
    }

    public void Update(double delta)
    {
        if (_enemy.NavigationAgent.IsNavigationFinished())
        {
            _enemy.Timer.Start();
            _enemy.ChangeState(new WaitingState());
            return;
        }
        _enemy.HandleMovement();
    }

    public void Exit()
    {
    }
}