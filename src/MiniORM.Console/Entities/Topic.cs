namespace MiniOrm.Console.Entities;

public class Topic
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<Session> Sessions { get; set; }
}
