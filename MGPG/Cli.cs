// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MGPG
{
    internal static class Cli
    {
        private static void Main(string[] args)
        {
            string tmpl = null;
            if (args.Length > 0)
            {
                tmpl = args[0];
                if (!Path.HasExtension(tmpl))
                    tmpl += ".xml";
                // the template path argument is relative to the MGPG executable
                var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                tmpl = Path.Combine(exeDir, tmpl);
            }

            if (args.Length == 0 || (args.Length == 1 && (args[0] == "--help" || args[0] == "-h")))
            {
                PrintHelp();
                Environment.Exit(1);
            }
            if (args.Length == 1)
            {
                var logger = new Logger {LogLevel = LogLevel.Warning};
                var t = new Template(tmpl, logger);
                Console.WriteLine();
                WriteTemplateData(t);
                Environment.Exit(0);
            }

            var dst = args[1];
            var sln = args.Length >= 3  && args[2].EndsWith(".sln") ? args[2] : null;

            var varIndex = sln == null ? 2 : 3;
            var vars = ParseVariables(args, varIndex);

            var g = new Generator();
            g.LoadTemplate(tmpl);

            var generatorArgs = new GeneratorArguments
            {
                DestinationFolder = dst,
                Solution = sln,
                Variables = vars
            };

            g.Generate(generatorArgs);
        }

        private static void WriteTemplateData(Template template)
        {
            Console.WriteLine($"Name:        {template.Name}");
            Console.WriteLine($"Description: {template.Description}");
            Console.WriteLine();
            Console.WriteLine("Variables:");
            Console.WriteLine();

            const int spacing = 4;
            var vars = template.Variables.Where(t => !t.Hidden).ToList();
            var names = FormatColumn(vars.Select(v => v.Name.ToString()), "Name", spacing).ToList();
            var types = FormatColumn(vars.Select(v => v.Type.ToString()), "Type", spacing).ToList();
            var values = FormatColumn(vars.Select(v => v.HasValue ? $"{v.Value}" : string.Empty), "Default", spacing).ToList();
            var semantics = FormatColumn(vars.Select(v => v.HasSemantic ? $"{v.Semantic}": string.Empty), "Semantic", spacing).ToList();

            var lines = Enumerable.Repeat(string.Empty, vars.Count + 3);
            lines = lines.Zip(names, (l, s) => l + s);
            lines = lines.Zip(types, (l, s) => l + s);
            lines = lines.Zip(values, (l, s) => l + s);
            lines = lines.Zip(semantics, (l, s) => l + s);
            foreach (var l in lines)
                Console.WriteLine(l);
        }

        private static IEnumerable<string> FormatColumn(IEnumerable<string> strings, string header, int extraPadding)
        {
            strings = new[] {header, new string('=', header.Length), string.Empty}.Concat(strings);
            var length = strings.Max(s => s.Length);
            return strings.Select(s => s.PadRight(length + extraPadding));
        }

        private static Dictionary<string, string> ParseVariables(string[] args, int firstIndex)
        {
            var vars = new Dictionary<string, string>();

            for (var varIndex = firstIndex; varIndex < args.Length; varIndex++)
            {
                var kv = args[varIndex].Split(':');
                if (kv.Length == 1)
                    vars[kv[0]] = "true";
                else if (kv.Length == 2)
                    vars[kv[0]] = kv[1];
                else
                    Console.WriteLine($"Wrong format '{args[varIndex]}'; should be <variable>:<value>.");
            }
            return vars;
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: MGPG <template>");
            Console.WriteLine("       MGPG <template> <destinationDir> [<solution>] (<variable>:<value> )*");
        }
    }
}