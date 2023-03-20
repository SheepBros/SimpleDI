using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleDI
{
    [InitializeOnLoad]
    public class LightSceneContextAutoRegister
    {
        private const bool AutoRegister = true;

        static LightSceneContextAutoRegister()
        {
            if (!AutoRegister)
            {
                return;
            }
            
            if (EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static void OnSceneSaved(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                LightSceneContext lightSceneContext = rootObject.GetComponentInChildren<LightSceneContext>();
                if (lightSceneContext != null)
                {
                    lightSceneContext.RegisterAllInjectInstancesInScene();
                }
            }
        }
    }
}