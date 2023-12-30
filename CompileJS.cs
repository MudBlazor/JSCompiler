using System;
using System.IO;
using System.Text;
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
            var sourceDirectory = new DirectoryInfo(SourceDirectory);
            var sourceFiles = sourceDirectory.GetFiles("*.js");
            var maxSourceWriteTime = new DateTime();
            var combinedJS = new StringBuilder();

            foreach (var sourceFile in sourceFiles)
            {
                maxSourceWriteTime = sourceFile.LastWriteTime > maxSourceWriteTime ? sourceFile.LastWriteTime : maxSourceWriteTime;
                var fileText = File.ReadAllText(sourceFile.FullName);
                combinedJS.Append(fileText);
            }

            var compressedJS = JavaScriptCompressor.Compress(combinedJS.ToString());

            if (File.Exists(DestinationFile))
            {
                if (File.GetLastWriteTime(DestinationFile) < maxSourceWriteTime)
                {
                    File.WriteAllText(DestinationFile, compressedJS);
                    Log.LogMessage(MessageImportance.High, $"{DestinationFile} Updated");
                }
                else
                {
                    Log.LogMessage(MessageImportance.High, $"{DestinationFile} UpToDate");
                }
            }
            else
            {
                File.WriteAllText(DestinationFile, compressedJS);
                Log.LogMessage(MessageImportance.High, $"{DestinationFile} Created");
            }

            GeneratedFile = new TaskItem(DestinationFile);
            return true;
        }
    }
}
