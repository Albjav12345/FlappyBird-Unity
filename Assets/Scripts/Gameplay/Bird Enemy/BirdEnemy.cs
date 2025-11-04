using UnityEngine;
using System.Collections;
using CarterGames.Assets.AudioManager;

public class BirdEnemy : BaseObstacle
{
    protected override float Speed => BirdEnemiesController.Instance.speed;
    protected override bool CanMove => !Player.Instance.IsDead;

    private bool isHit = false;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isHit = false;
        spriteRenderer.enabled = true;
    }

    public void OnHitByPlayer()
    {
        if (isHit) return;
        isHit = true;

        if (hitParticles != null)
            hitParticles.Play();

        AudioManager.Play("an_ominous-crow-call-255173 (mp3cut.net)-5a873b49-abef-4f91-a854-faddc9143f5b");
        
        StartCoroutine(HideSpriteAfterDelay(0));
    }

    private IEnumerator HideSpriteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        spriteRenderer.enabled = false;
    }

    // ✅ Nueva función usada por el controller para reiniciar solo el aspecto visual
    public void ResetBirdVisuals()
    {
        isHit = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (hitParticles != null)
            hitParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}