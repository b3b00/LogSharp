namespace logSharp.model
{
    public interface LogValuable : LogSharpAST
    {
        bool IsScalar { get;  }
        bool IsVariable { get;  }
        bool IsTerm { get;  }
        string Name{ get;  }
    }
}