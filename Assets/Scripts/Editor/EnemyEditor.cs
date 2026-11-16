using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor
{
    private void OnSceneGUI()
    {
        Enemy enemy = (Enemy)target;
        if (enemy == null) return;

        // In edit mode the limits follow the enemy's transform; in play mode they remain at start position
        Vector3 origin = enemy.transform.position;
        float y = origin.y;
        float z = origin.z;

        Vector3 leftLimitPos = new Vector3(origin.x - Mathf.Abs(enemy.LeftDistance), y, z);
        Vector3 rightLimitPos = new Vector3(origin.x + Mathf.Abs(enemy.RightDistance), y, z);

        // Scene view handles to change patrol limits
        if (!Application.isPlaying)
        {
            // Left Limit slider handle
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.red;
            Vector3 newLeft = Handles.Slider(leftLimitPos, Vector3.right, HandleUtility.GetHandleSize(leftLimitPos) * 0.15f, Handles.SphereHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(enemy, "Change Enemy Left Patrol Limit");
                float dist = origin.x - newLeft.x;
                enemy.LeftDistance = Mathf.Max(0f, dist);
                EditorUtility.SetDirty(enemy);
            }

            // Right Limit slider handle
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.green;
            Vector3 newRight = Handles.Slider(rightLimitPos, Vector3.right, HandleUtility.GetHandleSize(rightLimitPos) * 0.15f, Handles.SphereHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(enemy, "Change Enemy Right Patrol Limit");
                float dist = newRight.x - origin.x;
                enemy.RightDistance = Mathf.Max(0f, dist);
                EditorUtility.SetDirty(enemy);
            }

            // Display distance labels in Scene view
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 12;
            labelStyle.fontStyle = FontStyle.Bold;

            Handles.Label(leftLimitPos + Vector3.up * 0.25f, $"Left: {enemy.LeftDistance:F1}", labelStyle);
            Handles.Label(rightLimitPos + Vector3.up * 0.25f, $"Right: {enemy.RightDistance:F1}", labelStyle);
        }
    }
}

