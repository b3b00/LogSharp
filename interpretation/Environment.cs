using System.Collections.Generic;
using logSharp.model;

namespace logSharp.interpretation
{
    public class TermEnvironment
    {
        public Dictionary<string,LogValuable> Environment { get; set; }

        public TermEnvironment()
        {
            Environment = new Dictionary<string, LogValuable>();
        }

        public bool ContainsVariable(string key)
        {
            return Environment.ContainsKey(key);
        }
        
        public LogValuable this[string key]
        {
            get
            {
                return ContainsVariable(key) ? Environment[key] : null;
            }
            set
            {
                Environment[key] = value;
            }
        }

        public TermEnvironment Clone()
        {
            TermEnvironment clone = new TermEnvironment();
            foreach (var pair in Environment)
            {
                clone[pair.Key] = pair.Value;
            }

            return clone;
        }
        
    }
}