public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            instance ??= new();
            return instance;
        }
    }

    protected Singleton() { }
}