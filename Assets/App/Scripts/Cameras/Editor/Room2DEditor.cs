using UnityEditor;
using UnityEngine;

namespace App.Cameras.Editor
{
    /// <summary>
    /// Room2D コンポーネント Editor 拡張
    /// - SceneView 内でハンドル型 Gizmo を表示して調整可能にする
    /// - Undo /Redo 対応
    /// </summary>
    [CustomEditor(typeof(Room2D))]
    public class Room2DEditor : UnityEditor.Editor
    {
        SerializedProperty _boundsProperty;

        void OnEnable()
        {
            _boundsProperty = serializedObject.FindProperty("_bounds");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI() を呼ばないことでインスペクタ上に表示する項目を置き換える

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            var newRect = EditorGUILayout.RectField("Bounds", _boundsProperty.rectValue);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Bounds");
                _boundsProperty.rectValue = newRect;
            }

            // _bounds 以外のプロパティを表示する
            DrawPropertiesExcluding(serializedObject, "_bounds", "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            var room = (Room2D)target;
            var bounds = room.Bounds;
            var corners = room.GetCorners();

            for (var i = 0; i < corners.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                var snap = EditorSnapSettings.move;
                var handleSize = HandleUtility.GetHandleSize(corners[i]) * 0.1f;
                var newCorner = Handles.FreeMoveHandle(corners[i], handleSize, snap, Handles.CubeHandleCap);

                // スナップ処理
                newCorner.x = Mathf.Round(newCorner.x / snap.x) * snap.x;
                newCorner.y = Mathf.Round(newCorner.y / snap.y) * snap.y;
                newCorner.z = Mathf.Round(newCorner.z / snap.z) * snap.z;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(room, "Change Bounds");

                    switch(i)
                    {
                        case 0: // bottom left
                            bounds.xMin = newCorner.x;
                            bounds.yMin = newCorner.y;
                            break;
                        case 1: // bottom right
                            bounds.xMax = newCorner.x;
                            bounds.yMin = newCorner.y;
                            break;
                        case 2: // top right
                            bounds.xMax = newCorner.x;
                            bounds.yMax = newCorner.y;
                            break;
                        case 3: // top left
                            bounds.xMin = newCorner.x;
                            bounds.yMax = newCorner.y;
                            break;
                    }

                    room.Bounds = bounds;
                }
            }
        }
    }
}
