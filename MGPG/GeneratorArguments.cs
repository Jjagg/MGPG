using System.IO;

namespace MGPG
{
    public class GeneratorArguments
    {
        /// <summary>
        /// Path to the file that describes the template to generate.
        /// </summary>
        /// <remarks>
        /// TODO something about the file format and an example.
        /// </remarks>
        public string TemplateFile { get; set; }

        /// <summary>
        /// The folder in which to place the rendered files.
        /// </summary>
        public string DestinationFolder { get; set; }

        /// <summary>
        /// The path to the solution to add any rendered .csproj files to.
        /// If the solution does not exist yet it will be created.
        /// Set to <code>null</code> to not add the project to a solution.
        /// </summary>
        public string Solution { get; set; }

        public VariableCollection Variables { get; }

        public GeneratorArguments()
        {
            Variables = new VariableCollection();
        }
    }
}