using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public float speed = 2f; // Kecepatan awan
    public float resetPositionX = -10f; // Posisi saat awan di-reset ke awal
    public float startPositionX = 10f; // Posisi awal awan saat reset

    void Update()
    {
        // Gerakkan awan ke kiri tanpa mengubah posisi Z
        transform.position += new Vector3(-speed * Time.deltaTime, 0f, 0f);

        // Jika awan sudah melewati batas reset, pindahkan ke posisi awal tanpa mengubah Z
        if (transform.position.x <= resetPositionX)
        {
            transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
        }
    }
}
