using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// InterfaceReference 직렬화할 때, Inspector 에서 사용하는 GUI 를 커스터마이징하기 위한 도우미 클래스
// 특정 인터페이스를 구현하는 에셋, 컴포넌트, 오브젝트를 Inspector 에서 드래그 앤 드랍하여 편하게 읽기/쓰기/확인이 가능
[CustomPropertyDrawer(typeof(InterfaceReference<>))]
[CustomPropertyDrawer(typeof(InterfaceReference<,>))]
public class InterfaceReferenceDrawer : PropertyDrawer
{
    const string FieldName = "referenceObj";

    // Unity editor 로부터 특정 프로퍼티를 전달받아서 Inspector 에 UI를 그리고 해당 UI를 통해 데이터 조작이 가능
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var referenceObjProperty = property.FindPropertyRelative(FieldName);    // 해당 필드의 프로퍼티를 찾음
        var args = GetArguments(fieldInfo); // 필드의 메타 데이터에서 어떤 타입을 사용하는지 파싱

        EditorGUI.BeginProperty(position, label, property);

        var assignedObject = EditorGUI.ObjectField(position, label, referenceObjProperty.objectReferenceValue, args.ObjectType, true);

        // 사용자가 필드에 오브젝트를 할당했다면
        if (assignedObject != null)
        {
            Object component = null;

            // 1. GameObject 라면
            if (assignedObject is GameObject gameObject)
                component = gameObject.GetComponent(args.InterfaceType);

            // 2. 해당 Object 가 필요로 하는 인터페이스를 구현한다면
            else if (args.InterfaceType.IsAssignableFrom(assignedObject.GetType()))
                component = assignedObject;

            // 유효한 결과를 찾았다면 해당 결과를 할당
            if (component != null)
                ValidateAndAssignObject(referenceObjProperty, component, component.name, args.InterfaceType.Name);

            else
            {
                Debug.LogWarning($"할당된 Object 가 필요한 인터페이스를 구현하지 않습니다: {args.InterfaceType.Name}");
                referenceObjProperty.objectReferenceValue = null;
            }
        }
        // 아니라면 실제 데이터를 null 로 설정
        else
            referenceObjProperty.objectReferenceValue = null;


        EditorGUI.EndProperty();
        InterfaceReferenceUtil.OnGUI(position, referenceObjProperty, label, args);  // GUI 업데이트
    }

    // 현재 필드가 어떤 인터페이스 타입과 오브젝트 타입을 필요로 하는지 파싱하는 함수
    static InterfaceArgs GetArguments(FieldInfo fieldInfo)
    {
        Type objectType = null, interfaceType = null;
        Type fieldType = fieldInfo.FieldType;

        // 단일 필드일 때, 해당 필드에 사용된 type 들을 알아내는 함수
        bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
        {
            objType = intfType = null;

            // 해당 타입이 제네릭 타입이 아니라면 중단
            if (type?.IsGenericType != true) 
                return false;

            // 원본 제네릭 타입을 알아냄
            var genericType = type.GetGenericTypeDefinition();

            // 1. 해당 타입이 단축형 InterfaceReference 타입이라면
            // 인터페이스와 오브젝트를 알아내기 위해서 부모 클래스를 사용
            if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

            // 2. 해당 타입이 부모 클래스라면 제네릭 인자를 뽑아내
            // 인터페이스와 오브젝트를 파싱
            if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
            {
                var types = type.GetGenericArguments();
                intfType = types[0];
                objType = types[1];
                return true;
            }

            return false;
        }

        // 리스트 필드일 때, 해당 필드에 사용된 type 들을 알아내는 함수
        void GetTypesFromList(Type type, out Type objType, out Type intfType)
        {
            objType = intfType = null;

            // IList 인터페이스를 구현하는 경우에만 처리
            var listInterface = type.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

            // IList 인 경우라면 제네릭 인자인 (= InterfaceReference<...>)을 추출
            if (listInterface != null)
            {
                var elementType = listInterface.GetGenericArguments()[0];
                TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
            }
        }

        // 필드가 InterfaceReference 인지 먼저 확인
        if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            // 아니라면 List 인지 확인
            GetTypesFromList(fieldType, out objectType, out interfaceType);

        // 파싱한 오브젝트, 인터페이스 타입을 반환
        return new InterfaceArgs(objectType, interfaceType);
    }

    // 찾은 오브젝트를 실제 데이터에 할당하는 함수
    static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentName, string interfaceName = null)
    {
        if (targetObject != null)
            property.objectReferenceValue = targetObject;

        else
        {
            Debug.LogWarning($"{componentName} 가 필요한 인터페이스를 구현하지 않습니다: {interfaceName}");
            property.objectReferenceValue = null;
        }
    }
}