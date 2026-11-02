using System.Collections.Generic;
using System.Linq;
using logSharp.interpretation.unification;
using NFluent;
using NFluent.Extensibility;

namespace ogsharpTests;

public static class NFluentExtensions
{
    
    public static ICheckLink<ICheck<Unification>> IsOk(this ICheck<Unification> context )
    {
        ExtensibilityHelper.BeginCheck(context)
            .FailWhen(sut => !sut.IsOk, "unification failed.")
            .FailWhen(sut => sut.Environment.Environment.Count > 0, "unification environment is not empty.")
            .OnNegate("unification expected to fail.");
        return ExtensibilityHelper.BuildCheckLink(context);
    }
    
    public static ICheckLink<ICheck<Unification>>Contains(this ICheck<Unification> context, string name, string expectedValue )
    {
        ExtensibilityHelper.BeginCheck(context)
            .FailWhen(sut => !sut.HasBinding(name), $"unification does not contain {name} variable.")
            .FailWhen(sut =>
            {
                var value = sut.GetBinding(name);
                if (value != null)
                {
                    return value.ToString() == expectedValue;
                }
                return false;
            }, "variable {name} does not have expected value.")
            .OnNegate("unification expected to fail.");
        return ExtensibilityHelper.BuildCheckLink(context);
    }
    
     
}