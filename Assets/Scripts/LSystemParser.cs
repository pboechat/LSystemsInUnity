using System;
using System.Collections.Generic;
using System.IO;

public static class LSystemParser
{
    public static void LoadFromString(string content, out string axiom, out float angle, out int derivations, out ProductionRuleSet rules)
    {
        axiom = "";
        angle = 0;
        derivations = 0;
        rules = new ProductionRuleSet();
        var lines = content.Split('\n');
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length == 0)
                continue;
            else if (line.Length == 1 && line[0] == '\r')
                continue;
            else if (line[0] == '/' && line[1] == '/')
                continue;
            string value;
            if (line.IndexOf("axiom") != -1)
            {
                value = line.Substring(line.IndexOf("=") + 1);
                value = value.Trim();
                axiom = value;
            }
            else if (line.IndexOf("angle") != -1)
            {
                value = line.Substring(line.IndexOf("=") + 1);
                value = value.Trim();
                angle = float.Parse(value);
            }
            else if (line.IndexOf("number of derivations") != -1)
            {
                value = line.Substring(line.IndexOf("=") + 1);
                value = value.Trim();
                derivations = int.Parse(value);
            }
            else
            {
                ProductionRule productionRule = ProductionRule.Build(line);
                rules.Add(productionRule);
            }
        }

        if (!rules.CheckProbabilities())
            throw new Exception("There's one of more production rules with probability < 1");
    }

}
