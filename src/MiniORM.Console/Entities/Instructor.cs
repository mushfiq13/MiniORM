namespace MiniOrm.Console.Entities;

public class Instructor
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Phone> PhoneNumbers { get; set; }
}
