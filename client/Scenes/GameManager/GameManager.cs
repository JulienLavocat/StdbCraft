using System.Linq;
using Godot;
using StDBCraft.Entities.Player;
using StDBCraft.Scripts;
using StDBCraft.Scripts.Utils;
using StdbCraft.SpacetimeDb;

namespace StDBCraft.Scenes.GameManager;

public partial class GameManager : Node
{
    private readonly Logger _logger = new(typeof(GameManager));

    private ChunkManager.ChunkManager _chunkManager;

    [Export] public PackedScene LocalPlayerScene { get; set; }
    [Export] public PackedScene ChunkManagerScene { get; set; }
    [Export] public FastNoiseLite Noise { get; set; }

    public override void _Ready()
    {
        StDb.OnReady += InitialiseWorld;
        StDb.Connect();
    }

    public override void _Process(double delta)
    {
        StDb.Update();
    }

    private void InitialiseWorld()
    {
        BlockManager.Instance.GenerateTextureAtlas();

        var wi = WorldInfos.Iter().First();
        _chunkManager = ChunkManagerScene.Instantiate<ChunkManager.ChunkManager>();
        _chunkManager.Noise = Noise;
        AddChild(_chunkManager);
        _chunkManager.StartChunkGeneration(wi.Seed);

        var player = LocalPlayerScene.Instantiate<Player>();
        AddChild(player);
        player.GlobalPosition = new Vector3(0, 100, 0);
    }
}