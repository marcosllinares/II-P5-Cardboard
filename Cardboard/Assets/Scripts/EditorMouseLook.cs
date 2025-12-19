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
