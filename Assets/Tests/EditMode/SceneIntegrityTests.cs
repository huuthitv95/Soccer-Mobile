using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SoccerMobilePro.Tests
{
    public sealed class SceneIntegrityTests
    {
        private static readonly HashSet<string> BuiltInGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "00000000000000000000000000000000",
            "0000000000000000e000000000000000",
            "0000000000000000f000000000000000"
        };

        [Test]
        public void AllProjectScenesHaveValidReferences()
        {
            var sceneSetup = EditorSceneManager.GetSceneManagerSetup();
            var errors = new List<string>();
            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.That(scenePaths.Length, Is.EqualTo(14), "Danh s?ch Scene ?? thay ??i; h?y c?p nh?t audit v? k? v?ng test.");

            try
            {
                foreach (var scenePath in scenePaths)
                {
                    ValidateYamlGuids(scenePath, errors);
                    ValidateLoadedScene(scenePath, errors);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
            }

            Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
        }

        private static void ValidateYamlGuids(string scenePath, ICollection<string> errors)
        {
            var lines = File.ReadAllLines(scenePath);
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                foreach (Match match in Regex.Matches(lines[lineIndex], "guid: ([0-9a-f]{32})", RegexOptions.IgnoreCase))
                {
                    var guid = match.Groups[1].Value;
                    if (BuiltInGuids.Contains(guid) || !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        continue;
                    }

                    errors.Add($"{scenePath}:{lineIndex + 1} Missing GUID {guid}");
                }
            }
        }

        private static void ValidateLoadedScene(string scenePath, ICollection<string> errors)
        {
            try
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        ValidateGameObject(scenePath, transform.gameObject, errors);
                    }
                }
            }
            catch (Exception exception)
            {
                errors.Add($"{scenePath} Load exception: {exception.GetType().Name}: {exception.Message}");
            }
        }

        private static void ValidateGameObject(string scenePath, GameObject gameObject, ICollection<string> errors)
        {
            var components = gameObject.GetComponents<Component>();
            for (var componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                var component = components[componentIndex];
                if (component == null)
                {
                    errors.Add($"{scenePath} Missing Script: {GetHierarchyPath(gameObject.transform)} componentIndex={componentIndex}");
                    continue;
                }

                try
                {
                    var serializedObject = new SerializedObject(component);
                    var property = serializedObject.GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference ||
                            property.objectReferenceValue != null ||
                            property.objectReferenceInstanceIDValue == 0)
                        {
                            continue;
                        }

                        errors.Add($"{scenePath} Broken Reference: {GetHierarchyPath(gameObject.transform)} " +
                                   $"{component.GetType().FullName}.{property.propertyPath}");
                    }
                }
                catch (Exception exception)
                {
                    errors.Add($"{scenePath} Inspection exception: {GetHierarchyPath(gameObject.transform)} " +
                               $"{component.GetType().FullName}: {exception.GetType().Name}: {exception.Message}");
                }
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new List<string>();
            while (transform != null)
            {
                names.Add(transform.name);
                transform = transform.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}