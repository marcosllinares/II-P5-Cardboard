using UnityEngine;

public class CardboardPointerRelay : MonoBehaviour
{
    ILookSelectable selectable;

    void Awake()
    {
        // Busca el interactuable en este objeto o en su jerarquía
        selectable = GetComponentInParent<ILookSelectable>();
    }

    public void OnPointerEnter() => selectable?.OnLookEnter();
    public void OnPointerExit()  => selectable?.OnLookExit();
    public void OnPointerClick() => selectable?.OnLookSelect();
}
