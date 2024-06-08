using Godot;

namespace StDBCraft.Scripts.World;

public static class WorldUtils
{
    public static Vector2I ChunkFromWorldPosition(Vector3I worldPosition)
    {
        return new Vector2I(Mathf.FloorToInt(worldPosition.X / (float)Chunk.Dimensions.X),
            Mathf.FloorToInt(worldPosition.Z / (float)Chunk.Dimensions.Z));
    }

    public static Vector2I ChunkFromWorldPosition(Vector3 worldPosition)
    {
        return new Vector2I(Mathf.FloorToInt(worldPosition.X / Chunk.Dimensions.X),
            Mathf.FloorToInt(worldPosition.Z / Chunk.Dimensions.Z));
    }

    public static Vector2I ChunkFromWorldPosition(float x, float z)
    {
        return new Vector2I(Mathf.FloorToInt(x / Chunk.Dimensions.X),
            Mathf.FloorToInt(z / Chunk.Dimensions.Z));
    }

    public static Vector3I WorldFromChunkPosition(Vector3I position, Vector2I chunk)
    {
        return new Vector3I(position.X + chunk.X * Chunk.Dimensions.X, position.Y,
            position.Z + chunk.Y * Chunk.Dimensions.Z);
    }

    public static Vector3I ChunkRelativePosition(Vector3 position)
    {
        var chunkPos = ChunkFromWorldPosition(position);
        return new Vector3I(Mathf.FloorToInt(position.X - chunkPos.X * Chunk.Dimensions.X),
            Mathf.FloorToInt(position.Y),
            Mathf.FloorToInt(position.Z - chunkPos.Y * Chunk.Dimensions.Z));
    }


    public static Vector3I ChunkRelativePosition(int x, int y, int z)
    {
        return ChunkRelativePosition(new Vector3(x, y, z));
    }
}