using UnityEngine;

public class Magnet : MonoBehaviour, ILookSelectable
{
    public void OnLookEnter() { }
    public void OnLookExit()  { }

    public void OnLookSelect()
    {
        Inventory.Instance.SummonToPlayer();
    }
}
