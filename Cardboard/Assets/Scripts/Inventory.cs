using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public Transform player;

    private readonly List<Collectible> collected = new();

    public delegate void SummonOrder(Transform target);
    public static event SummonOrder OnSummon;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Add(Collectible c)
    {
        if (!collected.Contains(c)) collected.Add(c);
    }

    public void SummonToPlayer()
    {
        if (player != null) OnSummon?.Invoke(player);
    }
}
