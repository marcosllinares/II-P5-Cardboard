using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour, ILookSelectable
{
    public float pickupDistance = 4f;
    public float attractSpeed = 6f;

    bool collected = false;
    bool attracting = false;
    Transform attractTarget;
    Transform cam;

    void OnEnable()  { Inventory.OnSummon += HandleSummon; }
    void OnDisable() { Inventory.OnSummon -= HandleSummon; }

    void Start() { cam = Camera.main.transform; }

    public void OnLookEnter() { }
    public void OnLookExit()  { }

    public void OnLookSelect()
    {
        if (collected) return;

        if (Vector3.Distance(transform.position, cam.position) <= pickupDistance)
            Collect();
    }

    void Collect()
    {
        collected = true;
        Inventory.Instance.Add(this);

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        transform.localScale *= 0.9f; // feedback visual simple
    }

    void HandleSummon(Transform target)
    {
        if (!collected) return;
        attractTarget = target;
        attracting = true;
    }

    void Update()
    {
        if (attracting && attractTarget != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, attractTarget.position, attractSpeed * Time.deltaTime
            );
        }
    }
}
