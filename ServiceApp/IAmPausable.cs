namespace ServiceApp
{
    public interface IAmPausable
    {
        void Pause();
        void Unpause();
        Status CurrentStatus {get;}
    }

    public enum Status
    {
        Stopped,
        Started,
        Paused
    }
}