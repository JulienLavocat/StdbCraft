using Godot;
using StDBCraft.Scripts.StateMachine;

namespace StDBCraft.Entities.Player.States;

public partial class Flying : FsmState
{
    [Export] private CharacterBody3D CharacterBody { get; set; }
    [Export] private float FlySpeed { get; set; } = 10f;

    public override void OnEnter()
    {
        GD.Print("[PlayerFSM] Entering Flying");
    }

    public override void OnProcess(double delta)
    {
        if (CharacterBody.IsOnFloor()) ChangeState("Idle");
    }

    public override void OnPhysicsProcess(double delta)
    {
        var velocity = CharacterBody.Velocity;
        velocity.Y = Input.GetAxis("Sprint", "Jump") * FlySpeed;
        CharacterBody.Velocity = velocity;

        Player.Instance.ProcessMovement(delta, FlySpeed, false);
    }

    public override void OnInput(InputEvent @event)
    {
        Player.Instance.ProcessRotationInput(@event);
    }
}