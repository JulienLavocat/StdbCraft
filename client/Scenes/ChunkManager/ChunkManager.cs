using System.Collections.Generic;
using System.Threading;
using Godot;
using StDBCraft.Entities.Player;
using StdbCraft.SpacetimeDb;

namespace StDBCraft.Scenes.ChunkManager;

public partial class ChunkManager : Node
{
    private readonly List<Chunk.Chunk> _chunks = new();
    private readonly Godot.Collections.Dictionary<Chunk.Chunk, Vector2I> _chunkToPosition = new();

    private readonly object _playerPositionLock = new();
    private readonly Godot.Collections.Dictionary<Vector2I, Chunk.Chunk> _positionToChunk = new();

    private readonly int _viewRadius = 5;

    private Vector3 _playerPosition;

    public static ChunkManager Instance { get; private set; }

    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public FastNoiseLite Noise { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartChunkGeneration(int seed)
    {
        Noise.Seed = seed;
        var halfViewRadius = _viewRadius / 2;
        for (var i = 0; i < _viewRadius * _viewRadius; i++)
        {
            var chunk = ChunkScene.InstantiateOrNull<Chunk.Chunk>();
            chunk.Noise = Noise;
            _chunks.Add(chunk);
            CallDeferred(Node.MethodName.AddChild, chunk);
            chunk.SetChunkPosition(new Vector2I(i / _viewRadius - halfViewRadius, i % _viewRadius - halfViewRadius));
        }

        new Thread(ThreadProcess).Start();
    }

    public void UpdateChunkPosition(Chunk.Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
    {
        lock (_positionToChunk)
        {
            if (_positionToChunk.TryGetValue(previousPosition, out var chunkAtPosition) && chunkAtPosition == chunk)
                _positionToChunk.Remove(previousPosition);

            _chunkToPosition[chunk] = currentPosition;
            _positionToChunk[currentPosition] = chunk;
        }
    }

    public void SetBlock(Vector3I worldPosition, Block block)
    {
        var chunkPosition = WorldToChunk(worldPosition);
        lock (_positionToChunk)
        {
            if (_positionToChunk.TryGetValue(chunkPosition, out var chunk))
                chunk.SetBlock((Vector3I)(worldPosition - chunk.GlobalPosition), block);
        }
    }

    public static Vector2I WorldToChunk(Vector3I worldPosition)
    {
        return new Vector2I(Mathf.FloorToInt(worldPosition.X / (float)Chunk.Chunk.Dimensions.X),
            Mathf.FloorToInt(worldPosition.Z / (float)Chunk.Chunk.Dimensions.Z));
    }

    public override void _PhysicsProcess(double delta)
    {
        lock (_playerPositionLock)
        {
            _playerPosition = Player.Instance.GlobalPosition;
        }
    }

    private void ThreadProcess()
    {
        var halfViewRadius = _viewRadius / 2;

        while (IsInstanceValid(this))
        {
            int playerChunkZ;
            int playerChunkX;
            lock (_playerPositionLock)
            {
                playerChunkX = Mathf.FloorToInt(_playerPosition.X / Chunk.Chunk.Dimensions.X);
                playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / Chunk.Chunk.Dimensions.Z);
            }

            foreach (var chunk in _chunks)
            {
                if (!IsInstanceValid(chunk)) continue;

                var chunkX = 0;
                var chunkZ = 0;
                if (_chunkToPosition.TryGetValue(chunk, out var chunkPosition))
                {
                    chunkX = chunkPosition.X;
                    chunkZ = chunkPosition.Y;
                }

                var newChunkX = Mathf.PosMod(chunkX - playerChunkX + halfViewRadius, _viewRadius) + playerChunkX -
                                halfViewRadius;
                var newChunkZ = Mathf.PosMod(chunkZ - playerChunkZ + halfViewRadius, _viewRadius) + playerChunkZ -
                                halfViewRadius;

                if (newChunkX == chunkX && newChunkZ == chunkZ) continue;

                lock (_positionToChunk)
                {
                    if (_positionToChunk.ContainsKey(chunkPosition)) _positionToChunk.Remove(chunkPosition);

                    var newPosition = new Vector2I(newChunkX, newChunkZ);
                    _chunkToPosition[chunk] = newPosition;
                    _positionToChunk[newPosition] = chunk;

                    chunk.CallDeferred(nameof(Chunk.Chunk.SetChunkPosition), newPosition);
                }

                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }
    }
}