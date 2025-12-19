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
