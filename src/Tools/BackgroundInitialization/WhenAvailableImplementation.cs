namespace Conesoft_Website_Kontrol.Tools.BackgroundInitialization;

public class WhenAvailableImplementation<IT> : IWhenAvailable<IT>
{
    record Callback(WeakReference Reference, Action<object> Action);

    private readonly Queue<Callback> queue = [];
    private IT? instance;

    public void Set(IT instance)
    {
        this.instance = instance;
        foreach (var item in queue)
        {
            if (item.Reference.Target is object reference)
            {
                item.Action(reference);
            }
        }
        queue.Clear();
    }

    public IT? Instance => instance;

    public void WhenAvailable<T>(T reference, Action<T> callback) where T : class
    {
        if (instance != null)
        {
            callback(reference);
        }
        else
        {
            queue.Enqueue(new(new WeakReference(reference), obj => callback((T)obj)));
        }
    }
}
