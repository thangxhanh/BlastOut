namespace Dacodelaac.Events
{
    public interface IEventListener
    {
        void OnEventRaised();
    }
    
    public interface IEventListener<in TType>
    {
        void OnEventRaised(TType value);
    }
}