// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MGPG.IdeTemplateWriters;

namespace MGPG
{
    internal class Cli
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                PrintHelp();
                Environment.Exit(1);
            }

            if (string.Equals(args[0],"--vs", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length != 3)
                {
                    PrintHelp();
                    Environment.Exit(1);
                }

                var templatePath = Path.GetFullPath(args[1]);
                var logger = new Logger();
                var t = new Template(templatePath, logger);
                var vsTemplateWriter = new VsTemplateWriter();
                vsTemplateWriter.WriteIdeTemplate(t, args[2], logger);
                return;
            }

            var tmpl = args[0];
            var dst = args[1];
            var sln = args.Length == 3 ? args[2] : null;

            var g = new Generator();

            g.Render += (s, a) => Console.WriteLine($"[Info] Rendered file {a.Source} to {a.Destination}.");

            var generatorArgs = new GeneratorArguments
            {
                DestinationFolder = dst,
                TemplateFile = tmpl,
                Solution = sln,
            };

            g.Generate(generatorArgs);
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: MGPG <template> <destinationDir> [<solution>]");
            Console.WriteLine("    OR MGPG --vs <template> <output>");
        }
    }
}