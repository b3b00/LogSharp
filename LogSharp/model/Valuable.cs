using System.Collections.Generic;

namespace logSharp.model
{
    public interface IValuable : ILogSharpAst
    {
        Dictionary<string,object> Data { get; set; }
        bool IsScalar { get;  }
        bool IsVariable { get;  }
        
        bool IsDiscardableVariable { get;  }
        
        bool IsTerm { get;  }
        
        bool IsList { get;  }
        
        bool IsConstantList { get;  }
        string Name{ get;  }
    }
}