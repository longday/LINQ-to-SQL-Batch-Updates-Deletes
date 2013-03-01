using System;
using System.Linq;
using System.Linq.Expressions;

namespace BTR.Core.Linq
{
	public static class ExpressionExtensions
	{
		public static Expression Visit<T>(
			this Expression exp,
			Func<T, Expression> visitor,
			bool visitReplacement = true ) where T : Expression
		{
			return ExpressionVisitor<T>.Visit( exp, visitor, visitReplacement );
		}

		public static TExp Visit<T, TExp>(
			this TExp exp,
			Func<T, Expression> visitor,
			bool visitReplacement = true )
			where T : Expression
			where TExp : Expression
		{
			return (TExp)ExpressionVisitor<T>.Visit( exp, visitor, visitReplacement );
		}

		public static Expression<TDelegate> Visit<T, TDelegate>(
			this Expression<TDelegate> exp,
			Func<T, Expression> visitor,
			bool visitReplacement = true ) where T : Expression
		{
			return ExpressionVisitor<T>.Visit<TDelegate>( exp, visitor, visitReplacement );
		}

		public static IQueryable<TSource> Visit<T, TSource>(
			this IQueryable<TSource> source,
			Func<T, Expression> visitor,
			bool visitReplacement = true ) where T : Expression
		{
			return source.Provider.CreateQuery<TSource>( ExpressionVisitor<T>.Visit( source.Expression, visitor, visitReplacement ) );
		}
	}

	/// <summary>
	/// This class visits every Parameter expression in an expression tree and calls a delegate
	/// to optionally replace the parameter.  This is useful where two expression trees need to
	/// be merged (and they don't share the same ParameterExpressions).
	/// </summary>
	public class ExpressionVisitor<T> : ExpressionVisitor where T : Expression
	{
		private Func<T, Expression> visitor;
		private bool visitReplacement;

		public ExpressionVisitor( Func<T, Expression> visitor, bool visitReplacement = true )
		{
			this.visitor = visitor;
			this.visitReplacement = visitReplacement;
		}

		public static Expression Visit(
			Expression exp,
			Func<T, Expression> visitor,
			bool visitReplacement = true )
		{
			return new ExpressionVisitor<T>( visitor, visitReplacement ).Visit( exp );
		}

		public static Expression<TDelegate> Visit<TDelegate>(
			Expression<TDelegate> exp,
			Func<T, Expression> visitor,
			bool visitReplacement = true )
		{
			return (Expression<TDelegate>)new ExpressionVisitor<T>( visitor, visitReplacement ).Visit( exp );
		}

		public override Expression Visit( Expression exp )
		{
			var result = ( exp is T && visitor != null ) ? visitor( (T)exp ) : exp;

			return ( result != exp && !visitReplacement ) ? result : base.Visit( result );
		}
	}
}
