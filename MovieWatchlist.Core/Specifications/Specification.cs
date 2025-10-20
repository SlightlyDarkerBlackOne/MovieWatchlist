using System;
using System.Linq.Expressions;

namespace MovieWatchlist.Core.Specifications;

public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> m_left;
    private readonly Specification<T> m_right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        m_left = left;
        m_right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = m_left.ToExpression();
        var rightExpression = m_right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

internal class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> m_left;
    private readonly Specification<T> m_right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        m_left = left;
        m_right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = m_left.ToExpression();
        var rightExpression = m_right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> m_specification;

    public NotSpecification(Specification<T> specification)
    {
        m_specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = m_specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var notExpression = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(notExpression, parameter);
    }
}

