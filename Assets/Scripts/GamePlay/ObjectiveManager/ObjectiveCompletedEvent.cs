public class ObjectiveCompletedEvent
{
    public string ObjectiveName { get; private set; }

    public ObjectiveCompletedEvent(string objectiveName)
    {
        ObjectiveName = objectiveName;
    }
}
