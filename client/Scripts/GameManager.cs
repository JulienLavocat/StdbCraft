using System.Linq;
using Godot;
using StdbCraft.Scripts.SpacetimeDb;
using StDBCraft.Scripts.Utils;
using StDBCraft.Scripts.World;
using ChunkManager = StDBCraft.Scripts.World.ChunkManager;
using Player = StDBCraft.Scripts.Entities.Player;

namespace StDBCraft.Scripts;

public partial class GameManager : Node
{
    private readonly Logger _logger = new(typeof(GameManager));

    private ChunkManager _chunkManager;

    [Export] public PackedScene LocalPlayerScene { get; set; }
    [Export] public PackedScene ChunkManagerScene { get; set; }
    [Export] public FastNoiseLite Noise { get; set; }
    [Export] public Texture2D[] Textures { get; set; }

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
        _logger.Info("Initialising world");
        BlockManager.SetTextures(Textures);
        BlockManager.GenerateTextureAtlas();


        var wi = WorldInfos.Iter().First();
        _chunkManager = ChunkManagerScene.Instantiate<ChunkManager>();
        _chunkManager.Noise = Noise;
        AddChild(_chunkManager);
        _chunkManager.StartChunkGeneration(wi.Seed);

        _logger.Info("Initialisation completed, spawning local player");
        var player = LocalPlayerScene.Instantiate<Player>();
        AddChild(player);
        player.GlobalPosition = new Vector3(0, 100, 0);
    }
}