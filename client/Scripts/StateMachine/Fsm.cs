using Godot;
using Godot.Collections;

namespace StDBCraft.Scripts.StateMachine;

[GlobalClass]
public partial class Fsm : Node
{
    [Signal]
    public delegate void StateChangedEventHandler(FsmState newState);

    private AnimationPlayer _animationPlayer;

    private Dictionary<NodePath, FsmState> _cache = new();
    private FsmState _currentState;

    private FsmState _previousState;

    [Export]
    public FsmState CurrentState
    {
        get => _currentState;
        private set => SetCurrentState(value);
    }

    [Export]
    public AnimationPlayer AnimationPlayer
    {
        get => _animationPlayer;
        private set => SetAnimationPlayer(value);
    }


    public override void _EnterTree()
    {
        if (IsNodeReady() && IsInstanceValid(CurrentState)) EnterState(CurrentState);
    }

    public override void _Ready()
    {
        if (IsInstanceValid(CurrentState)) EnterState(CurrentState);
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(CurrentState)) CurrentState.OnProcess(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsInstanceValid(CurrentState)) CurrentState.OnPhysicsProcess(delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (IsInstanceValid(CurrentState)) CurrentState.OnInput(@event);
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(CurrentState)) ExitState(CurrentState);
    }

    public void ChangeState(NodePath path)
    {
        // Use cache value if that state has already been used in the past
        if (_cache.TryGetValue(path, out var state) && IsInstanceValid(state))
        {
            CurrentState = state;
            return;
        }

        var nextState = GetNode<FsmState>(path);
        if (!IsInstanceValid(nextState))
        {
            GD.PushError($"Node {nextState} at path {path} is not a valid FsmState");
            return;
        }

        _cache[path] = nextState;
        CurrentState = nextState;
    }

    public void PreviousState()
    {
        if (!IsInstanceValid(_previousState))
        {
            GD.PushError($"Previous state {_previousState} is not valid");
            return;
        }

        CurrentState = _previousState;
        _previousState = null;
    }

    private void OnAnimationStarted(StringName name)
    {
        if (IsInstanceValid(CurrentState))
            CurrentState.OnAnimationStarted(name);
    }

    private void OnAnimationFinished(StringName name)
    {
        if (IsInstanceValid(CurrentState))
            CurrentState.OnAnimationFinished(name);
    }

    private void OnAnimationChanged(StringName previous, StringName next)
    {
        if (IsInstanceValid(CurrentState))
            CurrentState.OnAnimationChanged(previous, next);
    }

    private void SetCurrentState(FsmState nextState)
    {
        if (IsInsideTree() && IsInstanceValid(CurrentState)) ExitState(CurrentState);

        _previousState = CurrentState;
        _currentState = nextState;

        if (!IsInstanceValid(CurrentState)) return;

        CurrentState.SetFsm(this);

        if (!IsInsideTree()) return;

        EmitSignal(SignalName.StateChanged, CurrentState);
        EnterState(CurrentState);
    }

    private void SetAnimationPlayer(AnimationPlayer player)
    {
        if (IsInstanceValid(AnimationPlayer))
        {
            AnimationPlayer.AnimationStarted -= OnAnimationStarted;
            AnimationPlayer.AnimationFinished -= OnAnimationFinished;
            AnimationPlayer.AnimationChanged -= OnAnimationChanged;
        }

        if (!IsInstanceValid(player)) return;

        AnimationPlayer = player;
        AnimationPlayer.AnimationStarted += OnAnimationStarted;
        AnimationPlayer.AnimationFinished += OnAnimationFinished;
        AnimationPlayer.AnimationChanged += OnAnimationChanged;
    }

    private static void EnterState(FsmState state)
    {
        state.OnEnter();
        state.EmitStateEntered();
    }

    private static void ExitState(FsmState state)
    {
        state.OnExit();
        state.EmitStateExited();
    }
}