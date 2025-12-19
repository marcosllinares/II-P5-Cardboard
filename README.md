Autor: Marcos Llinares Montes
alu0100972443

---

A partir de la escena de ejemplo incluida en el complemento oficial de Google Cardboard XR para Unity, se desarrolló una versión personalizada orientada a Android con el objetivo de implementar una experiencia de realidad virtual donde el usuario interactúa principalmente mediante la mirada: recolectando objetos del entorno y, posteriormente, recuperándolos mediante un elemento especial.

En primer lugar, se partió de la escena base del tutorial y se trabajó sobre una copia para conservar intacta la configuración original. Se mantuvo el sistema de retícula propio de Cardboard, encargado de indicar visualmente el punto de enfoque del usuario y de detectar colisiones con objetos interactuables.

A continuación, se implementó un sistema de interacción por “fijación de mirada” (gaze dwell) mediante los scripts GazeInteractor e ILookSelectable, que permiten activar una selección cuando el usuario mantiene la vista sobre un objeto durante un tiempo determinado. Para garantizar la compatibilidad con el retículo de Cardboard y evitar avisos en consola, se incorporó un componente de apoyo (CardboardPointerRelay) en los objetos con collider, enlazando los eventos OnPointerEnter/Exit/Click con la lógica de selección por mirada.

Seguidamente, se creó una mecánica de recolección con el script Collectible, aplicada a un conjunto de objetos (por ejemplo, monedas) distribuidos sobre un terreno junto con elementos importados desde la Asset Store. Estos objetos se configuraron con la capa Interactable y colliders para que puedan ser detectados correctamente por el raycast de la retícula. Al ser seleccionados dentro de la distancia configurada, quedan registrados en un inventario gestionado por Inventory.

Por último, se añadió un objeto “recuperador” (imán) controlado por Magnet. Al seleccionarlo con la mirada, se emite un evento global que hace que todos los recolectables previamente registrados se desplacen hacia el jugador (cámara), simulando un efecto de atracción. Con todo ello, la escena queda preparada para su compilación y exportación como APK en Android cumpliendo los requisitos de la práctica.

---

![](https://imgur.com/W1C7J0C.gif)

Scripts

## ILookSelectable.cs

```csharp
public interface ILookSelectable
{
    void OnLookEnter();
    void OnLookExit();
    void OnLookSelect();
}
```

## GazeInteractor.cs

```csharp
using UnityEngine;
using UnityEngine.UI;

public class GazeInteractor : MonoBehaviour
{
    public float maxDistance = 20f;
    public LayerMask interactableMask;
    public float dwellTime = 1.2f;

    // Opcional: UI de progreso (si tienes una imagen radial o similar)
    public Image reticleFill;

    private float timer = 0f;
    private ILookSelectable current;
    private Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
        if (reticleFill) reticleFill.fillAmount = 0f;
    }

    void Update()
    {
        Ray ray = new Ray(cam.position, cam.forward);

        bool hitSomething = Physics.Raycast(
            ray, out RaycastHit hit, maxDistance,
            interactableMask.value == 0 ? ~0 : interactableMask
        );

        if (hitSomething)
        {
            var selectable = hit.collider.GetComponentInParent<ILookSelectable>();
            if (selectable != null)
            {
                if (current != selectable)
                {
                    current?.OnLookExit();
                    current = selectable;
                    current.OnLookEnter();
                    timer = 0f;
                    if (reticleFill) reticleFill.fillAmount = 0f;
                }
                else
                {
                    timer += Time.deltaTime;
                    if (reticleFill) reticleFill.fillAmount = Mathf.Clamp01(timer / dwellTime);

                    if (timer >= dwellTime)
                    {
                        current.OnLookSelect();
                        timer = 0f;
                        if (reticleFill) reticleFill.fillAmount = 0f;
                    }
                }
                return;
            }
        }

        ClearCurrent();
    }

    void ClearCurrent()
    {
        if (current != null)
        {
            current.OnLookExit();
            current = null;
        }
        timer = 0f;
        if (reticleFill) reticleFill.fillAmount = 0f;
    }
}
```

## Inventory.cs

```csharp
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
```

## Collectible.cs

```csharp
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
```

## Magnet.cs

```csharp
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
```

## EditorMouseLook.cs

```csharp
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class EditorMouseLook : MonoBehaviour
{
    public float sensitivity = 0.15f;

    float yaw;
    float pitch;

    void Start()
    {
#if UNITY_EDITOR
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Mouse.current == null) return;

        // Mantén botón derecho para mirar
        if (!Mouse.current.rightButton.isPressed) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * sensitivity;
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
#endif
    }
}
```

## CardboardPointerRelay.cs

```csharp
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
```
