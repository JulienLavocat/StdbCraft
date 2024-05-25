using Godot;

public partial class DebugInfo : Label
{
    [Export] public Vector3 Step = new(0.1f, 0.1f, 0.1f);

    public override void _Process(double delta)
    {
        var playerPos = Player.Instance.GlobalPosition;
        Text =
            $"FPS: {Engine.GetFramesPerSecond()}\nPosition: ({playerPos.X:F2},{playerPos.Y:F2},{playerPos.Z:F2})\nChunk: {ChunkManager.WorldToChunk((Vector3I)Player.Instance.GlobalPosition)}";
    }
}