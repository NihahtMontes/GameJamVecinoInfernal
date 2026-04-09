using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // arrastra el Player aquí en el Inspector
    public float smoothSpeed = 5f;  // qué tan fluido sigue
    public Vector3 offset = new Vector3(0, 0, -10); // Z debe ser -10 siempre en 2D

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}