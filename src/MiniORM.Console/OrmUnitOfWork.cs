using AdoNet.Persistence.Repositories;
using AdoNet.Persistence.UnitOfWorks;
using MiniOrm.Console.Entities;

namespace MiniOrm.Console.UnitOfWorks;

public class OrmUnitOfWork : UnitOfWork
{
    public Repository<Course> Courses { get; }

    internal OrmUnitOfWork(string connectionString) : base(connectionString)
    {
        Courses = base.Set<Course>();
    }

    public void Commit()
    {
        base.Save();
    }
}
