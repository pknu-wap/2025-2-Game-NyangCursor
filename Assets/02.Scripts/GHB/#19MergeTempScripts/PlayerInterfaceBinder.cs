using UnityEngine;

public class PlayerInterfaceBinder : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private InterfaceReference<IDamageable> damageable;
    [SerializeField] private InterfaceReference<ICollidable> collidable;

    public IDamageable Damageable => damageable?.TargetInterface;
    public ICollidable Collidable => collidable?.TargetInterface;

    private void Awake()
    {
        damageable?.TargetInterface?.Initialize(this);
        collidable?.TargetInterface?.Initialize(this);
    }
}
