
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace logSharp.model
{
    public class LogList : LogTerm
    {
        public LogList()
        {
            Name = LogTerm.listConstructor;
        }

        public LogList(List<LogValuable> parameters)
        {
            Name = LogTerm.listConstructor;
            
            LogValuable tail = new LogList();
            
            
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                LogTerm term = new LogTerm(LogTerm.listConstructor,new List<LogValuable>(){parameters[i],tail});
                tail = term;
            }

            Parameters = (tail as LogTerm).Parameters;
        }

        
    }
}