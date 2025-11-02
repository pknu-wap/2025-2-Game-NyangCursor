using System;
using UnityEngine;

public class ExpAltar : AltarBase
{
    [Header("설정")]
    [SerializeField] private float pullSpeed = 10f;
    public static event Action<Transform,float> expAltarEvent;

    public override void Execute(Transform player)
    {
        expAltarEvent.Invoke(player,pullSpeed);
    }
}
