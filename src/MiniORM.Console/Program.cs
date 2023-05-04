using MiniOrm.Console.UnitOfWorks;

// Assuming Database with tables already exist!
var connectionString =
    @"Data Source=(localdb)\MSSQLLocalDB;Database=MiniOrm;Integrated Security=True;";

var unitOfWork = new OrmUnitOfWork(connectionString);
var courses = unitOfWork.Courses.GetAll();

foreach (var course in courses)
{
    Console.WriteLine(course.Title);
}

