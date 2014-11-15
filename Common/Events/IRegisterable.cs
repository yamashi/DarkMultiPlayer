namespace CoreNet.Events
{
    public interface IRegisterable
    {
        void Register(IEventAggregator aggregator);
    }
}