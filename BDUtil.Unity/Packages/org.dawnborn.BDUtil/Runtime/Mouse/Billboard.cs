using UnityEngine;
using UnityEngine.Tilemaps;

namespace BDUtil
{
    /// An entity which orients relative to the (assumed orthorgraphic) camera initially (editor via onValidate, runtime onStart).
    /// If the camera angle isn't fixed, after modifying its angle you should call Billboard.AllFaceCamera.
    /// Special handling for tilemaps, since their internal structure needs (additional) geometry.
    // TODO: strip from build? Right now, it's purely editor-time; it could even be implemented on all spriterenderers...
    public class Billboard : MonoBehaviour
    {
        void OnStart() => FaceCamera(Camera.main);
        void OnValidate() => EditorUtils.Delay(this, () => FaceCamera(Camera.main));  // "SendMessage can't be called from OnValidate" grumble grumble.
        void OnReset() => EditorUtils.Delay(this, () => FaceCamera(Camera.main));  // "SendMessage can't be called from OnValidate" grumble grumble.
        public static void BillboardAll(Camera camera)
        { foreach (Billboard b in FindObjectsOfType<Billboard>()) b.FaceCamera(camera); }
        public void FaceCamera(Camera camera)
        {
            Grid grid = GetComponent<Grid>();
            if (grid == null)
            {
                // Not a grid, the default case.
                // Set our transform to any z rotation we already had + the camera's rotation.
                transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z);
                return;
            }
            foreach (Tilemap tilemap in GetComponentsInChildren<Tilemap>())
            {
                tilemap.orientation = Tilemap.Orientation.Custom;
                Matrix4x4 orientation = tilemap.orientationMatrix;
                Quaternion rotation = Quaternion.Inverse(tilemap.transform.rotation) * camera.transform.rotation;
                orientation.SetTRS(orientation.GetPosition(), rotation, orientation.lossyScale);
                tilemap.orientationMatrix = orientation;
            }
            return;
        }
    }
}
