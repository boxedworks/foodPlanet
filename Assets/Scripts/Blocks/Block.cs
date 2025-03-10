using UnityEngine;
using BlockType = BlockManager.BlockType;

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
  public virtual void Update() { }

  //
  public bool Equals(GameObject gameObject)
  {
    return _gameObject.Equals(gameObject);
  }
}