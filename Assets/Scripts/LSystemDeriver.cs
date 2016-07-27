using System;

public static class LSystemDeriver
{
    public static void Derive(string axiom, float angle, int derivations, ProductionRuleSet rules, out string moduleString)
    {
        moduleString = axiom;
        for (int i = 0; i < Math.Max(1, derivations); i++)
        {
            string newModuleString = "";
            for (int j = 0; j < moduleString.Length; j++)
            {
                string module = moduleString[j] + "";
                if (!rules.Contains(module))
                {
                    newModuleString += module;
                    continue;
                }
                ProductionRule productionRule = rules.Match(module);
                newModuleString += productionRule.successor;
            }
            moduleString = newModuleString;
        }
    }

}

