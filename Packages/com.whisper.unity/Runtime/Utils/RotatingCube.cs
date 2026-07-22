using UnityEngine;

namespace Whisper.Utils
{
    /// <summary>
    /// Simple rotation script to check if Unity didn't hang
    /// </summary>
    public class RotatingCube : MonoBehaviour
    {
        public float speed = 10f;
        public bool isRotating = true; // Dönüþ durumunu kontrol eden bayrak

        private void Update()
        {
            if (isRotating)
            {
                transform.Rotate(Vector3.one * (Time.deltaTime * speed));
            }
        }

        // Dýþarýdan veya Event üzerinden çaðrýlacak metodlar
        public void StartRotation()
        {
            isRotating = true;
        }

        public void StopRotation()
        {
            isRotating = false;
        }
    }
}