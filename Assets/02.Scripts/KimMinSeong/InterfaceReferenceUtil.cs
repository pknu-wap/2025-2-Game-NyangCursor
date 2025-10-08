using UnityEditor;
using UnityEngine;

// Inspector 에서 GUI 에 필요한 디자인을 담당하는 클래스
public class InterfaceReferenceUtil
{
    static GUIStyle labelStyle;

    public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, InterfaceArgs args)
    {
        InitializeStyleIfNeeded();

        // GUI 시스템에서 사용하는 식별자인 ID 를 생성
        var controlID = GUIUtility.GetControlID(FocusType.Passive) - 1;
        // 필드가 비어있으면 인터페이스 이름을 표시
        var displayString = property.objectReferenceValue == null
            ? $"({args.InterfaceType.Name})" 
            : "";
        DrawInterfaceNameLabel(position, displayString, controlID);
    }

    static void DrawInterfaceNameLabel(Rect position, string displayString, int controlID)
    {
        if (Event.current.type == EventType.Repaint)    // 최적화 코드로써 특정 이벤트일 때만 동작하도록 설정
        {
            const int additionalLeftWidth = 3;
            const int verticalIndent = 1;

            var content = EditorGUIUtility.TrTextContent(displayString);
            var size = labelStyle.CalcSize(content);
            var labelPos = position;

            labelPos.width = size.x + additionalLeftWidth;
            labelPos.x += position.width - labelPos.width - 18;
            labelPos.height -= verticalIndent * 2;
            labelPos.y += verticalIndent;
            labelStyle.Draw(labelPos, EditorGUIUtility.TrTextContent(displayString), controlID, DragAndDrop.activeControlID == controlID, false);
        }
    }

    // GUI 에 사용하는 스타일을 초기화하는 함수
    static void InitializeStyleIfNeeded()
    {
        if (labelStyle != null) 
            return;

        var style = new GUIStyle(EditorStyles.label)
        {
            font = EditorStyles.objectField.font,
            fontSize = EditorStyles.objectField.fontSize,
            fontStyle = EditorStyles.objectField.fontStyle,
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(0, 2, 0, 0)
        };
        labelStyle = style;
    }
}