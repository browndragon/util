using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BDUtil.Screen
{
    /// An entity which orients relative to the (assumed orthorgraphic) camera initially (editor via onValidate, runtime onStart).
    /// If the camera angle isn't fixed, after modifying its angle you should call Billboard.AllFaceCamera.
    /// Special handling for tilemaps, since their internal structure needs (additional) geometry.
    // TODO: strip from build? Right now, it's purely editor-time; it could even be implemented on all spriterenderers...
    [AddComponentMenu("BDUtil/Billboard")]
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Modify sprite orientation before billboarding (or else each tile's orientation)")]
        public Vector3 PreAdjust;
        [Tooltip("Modify sprite positioning *after* billboarding (you shouldn't use me for tiles!)")]
        public Vector3 PostAdjust = new(0f, 0f, .5f);
        void OnStart() => FaceCamera(Camera.main);
        void OnValidate() => EditorUtils.Delay(this, () => FaceCamera(Camera.main));  // "SendMessage can't be called from OnValidate" grumble grumble.
        void OnReset() => EditorUtils.Delay(this, () => FaceCamera(Camera.main));  // "SendMessage can't be called from OnValidate" grumble grumble.
        public static void BillboardAll(Camera camera)
        { foreach (Billboard b in FindObjectsOfType<Billboard>()) b.FaceCamera(camera); }
        [ContextMenu("BDUtil/Face Camera")]
        public void FaceCamera() => FaceCamera(Camera.main);
        public void FaceCamera(Camera camera)
        {
            if (camera == null) return;
            Quaternion rotate = camera.transform.rotation * Quaternion.Euler(PreAdjust.x, PreAdjust.y, PreAdjust.z);
            Grid grid = GetComponent<Grid>();
            if (grid == null)
            {
                // Not a grid, the default case.
                // Set our transform to any z rotation we already had + the camera's rotation.
                transform.rotation = rotate;
                if (transform.parent != null) transform.localPosition = PostAdjust;
                return;
            }
            foreach (Tilemap tilemap in GetComponentsInChildren<Tilemap>())
            {
                tilemap.orientation = Tilemap.Orientation.Custom;
                Matrix4x4 orientation = tilemap.orientationMatrix;
                /// Note that the tilemap transform SHOULD be 0: the whole idea is that it will mirror "real" positions.
                Quaternion tilemapRotation = Quaternion.Inverse(tilemap.transform.rotation) * rotate;
                orientation.SetTRS(orientation.GetPosition(), tilemapRotation, orientation.lossyScale);
                tilemap.orientationMatrix = orientation;
            }
            return;
        }
    }
}
