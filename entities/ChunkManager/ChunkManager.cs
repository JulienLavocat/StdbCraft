using System.Collections.Generic;
using System.Threading;
using Godot;

public partial class ChunkManager : Node
{
    private readonly List<Chunk> _chunks = new();

    private readonly object _playerPositionLock = new();
    private Godot.Collections.Dictionary<Chunk, Vector2I> _chunkToPosition = new();

    private Vector3 _playerPosition;
    private Godot.Collections.Dictionary<Vector2I, Chunk> _positionToChunk = new();

    private int _viewRadius = 12;

    public static ChunkManager Instance { get; private set; }

    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public FastNoiseLite Noise { get; set; }

    public override void _Ready()
    {
        Instance = this;

        var halfViewRadius = _viewRadius / 2;
        for (var i = 0; i < _viewRadius * _viewRadius; i++)
        {
            var chunk = ChunkScene.InstantiateOrNull<Chunk>();
            chunk.Noise = Noise;
            _chunks.Add(chunk);
            CallDeferred(Node.MethodName.AddChild, chunk);
            chunk.SetChunkPosition(new Vector2I(i / _viewRadius - halfViewRadius, i % _viewRadius - halfViewRadius));
        }

        new Thread(ThreadProcess).Start();
    }

    public void UpdateChunkPosition(Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
    {
        lock (_playerPositionLock)
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
        return new Vector2I(Mathf.FloorToInt(worldPosition.X / (float)Chunk.Dimensions.X),
            Mathf.FloorToInt(worldPosition.Z / (float)Chunk.Dimensions.Z));
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
                playerChunkX = Mathf.FloorToInt(_playerPosition.X / Chunk.Dimensions.X);
                playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / Chunk.Dimensions.Z);
            }

            foreach (var chunk in _chunks)
            {
                var chunkPosition = _chunkToPosition[chunk];
                var chunkX = chunkPosition.X;
                var chunkZ = chunkPosition.Y;

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

                    chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newPosition);
                }

                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }
    }
}