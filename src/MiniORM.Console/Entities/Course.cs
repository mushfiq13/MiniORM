namespace MiniOrm.Console.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Instructor Teacher { get; set; }
    public List<Topic> Topics { get; set; }
}
