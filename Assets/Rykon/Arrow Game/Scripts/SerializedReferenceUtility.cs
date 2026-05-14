#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ArrowGame
{
    internal static class SerializedReferenceUtility
    {
        public static void Assign(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        public static void AssignArray(SerializedObject serializedObject, string propertyName, Object[] values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return;

            property.arraySize = values != null ? values.Length : 0;
            for (int i = 0; i < property.arraySize; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
