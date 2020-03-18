﻿using System;
using System.Reflection;
using UnityEditor;

namespace SimpleDI
{
    [InitializeOnLoad]
    internal class InitializeExecutionOrder
    {
        static InitializeExecutionOrder()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isPlaying)
            {
                return;
            }

            foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                Type type = script.GetClass();
                // Check only classes in SimpleDI namespace.
                if (type == default || !type.Namespace.Contains("SimpleDI"))
                {
                    continue;
                }

                ExecutionOrderAttribute attribute = type.GetCustomAttribute<ExecutionOrderAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                int order = MonoImporter.GetExecutionOrder(script);
                if (!attribute.FixedOrder && order != 0)
                {
                    continue;
                }

                MonoImporter.SetExecutionOrder(script, attribute.Order);
            }
        }
    }
}