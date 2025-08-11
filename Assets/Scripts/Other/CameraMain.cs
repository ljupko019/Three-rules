using UnityEngine;

public class CameraMain : MonoBehaviour
{
    [SerializeField] private GameObject player;

    // Definiši granice mape kao world coordinates
    [SerializeField] private Vector2 mapMin = new Vector2(-5, -4);
    [SerializeField] private Vector2 mapMax = new Vector2(20, 10);

    private float halfCameraWidth;
    private float halfCameraHeight;

    void Start()
    {
        Camera cam = Camera.main;
        halfCameraHeight = cam.orthographicSize;
        halfCameraWidth = halfCameraHeight * cam.aspect;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Minimum i maksimum pozicije kamere uzimajuci u obzir velicinu kamere i mapu
        float minX = mapMin.x + halfCameraWidth;
        float maxX = mapMax.x - halfCameraWidth;

        float minY = mapMin.y + halfCameraHeight;
        float maxY = mapMax.y - halfCameraHeight;

        // Ako je mapa manja od vidnog polja kamere centriraj kameru u sredinu mape
        if (maxX < minX)
        {
            minX = maxX = (mapMin.x + mapMax.x) / 2f;
        }
        if (maxY < minY)
        {
            minY = maxY = (mapMin.y + mapMax.y) / 2f;
        }

        float camX = Mathf.Clamp(player.transform.position.x, minX, maxX);
        float camY = Mathf.Clamp(player.transform.position.y, minY, maxY);

        transform.position = new Vector3(camX, camY, transform.position.z);
    }
}
