using UnityEngine;

public class CameraClamp : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Header("Map Bounds")]
    [SerializeField] public float _minX;
    [SerializeField] public float _maxX;
    [SerializeField] public float _minY;
    [SerializeField] public float _maxY;

    public Vector3 HandleClamp(Vector3 desiredPos)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampX = Mathf.Clamp(desiredPos.x, _minX +  camWidth, _maxX - camWidth);
        float clampY = Mathf.Clamp(desiredPos.y, _minY + camHeight, _maxY - camHeight);

        return new Vector3(clampX, clampY, desiredPos.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 center = new Vector3((_minX + _maxX) / 2f, (_minY + _maxY) / 2f, 0);
        Vector3 size = new Vector3(_maxX - _minX, _maxY - _minY, 0);

        Gizmos.DrawWireCube(center, size);
    }
}
