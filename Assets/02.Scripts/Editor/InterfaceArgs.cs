using System;
using UnityEngine;
using Object = UnityEngine.Object;

public struct InterfaceArgs
{
    public readonly Type ObjectType;
    public readonly Type InterfaceType;

    public InterfaceArgs(Type objectType, Type interfaceType)
    {
        Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)} 는 {typeof(Object)} 의 타입이여야 합니다");
        Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} 는 인터페이스 타입이 아닙니다");

        ObjectType = objectType;
        InterfaceType = interfaceType;
    }
}
