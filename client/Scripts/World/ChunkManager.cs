using System.Collections.Generic;
using System.Threading;
using Godot;
using StdbCraft.Scripts.SpacetimeDb;
using Player = StDBCraft.Scripts.Entities.Player;

namespace StDBCraft.Scripts.World;

public partial class ChunkManager : Node
{
    private readonly List<Chunk> _chunks = new();
    private readonly Godot.Collections.Dictionary<Chunk, Vector2I> _chunkToPosition = new();

    private readonly object _playerPositionLock = new();
    private readonly Godot.Collections.Dictionary<Vector2I, Chunk> _positionToChunk = new();

    private Vector3 _playerPosition;
    private int _viewDistance;

    public static ChunkManager Instance { get; private set; }

    [Export] public PackedScene ChunkScene { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartChunkGeneration(WorldGen worldGen, int viewDistance)
    {
        _viewDistance = viewDistance;
        var halfViewRadius = viewDistance / 2;
        var doubleViewRadius = viewDistance * viewDistance;

        for (var i = 0; i < doubleViewRadius; i++)
        {
            var chunk = ChunkScene.InstantiateOrNull<Chunk>();
            chunk.SetWorldGenerator(worldGen);
            chunk.SetChunkPosition(new Vector2I(i / viewDistance - halfViewRadius, i % viewDistance - halfViewRadius));
            _chunks.Add(chunk);
            CallDeferred(Node.MethodName.AddChild, chunk);
        }

        new Thread(ThreadProcess).Start();
    }

    public void UpdateChunkPosition(Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
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
        var chunkPosition = WorldUtils.ChunkFromWorldPosition(worldPosition);
        Reducer.SendBlockChange(worldPosition.X, worldPosition.Y, worldPosition.Z, block.Id);
        lock (_positionToChunk)
        {
            if (_positionToChunk.TryGetValue(chunkPosition, out var chunk))
                chunk.SetBlock((Vector3I)(worldPosition - chunk.GlobalPosition), block);
        }
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
        var halfViewRadius = _viewDistance / 2;

        while (IsInstanceValid(this))
        {
            int playerChunkZ;
            int playerChunkX;
            lock (_playerPositionLock)
            {
                playerChunkX = Mathf.FloorToInt(_playerPosition.X / Chunk.Dimensions.X);
                playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / Chunk.Dimensions.Z);
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

                var newChunkX = Mathf.PosMod(chunkX - playerChunkX + halfViewRadius, _viewDistance) + playerChunkX -
                                halfViewRadius;
                var newChunkZ = Mathf.PosMod(chunkZ - playerChunkZ + halfViewRadius, _viewDistance) + playerChunkZ -
                                halfViewRadius;

                if (newChunkX == chunkX && newChunkZ == chunkZ) continue;

                lock (_positionToChunk)
                {
                    if (_positionToChunk.ContainsKey(chunkPosition)) _positionToChunk.Remove(chunkPosition);

                    var newPosition = new Vector2I(newChunkX, newChunkZ);
                    _chunkToPosition[chunk] = newPosition;
                    _positionToChunk[newPosition] = chunk;

                    chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newPosition);
                }

                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }
    }
}