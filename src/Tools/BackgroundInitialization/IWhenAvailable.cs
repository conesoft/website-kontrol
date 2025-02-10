namespace Conesoft_Website_Kontrol.Tools.BackgroundInitialization;

public interface IWhenAvailable<IT>
{
    void WhenAvailable<T>(T reference, Action<T> callback) where T : class;
    IT? Instance { get; }
}