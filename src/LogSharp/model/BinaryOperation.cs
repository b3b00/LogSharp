using System;
using System.Collections.Generic;
using logSharp.interpretation;
using logSharp.interpretation.unification;
using logSharp.model;
using logSharp.parser;

namespace logSharp.model
{

    public delegate object Operation(LogSharpToken oper, IComparable left, IComparable right);
    
    public class BinaryOperation : Scalar, LogSharpAST, IExpression
    {
        private static readonly Type IntType = typeof(Int32);

        private readonly LogSharpAST _leftExpression;
        public LogSharpAST LeftExpresion => _leftExpression;
        
        private readonly LogSharpToken _operator;
        public LogSharpToken Operator => _operator;
        
        private readonly LogSharpAST _rightExpression;
        public  LogSharpAST RightExpression => _rightExpression;
            
        

        private static object Compare(LogSharpToken oper, IComparable left, IComparable right)
        {
            
            var logAndReturn = (string loggedOperation, Func<object, object, object> f) =>
            {
                var result = f(left, right);
                Console.WriteLine($"Binary : {left} {loggedOperation} {right} = {result}");
                return result;
            };
            
            var leftType = left.GetType();
            var rightType = right.GetType();
            int comparison = left.CompareTo(right);
            switch (oper)
            {
                case LogSharpToken.EqualsTo:
                {
                    return logAndReturn("==",(object l , object r ) => comparison == 0);
                }
                case LogSharpToken.Different :
                case LogSharpToken.NotUnifiable :
                {
                    return logAndReturn("!=",(object l , object r ) => comparison != 0);
                }
                case LogSharpToken.Gt:
                {
                    return logAndReturn(">",(object l , object r ) => comparison > 0);
                }
                case LogSharpToken.Lt:
                {
                    return logAndReturn("<",(object l , object r ) => comparison < 0);
                }
                case LogSharpToken.Gte:
                {
                    return logAndReturn(">=",(object l , object r ) => comparison >= 0);return comparison >= 0;
                }
                case LogSharpToken.Lte:
                {
                    return logAndReturn("<=",(object l , object r ) => comparison <= 0);
                }
            }

            return false;
        }


        private static object Add(LogSharpToken oper, object left, object right)
        {

            var logAndReturn = (string loggedOperation, Func<object, object, object> f) =>
            {
                var result = f(left, right);
                Console.WriteLine($"Binary : {left} {loggedOperation} {right} = {result}");
                return result;
            };
            
            var leftType = left.GetType();
             
            var rightType = right.GetType();

            if (leftType == typeof(string) || rightType == typeof(string))
            {
                return logAndReturn("+",(object l , object r ) => l.ToString() + r.ToString());
            }
            
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return logAndReturn("+",(object l , object r ) => (double)l + (double)r);
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return logAndReturn("+",(object l , object r ) => (int)l + (int)r);
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for addition {leftType}  and {rightType}");

        }
        
        public static object Sub(LogSharpToken oper, object left, object right)
        {
            
            var logAndReturn = (string loggedOperation, Func<object, object, object> f) =>
            {
                var result = f(left, right);
                Console.WriteLine($"Binary : {left} {loggedOperation} {right} = {result}");
                return result;
            };
            
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return logAndReturn("-",(object l , object r ) => (double)l - (double)r);
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return logAndReturn("-",(object l , object r ) => (int)l - (int)r);
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for subsctraction {leftType}  and {rightType}");
        }
        
        public static object Mul(LogSharpToken oper, object left, object right)
        {
            var logAndReturn = (string loggedOperation, Func<object, object, object> f) =>
            {
                var result = f(left, right);
                Console.WriteLine($"Binary : {left} {loggedOperation} {right} = {result}");
                return result;
            };
            
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return logAndReturn("*",(object l , object r ) => (double)l * (double)r);
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return logAndReturn("*",(object l , object r ) => (int)l * (int)r);
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for multiplication {leftType}  and {rightType}");
        }
        
        public static object Div(LogSharpToken oper, object left, object right)
        {
            var logAndReturn = (string loggedOperation, Func<object, object, object> f) =>
            {
                var result = f(left, right);
                Console.WriteLine($"Binary : {left} {loggedOperation} {right} = {result}");
                return result;
            };
            
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType == typeof(double) || rightType == typeof(double))
            {
                return logAndReturn("-",(object l , object r ) => (double)l / (double)r);
            }

            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return logAndReturn("-",(object l , object r ) => (int)l - (int)r);
            }
            throw new ExpressionException(ExpressionError.BadTypes,$"incompatible types for division {leftType}  and {rightType}");
        }
        
        public static Operation GetOperation(LogSharpToken operation)
        {
            switch (operation)
            {
                case LogSharpToken.Plus:
                {
                    return Add;
                }
                case LogSharpToken.Minus:
                {
                    return Sub;
                }
                case LogSharpToken.Times:
                {
                    return Mul;
                }
                case LogSharpToken.Divide:
                {
                    return Div;
                }
                case LogSharpToken.EqualsTo:
                case LogSharpToken.Different:
                case LogSharpToken.NotUnifiable:
                case LogSharpToken.Gt:
                case LogSharpToken.Lt:
                case LogSharpToken.Gte:
                case LogSharpToken.Lte:
                {
                    return Compare;
                }
                
                default:
                {
                    return null;
                }
            }
        }


        public BinaryOperation(LogSharpAST left, LogSharpToken op, LogSharpAST right)
        {
            _leftExpression = left;
            _operator = op;
            _rightExpression = right;
        }



        public object Evaluate(Unification unification)
        {
            var left = ((IExpression)LeftExpresion).Evaluate(unification);
            var right = ((IExpression)RightExpression).Evaluate(unification);

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


        public override string Dump()
        {
            return $"{LeftExpresion.Dump()} {OperatorsHelper.TokenToOperator(Operator)} {RightExpression.Dump()}";
        }

        public string ToString(string format, IFormatProvider formatProvider) => Dump();
    }
}