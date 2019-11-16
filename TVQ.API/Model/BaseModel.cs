namespace Genometric.TVQ.API.Model
{
    public enum State { Ready=0, Update=1, Scheduled=2, Updating=3 };

    public abstract class BaseModel
    {
        public State Status { set; get; }
    }
}
