#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraClamp))]
public class CameraClampEditor : Editor
{
    private void OnSceneGUI()
    {
        CameraClamp clamp = (CameraClamp)target;

        Vector3 center = new Vector3((clamp._minX + clamp._maxX) / 2f, (clamp._minY + clamp._maxY) / 2f, 0);
        Vector3 size = new Vector3(clamp._maxX - clamp._minX, clamp._maxY - clamp._minY, 0);

        Handles.color = Color.red;
        Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);

        size = Handles.ScaleHandle(size, newCenter, Quaternion.identity, HandleUtility.GetHandleSize(newCenter));

        clamp._minX = newCenter.x - size.x / 2f;
        clamp._maxX = newCenter.x + size.x / 2f;
        clamp._minY = newCenter.y - size.y / 2f;
        clamp._maxY = newCenter.y + size.y / 2f;
    }
}
#endif