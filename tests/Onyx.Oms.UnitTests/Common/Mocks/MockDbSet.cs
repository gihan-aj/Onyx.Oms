using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Onyx.Oms.UnitTests.Common.Mocks;

public static class MockDbSet
{
    public static DbSet<T> Create<T>(params T[] data) where T : class
    {
        var queryable = new TestAsyncEnumerable<T>(data);
        var dbSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

        ((IQueryable<T>)dbSet).Provider.Returns(queryable.AsQueryable().Provider);
        ((IQueryable<T>)dbSet).Expression.Returns(queryable.AsQueryable().Expression);
        ((IQueryable<T>)dbSet).ElementType.Returns(queryable.AsQueryable().ElementType);
        ((IQueryable<T>)dbSet).GetEnumerator().Returns(queryable.AsQueryable().GetEnumerator());
        ((IAsyncEnumerable<T>)dbSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(queryable.GetAsyncEnumerator());

        return dbSet;
    }
}
