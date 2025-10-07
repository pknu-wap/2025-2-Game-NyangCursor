using UnityEngine;
using System.Collections.Generic;

// LayerMask 연산을 지원하는 확장 메서드
public static class LayerMaskHelper
{
    // 특정 레이어가 LayerMask에 포함되어 있는지 확인하는 함수
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    // GameObject의 레이어가 LayerMask에 포함되어 있는지 확인하는 함수
    public static bool Contains(this LayerMask layerMask, GameObject gameObject)
    {
        return layerMask.Contains(gameObject.layer);
    }

    // LayerMask에 특정 레이어 추가하는 함수
    public static LayerMask Add(this LayerMask layerMask, int layer)
    {
        return layerMask | (1 << layer);
    }

    // LayerMask에서 특정 레이어 제거하는 함수
    public static LayerMask Remove(this LayerMask layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }

    // LayerMask에서 특정 레이어를 토글
    public static LayerMask Toggle(this LayerMask layerMask, int layer)
    {
        return layerMask ^ (1 << layer);
    }

    // 레이어 이름으로 LayerMask 를 생성하는 함수
    public static LayerMask CreateFromNames(params string[] layerNames)
    {
        LayerMask mask = 0;
        foreach (string layerName in layerNames)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
                mask = mask.Add(layer);

            else
                Debug.LogWarning($"레이어 '{layerName}'를 찾을 수 없습니다.");
        }
        return mask;
    }

    // LayerMask에 포함된 모든 레이어 번호를 배열로 반환하는 함수
    public static int[] ToLayerArray(this LayerMask layerMask)
    {
        List<int> layers = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if (layerMask.Contains(i))
                layers.Add(i);
        }

        return layers.ToArray();
    }

    // LayerMask에 포함된 레이어들의 이름을 배열로 반환하는 함수
    public static string[] ToLayerNames(this LayerMask layerMask)
    {
        int[] layers = layerMask.ToLayerArray();
        string[] names = new string[layers.Length];

        for (int i = 0; i < layers.Length; i++)
            names[i] = LayerMask.LayerToName(layers[i]);

        return names;
    }
}