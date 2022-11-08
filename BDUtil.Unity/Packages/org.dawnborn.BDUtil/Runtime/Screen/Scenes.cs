using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BDUtil.Screen
{
    /// For things like UnityEvent callbacks, etc.
    public class Scenes : StaticAsset<Scenes>
    {
        public void ReloadActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scene);
            asyncOperation.completed += ao => AddScene(scene.name);
        }
        public void LoadScene(string name) => SceneManager.LoadScene(name);
        public void AddScene(string name) => SceneManager.LoadScene(name, LoadSceneMode.Additive);
        public int Unloading = 0;
        public void UnloadScene(string name)
        {
            Unloading++;
            SceneManager.UnloadSceneAsync(name).completed += CompletedUnload;
        }
        void CompletedUnload(AsyncOperation completed) { if (completed.isDone) Unloading--; }
    }
}