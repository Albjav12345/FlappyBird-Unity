public class Pipes : BaseObstacle
{
    protected override float Speed => PipesController.Instance.speed;

    protected override bool CanMove => !Player.Instance.IsDead;
}