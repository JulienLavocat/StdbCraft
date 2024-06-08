using System.Linq;
using StdbCraft.Scripts.SpacetimeDb;
using StDBCraft.Scripts.Utils;

namespace StDBCraft.Scripts.World;

public static class BlockManager
{
    private static readonly Logger Logger = new(typeof(BlockManager));

    public static Block[] Blocks { get; private set; }

    public static void LoadBlocks()
    {
        var blocks = Block.Iter().ToList();
        // This block is just the air, it's not added in the server
        blocks.Insert(0, new Block
        {
            Back = -1,
            Front = -1,
            Left = -1,
            Right = -1,
            Bottom = -1,
            Top = -1,
            IsTransparent = true,
            Id = 0
        });

        Blocks = blocks.ToArray();

        Logger.Info($"Loaded {Blocks.Length} blocks");
    }

    public static Block GetAir()
    {
        return Blocks[0];
    }
}