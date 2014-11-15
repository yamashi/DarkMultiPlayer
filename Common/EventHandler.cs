namespace DarkMultiPlayerCommon.Events
{
    public delegate void EventHandler<T>(T evt) where T : IEvent;
}