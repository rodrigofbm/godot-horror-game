using System.Linq;
using Godot;
using HorrorGame.Scripts.Contracts;

namespace HorrorGame.Scripts.EnemyStates;

public class WaitingState : IState
{
    private Enemy _enemy;
    
    public void Enter(CharacterBody3D character)
    {
        _enemy = (Enemy)character;
        _enemy.Target = null;
    }

    public void Update(double delta)
    {
        if (_enemy.IsWaitingTimeOut)
        {
            _enemy.CurrentWaitPointIndex++;
            if (_enemy.CurrentWaitPointIndex > _enemy.WayPoints.Count() - 1)
            {
                _enemy.CurrentWaitPointIndex = 0;
            }
            _enemy.ChangeState(new PatrolState());
        }
    }

    public void Exit()
    {
        _enemy.IsWaitingTimeOut = false;
        _enemy.Timer.Stop();
    }
}