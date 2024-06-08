using Godot;

namespace StDBCraft.Scripts.World;

[GlobalClass]
public partial class WorldGen : Resource
{
    private const int DirtLimit = 3;
    [Export] private FastNoiseLite _continentalness;
    [Export] private Curve _height;

    public void SetSeed(int seed)
    {
        _continentalness.Seed = seed;
    }

    public int GetBlock(Vector3I position)
    {
        return GetBlock(position.X, position.Y, position.Z);
    }

    public int GetBlock(int x, int y, int z)
    {
        if (ChangesRegistry.TryGetBlock(new Vector3I(x, y, z), out var blockId)) return blockId;

        var continentalness = _continentalness.GetNoise2D(x, z);
        var terrainHeight = Mathf.RoundToInt(_height.Sample((continentalness + 1) / 2) * 200);

        if (y == terrainHeight) return 3;
        if (y < terrainHeight - DirtLimit) return 1;
        if (y >= terrainHeight - DirtLimit && y < terrainHeight) return 2;

        return 0;
    }
}