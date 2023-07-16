using Godot;
using HorrorGame.Scripts.Contracts;

namespace HorrorGame.Scripts.EnemyStates;

public class HuntingState : IState
{
    private Enemy _enemy;
    
    public void Enter(CharacterBody3D character)
    {
        _enemy = (Enemy)character;
        if (_enemy.Target != null)
        {
            _enemy.NavigationAgent.TargetPosition = _enemy.Target.GlobalPosition;
        }
    }

    public void Update(double delta)
    {
        if (_enemy.NavigationAgent.IsNavigationFinished())
        {
            _enemy.Timer.Start();
            _enemy.ChangeState(new WaitingState());
            return;
        }
        
        _enemy.HandleMovement(_enemy.HuntingSpeed);
        _enemy.CheckForPlayer();
    }

    public void Exit()
    {
    }
}