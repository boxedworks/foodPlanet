//
using System.Collections.Generic;
using UnityEngine;

public class BlockManager
{

  public static BlockManager s_Singleton;

  public enum BlockType
  {
    NONE,

    COUNTER,
    STOVE,

    DOOR,
  }
  List<Block> _blocks;
  Dictionary<BlockType, List<Block>> _blocksByType;

  //
  public BlockManager()
  {
    s_Singleton = this;

    Block.s_Id = 0;
    _blocks ??= new();
    _blocksByType ??= new();

    // Register blocks
    var blocks = GameObject.Find("Blocks").transform;
    for (var i = 0; i < blocks.childCount; i++)
    {

      var block = blocks.GetChild(i);

      Block newBlock;
      BlockType blockType;
      switch (block.name)
      {

        case "Stove":
          blockType = BlockType.STOVE;

          newBlock = new StoveBlock(block.gameObject);
          break;

        default://case "Counter":
          blockType = BlockType.COUNTER;

          newBlock = new CounterBlock(block.gameObject);
          break;

      }

      //
      if (!_blocksByType.ContainsKey(blockType))
        _blocksByType.Add(blockType, new List<Block>());

      _blocks.Add(newBlock);
      _blocksByType[blockType].Add(newBlock);
    }
  }

  //
  public static void Update()
  {
    foreach (var block in s_Singleton._blocks)
      block.Update();
  }

  //
  public static Block GetBlock(BlockType blockType, GameObject gameObject)
  {
    var blocks = s_Singleton._blocksByType[blockType];
    foreach (var block in blocks)
    {
      if (block.Equals(gameObject)) return block;
    }
    return null;
  }
  public static Block GetBlockById(int id)
  {
    return s_Singleton._blocks[id];
  }

  public static List<Block> GetBlocksByType(BlockType blockType)
  {
    return s_Singleton._blocksByType[blockType];
  }
}