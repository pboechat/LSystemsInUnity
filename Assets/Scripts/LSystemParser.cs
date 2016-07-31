using System;
using System.Collections.Generic;

public static class LSystemParser
{
    public static void Parse(string content, out string axiom, out float angle, out int derivations, out Dictionary<string, List<Production>> productions)
    {
        axiom = "";
        angle = 0;
        derivations = 0;
        productions = new Dictionary<string, List<Production>>();
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
                string[] tokens = line.Split('=');
                if (tokens.Length != 2)
                    continue;
                string predecessor = tokens[0].Trim();
                tokens = tokens[1].Trim().Split(')');
                string probabilityString = tokens[0].Substring(1);
                string successor = tokens[1];
                float probability = float.Parse(probabilityString);
                if (!productions.ContainsKey(predecessor))
                    productions[predecessor] = new List<Production>();
                productions[predecessor].Add(new Production(predecessor, successor, probability));
            }
        }
        if (!ProductionMatcher.CheckProbabilities(productions))
            throw new Exception("There's one of more production rules with probability < 1");
    }

}
