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
    [Export] public Vector3I Spawn = new(0, 150, 0);

    [Export] public PackedScene LocalPlayerScene { get; set; }
    [Export] public PackedScene ChunkManagerScene { get; set; }
    [Export] public WorldGen Generator { get; set; }
    [Export] public Texture2D[] Textures { get; set; }
    [Export] public int ViewDistance { get; set; } = 1;

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
        Generator.SetSeed(wi.Seed);
        _chunkManager = ChunkManagerScene.InstantiateOrNull<ChunkManager>();
        _chunkManager.Init(Generator, ViewDistance);
        AddChild(_chunkManager);

        //TODO: Wait for initial chunk load

        _logger.Info("Initialisation completed, spawning local player");
        var player = LocalPlayerScene.Instantiate<Player>();
        AddChild(player);
        player.GlobalPosition = Spawn;
    }
}