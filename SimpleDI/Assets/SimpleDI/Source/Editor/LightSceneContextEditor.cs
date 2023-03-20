using UnityEditor;
using UnityEngine;

namespace SimpleDI
{
    [CustomEditor(typeof(LightSceneContext))]
    public class LightSceneContextEditor : Editor
    {
        private LightSceneContext _lightSceneContext;
 
        private void OnEnable()
        {
            if (AssetDatabase.Contains(target))
            {
                _lightSceneContext = null;
            }
            else
            {
                _lightSceneContext = target as LightSceneContext;
            }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_lightSceneContext == null)
            {
                return;
            }

            if (GUILayout.Button("Search & Register in scene"))
            {
                _lightSceneContext.RegisterAllInjectInstancesInScene();
            }
        }
    }
}