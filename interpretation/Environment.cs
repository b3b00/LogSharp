using System.Collections.Generic;
using System.Text;
using logSharp.model;

namespace logSharp.interpretation
{
    public class TermEnvironment
    {
        public Dictionary<string,Valuable> Environment { get; set; }

        public TermEnvironment()
        {
            Environment = new Dictionary<string, Valuable>();
        }

        public bool ContainsVariable(string key)
        {
            return Environment.ContainsKey(key);
        }
        
        public Valuable this[string key]
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

      
        public string ToString()
        {
            StringBuilder dump = new StringBuilder();
            foreach (var kvp in Environment)
            {
                dump.AppendLine($"{kvp.Key} = {kvp.Value.Dump()}");
            }

            return dump.ToString();
        }
        
    }
}