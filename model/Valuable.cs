namespace logSharp.model
{
    public interface Valuable : LogSharpAST
    {
        bool IsScalar { get;  }
        bool IsVariable { get;  }
        bool IsTerm { get;  }
        
        bool IsList { get;  }
        string Name{ get;  }
    }
}