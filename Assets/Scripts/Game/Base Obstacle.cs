using UnityEngine;

public abstract class BaseObstacle : MonoBehaviour
{
    protected abstract float Speed { get; }
    protected abstract bool CanMove { get; }

    void Update()
    {
        if (CanMove)
            transform.Translate(Vector3.left * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DestroyPoint"))
            gameObject.SetActive(false);
    }
}