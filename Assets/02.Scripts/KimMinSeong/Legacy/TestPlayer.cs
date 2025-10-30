using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TestPlayer : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private InterfaceReference<IDamageable> damageable;
    [SerializeField] private InterfaceReference<ICollidable> collidable;

    public IDamageable Damageable => damageable?.TargetInterface;
    public ICollidable Collidable => collidable?.TargetInterface;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageable?.TargetInterface?.Initialize(this);
        collidable?.TargetInterface?.Initialize(this);
    }

    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
    }

    private void FixedUpdate()
    {
        Vector2 movement = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}