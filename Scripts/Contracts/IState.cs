using Godot;

namespace HorrorGame.Scripts.Contracts;

public interface IState
{
    void Enter(CharacterBody3D character);
    void Update(double delta);
    void Exit();
}