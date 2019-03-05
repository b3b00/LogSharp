using System;
using System.Collections.Generic;
using logSharp.interpretation;
using logSharp.interpretation.unification;

namespace logSharp.builtins.expressions.model
{

    public delegate object Operation(ExpressionToken oper, IComparable left, IComparable right);
    
    public class BinaryOperation : Expression
    {
        private static readonly Type IntType = typeof(Int32);

        private readonly Expression LeftExpresion;
        private readonly ExpressionToken Operator;
        private readonly Expression RightExpression;


        private static object Compare(ExpressionToken oper, IComparable left, IComparable right)
        {
            var leftType = left.GetType();
            var rightType = right.GetType();
            int comparison = left.CompareTo(right);
            switch (oper)
            {
                case ExpressionToken.EQUALS:
                {
                    return comparison == 0;
                    break;
                }
                case ExpressionToken.DIFFERENT :
                {
                    return comparison != 0;
                    break;
                }
                case ExpressionToken.GT:
                {
                    return comparison > 0;
                    break;
                }
                case ExpressionToken.LT:
                {
                    return comparison < 0;
                    break;
                }
                case ExpressionToken.GTE:
                {
                    return comparison >= 0;
                    break;
                }
                case ExpressionToken.LTE:
                {
                    return comparison <= 0;
                    break;
                }
            }

            return false;
        }


        private static object Add(ExpressionToken oper, object left, object right)
        {
            var leftType = left.GetType();
             
            var rightType = right.GetType();

            if (leftType == typeof(string) || rightType == typeof(string))
            {
                return left.ToString() + right.ToString();
            }
            
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return (double)left + (double)right;
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return (int) left + (int) right;
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for addition {leftType}  and {rightType}");

        }
        
        public static object Sub(ExpressionToken oper, object left, object right)
        {
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return (double)left - (double)right;
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return (int) left - (int) right;
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for subsctraction {leftType}  and {rightType}");
        }
        
        public static object Mul(ExpressionToken oper, object left, object right)
        {
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return (double)left * (double)right;
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return (int) left * (int) right;
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for multiplication {leftType}  and {rightType}");
        }
        
        public static object Div(ExpressionToken oper, object left, object right)
        {
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return (double)left / (double)right;
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return (int) left / (int) right;
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for division {leftType}  and {rightType}");
        }
        
        public static Operation GetOperation(ExpressionToken operation)
        {
            switch (operation)
            {
                case ExpressionToken.PLUS:
                {
                    return Add;
                }
                case ExpressionToken.MINUS:
                {
                    return Sub;
                }
                case ExpressionToken.TIMES:
                {
                    return Mul;
                }
                case ExpressionToken.DIVIDE:
                {
                    return Div;
                }
                case ExpressionToken.EQUALS:
                case ExpressionToken.DIFFERENT:
                case ExpressionToken.GT:
                case ExpressionToken.LT:
                case ExpressionToken.GTE:
                case ExpressionToken.LTE:
                {
                    return Compare;
                }
                
                default:
                {
                    return null;
                }
            }
        }


        public BinaryOperation(Expression left, ExpressionToken op, Expression right)
        {
            LeftExpresion = left;
            Operator = op;
            RightExpression = right;

            
        }

        public object Evaluate(Unification unification)
        {
            var left = LeftExpresion.Evaluate(unification);
            var right = RightExpression.Evaluate(unification);

            if (left != null && right != null)
            {
                Operation operation = GetOperation(Operator);

                if (operation != null)
                {
                    return operation(Operator,(IComparable)left, (IComparable)right);
                }
                
            }
                
            throw new ExpressionException(ExpressionError.Error,$"bad operators for {Operator}");
        }
        
        
        
        
        
    }
}