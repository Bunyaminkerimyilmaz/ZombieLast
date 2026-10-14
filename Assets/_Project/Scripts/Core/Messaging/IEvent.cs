namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="EventBus{T}"/> ile gönderilen olayların işaret arayüzü.
    /// Olaylar <c>readonly struct</c> olarak tanımlanır ve alanına göre <c>Core/Events/XxxEvents.cs</c> dosyalarında durur.
    /// </summary>
    public interface IEvent
    {
    }
}
