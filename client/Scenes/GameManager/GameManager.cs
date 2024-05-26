using System.Linq;
using Godot;
using StDBCraft.Scripts;
using StDBCraft.Scripts.Utils;
using StdbCraft.SpacetimeDb;

namespace StDBCraft.Scenes.GameManager;

public partial class GameManager : Node
{
    private readonly Logger _logger = new(typeof(GameManager));

    [Export] public ChunkManager.ChunkManager ChunkManager { get; set; }

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
        // TODO: 
        // 4. Spawn player above the uppermost block of 0.0


        var wi = WorldInfos.Iter().First();

        BlockManager.Instance.GenerateTextureAtlas();
        ChunkManager.StartChunkGeneration(wi.Seed);
    }
}