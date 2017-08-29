// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MGPG
{
    internal class Cli
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                PrintHelp();
                Environment.Exit(0);
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
        }
    }
}