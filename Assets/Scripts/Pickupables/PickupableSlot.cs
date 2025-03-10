

//
class PickupableSlot
{

  public Pickupable _Pickupable;
  public bool _HasObject { get { return _Pickupable != null; } }

  //
  public PickupableSlot()
  {

  }

  //
  public void Set(Pickupable pickupable)
  {
    _Pickupable = pickupable;
  }
  public void Unset()
  {
    _Pickupable = null;
  }
}