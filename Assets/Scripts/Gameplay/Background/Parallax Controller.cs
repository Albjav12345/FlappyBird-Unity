using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public static ParallaxController Instance { get; private set; }

    [Header("Main Speed Control")]
    [Tooltip("La velocidad del suelo. El fondo se moverá automáticamente un 75% más lento.")]
    [SerializeField] private float groundSpeed = 2f;

    private float bgSpeed => groundSpeed * 0.25f;      // ✅ 75% más lento
    public bool parallax = true;

    [Header("Transforms")]
    [SerializeField] private Transform dayBgA;
    [SerializeField] private Transform dayBgB;
    [SerializeField] private Transform nightBgA;
    [SerializeField] private Transform nightBgB;
    [SerializeField] private Transform groundA;
    [SerializeField] private Transform groundB;

    [Header("Colliders")]
    [SerializeField] private BoxCollider2D backgroundCollider;
    [SerializeField] private BoxCollider2D groundCollider;

    private float backgroundWidth;
    private float groundWidth;

    private void Awake()
    {
        // ✅ Singleton correcto
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        backgroundWidth = backgroundCollider.size.x;
        groundWidth = groundCollider.size.x;
    }

    void FixedUpdate()
    {
        if (!parallax)
            return;

        MoveBackground(dayBgA);
        MoveBackground(dayBgB);
        MoveBackground(nightBgA);
        MoveBackground(nightBgB);

        MoveGround(groundA);
        MoveGround(groundB);

        ResetBackgroundLoop(dayBgA);
        ResetBackgroundLoop(dayBgB);
        ResetBackgroundLoop(nightBgA);
        ResetBackgroundLoop(nightBgB);

        ResetGroundLoop(groundA);
        ResetGroundLoop(groundB);
    }

    private void MoveBackground(Transform obj)
    {
        obj.Translate(Vector3.left * bgSpeed * Time.deltaTime);
    }

    private void MoveGround(Transform obj)
    {
        obj.Translate(Vector3.left * groundSpeed * Time.deltaTime);
    }

    private void ResetBackgroundLoop(Transform obj)
    {
        if (obj.position.x < -backgroundWidth)
            obj.Translate(Vector3.right * backgroundWidth * 2f);
    }

    private void ResetGroundLoop(Transform obj)
    {
        if (obj.position.x < -groundWidth)
            obj.Translate(Vector3.right * groundWidth * 2f);
    }

    public void SetParallaxSpeed(float newGroundSpeed)
    {
        groundSpeed = newGroundSpeed;
    }

    public float GetGroundSpeed() => groundSpeed;
    public float GetBackgroundSpeed() => bgSpeed;
}
