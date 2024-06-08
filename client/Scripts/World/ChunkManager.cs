using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Godot;
using StDBCraft.Scripts.Entities;
using StdbCraft.Scripts.SpacetimeDb;
using StDBCraft.Scripts.Utils;

namespace StDBCraft.Scripts.World;

public partial class ChunkManager : Node
{
    private readonly ConcurrentDictionary<Vector2I, Chunk> _chunks = new();
    private readonly Logger _logger = new(typeof(ChunkManager));
    private float _lastUpdateTime;
    private Vector3 _playerPos = Vector3.Zero;

    private int _updateTime = 1000;
    private int _viewDistance = 2;

    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public WorldGen Generator { get; set; }
    public static ChunkManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance?.QueueFree();
        Instance = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        _playerPos = Player.Instance.GlobalPosition;
        _playerPos.Y = 0;
    }

    public void Init(WorldGen generator, int viewDistance)
    {
        Generator = generator;
        _viewDistance = viewDistance;
        new Thread(UpdateChunks).Start();
    }

    private void UpdateChunks()
    {
        while (IsInstanceValid(this))
        {
            var startTime = Time.GetTicksMsec();

            var playerChunk = WorldUtils.ChunkFromWorldPosition(_playerPos);

            var minChunkX = playerChunk.X - _viewDistance;
            var maxChunkX = playerChunk.X + _viewDistance + 1;
            var minChunkZ = playerChunk.Y - _viewDistance;
            var maxChunkZ = playerChunk.Y + _viewDistance + 1;

            foreach (var chunkPos in _chunks.Keys.Where(
                         pos => pos.X < minChunkX || pos.X > maxChunkX || pos.Y < minChunkZ || pos.Y > maxChunkZ))
                if (_chunks.TryRemove(chunkPos, out var removed))
                    removed.QueueFree();


            for (var x = minChunkX; x < maxChunkX; x++)
            for (var z = minChunkZ; z < maxChunkZ; z++)
            {
                var chunkPos = new Vector2I(x, z);
                if (_chunks.ContainsKey(chunkPos)) continue;

                CallDeferred(MethodName.CreateChunk, chunkPos);

                Thread.Sleep(40);
            }

            _lastUpdateTime = Time.GetTicksMsec();
            _logger.Info("Update took ", Time.GetTicksMsec() - startTime, "ms");
            Thread.Sleep(1000);
        }
    }


    public void SetBlock(Vector3I worldPosition, Block block)
    {
        var chunkPosition = WorldUtils.ChunkFromWorldPosition(worldPosition);
        Reducer.SendBlockChange(worldPosition.X, worldPosition.Y, worldPosition.Z, block.Id);
        if (_chunks.TryGetValue(chunkPosition, out var chunk))
            chunk.SetBlock((Vector3I)(worldPosition - chunk.GlobalPosition), block);
    }

    private void CreateChunk(Vector2I position)
    {
        var chunk = ChunkScene.Instantiate<Chunk>();
        CallDeferred(Node.MethodName.AddChild, chunk);
        chunk.Init(position, Generator);
        _chunks.TryAdd(position, chunk);
    }
}