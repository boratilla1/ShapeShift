using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 100f; // Döndürme hýzý

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}