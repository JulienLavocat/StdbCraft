using Godot;

namespace StDBCraft.Scripts.World;

public class WorldGen
{
    private readonly FastNoiseLite _noise;

    public WorldGen(FastNoiseLite noise)
    {
        _noise = noise;
    }

    public int GetBlock(Vector2 position, int y)
    {
        var groundHeight =
            (int)(Chunk.Dimensions.Y * ((_noise.GetNoise2D(position.X, position.Y) + 1f) / 2f));

        var blockId = 1; // air
        if (y == groundHeight - 1) blockId = 5;
        else if (y == groundHeight - 2) blockId = 6;
        else if (y < groundHeight - 4)
            blockId = 2;
        else if (y < groundHeight)
            blockId = 3;
        else if (y == groundHeight) blockId = 4;

        return blockId;
    }
}