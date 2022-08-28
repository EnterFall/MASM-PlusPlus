using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Project_MASM__
{
    class Parser
    {
        public MatchEvaluator Method { get; }
        public Regex Reg { get; }

        public Parser(Regex reg, MatchEvaluator method)
        {
            Reg = reg;
            Method = method;
        }
    }
}
