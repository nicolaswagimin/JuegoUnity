using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float forwardSpeed = 6f;
    public float horizontalSpeed = 3f;

    [Header("Límites de movimiento horizontal")]
    public float rightLimit = 5.5f;
    public float leftLimit = -5.5f;

    void Update()
    {
        // Movimiento hacia adelante constante
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);

        float horizontalInput = 0f;

        // Entrada lateral
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
        }

        // Nueva posición lateral
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * horizontalSpeed * Time.deltaTime;

        // Aplicar límites
        newPosition.x = Mathf.Clamp(newPosition.x, leftLimit, rightLimit);

        // Mover jugador
        transform.position = newPosition;
    }
}
