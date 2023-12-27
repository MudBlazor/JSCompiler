using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MudBlazor.JSCompiler
{
    public class CompileJS : Task
    {
        public string SourceDirectory { get; set; } = "TScripts";

        public string DestinationFile { get; set; } = "wwwroot/MudBlazor.min.js";

        [Output]
        public ITaskItem GeneratedFile { get; set; }

        public override bool Execute()
        {
            File.Delete(DestinationFile);
            var sourceDirectory = new DirectoryInfo(SourceDirectory);
            Log.LogMessage(MessageImportance.High, sourceDirectory.FullName);
            var files = sourceDirectory.GetFiles("*.js");

            foreach (var file in files)
            {
                var fileText = File.ReadAllText(file.FullName);
                Log.LogMessage(MessageImportance.High, file.Name);
                File.AppendAllText(DestinationFile, fileText);
            }

            using (var reader = new StreamReader(DestinationFile))
            {
                var writer = new StringWriter();
                JavaScriptCompressor.Compress(reader, writer);
                File.WriteAllText(DestinationFile, writer.GetStringBuilder().ToString(), reader.CurrentEncoding);
            }

            Log.LogMessage(MessageImportance.High, "CompileJS Complete");
            GeneratedFile = new TaskItem(DestinationFile);
            return true;
        }
    }
}
