namespace CoreNet.Events
{
    public delegate void EventHandler<T>(T evt) where T : IEvent;
}