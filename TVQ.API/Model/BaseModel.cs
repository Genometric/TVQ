namespace Genometric.TVQ.API.Model
{
    public enum State { Ready=0, Scheduled=1, Updating=2 };

    public abstract class BaseModel
    {
        public State Status { set; get; }
    }
}
