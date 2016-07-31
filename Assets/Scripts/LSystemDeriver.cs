using System;
using System.Collections.Generic;

public static class LSystemDeriver
{
    public static void Derive(string axiom, float angle, int derivations, Dictionary<string, List<Production>> productions, out string moduleString)
    {
        moduleString = axiom;
        for (int i = 0; i < Math.Max(1, derivations); i++)
        {
            string newModuleString = "";
            for (int j = 0; j < moduleString.Length; j++)
            {
                string module = moduleString[j] + "";
                if (!productions.ContainsKey(module))
                {
                    newModuleString += module;
                    continue;
                }
                var production = ProductionMatcher.Match(module, productions);
                newModuleString += production.successor;
            }
            moduleString = newModuleString;
        }
    }

}

