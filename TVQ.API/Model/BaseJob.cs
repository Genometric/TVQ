namespace Genometric.TVQ.API.Model
{
    public enum State { Queued = 0, Running = 1, Completed = 2, Failed = 3 };

    public abstract class BaseJob
    {
        public int ID { set; get; }

        public State Status { set; get; }

        public string Message { set; get; }
    }
}
