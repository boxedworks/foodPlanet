//
using System.Collections.Generic;
using UnityEngine;

class BlockManager
{

  public static BlockManager s_Singleton;

  public enum BlockType
  {
    NONE,

    COUNTER,
  }
  List<Block> _blocks;
  Dictionary<BlockType, List<Block>> _blocksByType;

  public class Block
  {
    public static int s_Id;
    public int _Id;

    public BlockType _Type;

    protected GameObject _gameObject;
    public Block(BlockType blockType, GameObject gameObject)
    {
      _Id = s_Id++;
      _Type = blockType;

      _gameObject = gameObject;
    }

    //
    public bool Equals(GameObject gameObject)
    {
      return _gameObject.Equals(gameObject);
    }
  }

  //
  public class CounterBlock : Block
  {

    GameObject _heldObject;
    public bool _HasObject { get { return _heldObject != null; } }

    public CounterBlock(GameObject gameObject) : base(BlockType.COUNTER, gameObject)
    {

    }

    //
    public void SetObject(GameObject gameObject)
    {
      _heldObject = gameObject;
      _heldObject.transform.position = _gameObject.transform.position + new Vector3(0f, 2f, 0f);
    }
    public void UnsetObject()
    {
      _heldObject = null;
    }

    public bool HasThisObject(GameObject gameObject)
    {
      if (!_HasObject) return false;
      return _heldObject.Equals(gameObject);
    }
  }

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