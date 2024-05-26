using Godot;

public partial class DebugInfo : Label
{
    [Export] public Vector3 Step = new(0.1f, 0.1f, 0.1f);

    public override void _Process(double delta)
    {
        var playerPos = StDBCraft.Entities.Player.Player.Instance.GlobalPosition;
        Vector3? lookingAt = StDBCraft.Entities.Player.Player.Instance.BlockHighlight.Visible
            ? StDBCraft.Entities.Player.Player.Instance.BlockHighlight.GlobalPosition - new Vector3(0.5f, 0.5f, 0.5f)
            : null;
        Text =
            @$"FPS: {Engine.GetFramesPerSecond()}
Position: ({playerPos.X:F2},{playerPos.Y:F2},{playerPos.Z:F2})
Chunk: {ChunkManager.WorldToChunk((Vector3I)StDBCraft.Entities.Player.Player.Instance.GlobalPosition)}
LookingAt: {lookingAt}
";
    }
}