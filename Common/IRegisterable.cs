namespace DarkMultiPlayerCommon.Events
{
    public interface IRegisterable
    {
        void Register(IEventAggregator aggregator);
    }
}