using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class CommandLineArgsHelper
{
    public static Dictionary<string, List<string>> getArgsDictionary()
    {
        //raw args, without first one (the exe path)
        var argsRaw = Environment.GetCommandLineArgs().Skip(1).Select(x => x.ToLower()).ToArray();

        // -> dictionay, -command [extras...] ==> [command, {extras...}]
        var args = new Dictionary<string, List<string>>();
        List<String> currentExtras = null;
        foreach (var item in argsRaw)
        {
            if (item.StartsWith("-") || args.Count == 0)
            {
                currentExtras = new List<string>();
                args[item.Remove(0, 1).Trim()] = currentExtras;
            }
            else
            {
                if (currentExtras != null)
                    currentExtras.Add(item);
            }
        }
        return args;
    }

    public static bool getExtraAsBool(string extra)
    {
        return (
            extra == "on" ||
            extra == "1" ||
            extra == "true");
    }
}
