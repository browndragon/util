using UnityEngine;

namespace BDUtil
{
    public static class EditorUtils
    {
        public static T InstantiateWithLink<T>(T go) where T : Object =>
#if UNITY_EDITOR
            (T)UnityEditor.PrefabUtility.InstantiatePrefab(go)
#else  // !UNITY_EDITOR
            UnityEngine.Object.Instantiate(go)
#endif  // UNITY_EDITOR
        ;
        public static GameObject CloneInactive(GameObject gameObject)
        {
            bool wasActive = gameObject.activeSelf;
            try
            {
                gameObject.SetActive(false);
                return InstantiateWithLink(gameObject);
            }
            finally
            {
                gameObject.SetActive(wasActive);
            }
        }
        public static void DestroyChildrenByPlaystate(Transform transform)
        {
            if (!Application.isPlaying) for (int i = transform.childCount - 1; i >= 0; --i) Object.DestroyImmediate(transform.GetChild(i).gameObject);
            else for (int i = transform.childCount - 1; i >= 0; --i) Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}