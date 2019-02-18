
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace logSharp.model
{
    public class List : Term
    {
        public List()
        {
            Name = Term.listConstructor;
        }

        public List(List<Valuable> parameters)
        {
            Name = Term.listConstructor;
            
            Valuable tail = new List();
            
            
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                Term term = new Term(Term.listConstructor,new List<Valuable>(){parameters[i],tail});
                tail = term;
            }

            Parameters = (tail as Term).Parameters;
        }

        
    }
}