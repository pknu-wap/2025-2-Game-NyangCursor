using System;
using UnityEngine;
using Object = UnityEngine.Object;

// Insepctor 에서 인터페이스를 구현한 컴포넌트나 에셋을 직접 할당을 가능하게 해주는 참조 클래스 
[Serializable, HideInInspector]
public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
{
    [SerializeField] TObject referenceObj;

    // 인터페이스에 정의된 메서드를 사용하기 위한 프로퍼티
    // referenceObj 가 해당 인터페이스를 구현하지 않았을 경우, 에러 반환
    public TInterface TargetInterface
    {
        get => referenceObj switch
        {
            null => null,
            TInterface @interface => @interface,
            _ => throw new InvalidOperationException($"{referenceObj} 는 해당 인터페이스를 구현해야 합니다: {nameof(TInterface)}")
        };
        set => referenceObj = value switch
        {
            null => null,
            TObject newValue => newValue,
            _ => throw new ArgumentException($"매개 변수는 {typeof(TObject)} 의 타입이여야 합니다", nameof(value))
        };
    }

    // 인터페이스 기능 외의 Unitu Object 자체에 접근하기 위한 프로퍼티 (setActive, Transform 등이 필요할 때)
    public TObject TargetObject
    {
        get => referenceObj;
        set => referenceObj = value;
    }
    

    // 1. 기본 생성자
    public InterfaceReference() { }
    // 2. Unity Object 를 전달받아 생성
    public InterfaceReference(TObject target) => referenceObj = target;
    // 3. 인터페이스를 구현한 클래스의 객체를 전달받아 생성
    public InterfaceReference(TInterface @interface) => referenceObj = @interface as TObject;

    // 인터페이스 타입의 변수에 InterfaceReference 객체를 할당하려할 때, 자동 타입 변환을 지원
    public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.TargetInterface;
}

// 현재 클래스를 InterfaceReference<TInterface> 클래스로 편리하게 줄여서 사용할 수 있게 하는 클래스 
[Serializable]
public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class { }