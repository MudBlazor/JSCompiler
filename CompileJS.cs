using System;
using System.IO;
using System.Text;
using System.Threading;
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
            var sourceFiles = new DirectoryInfo(SourceDirectory).GetFiles("*.js");
            var maxSourceWriteTime = new DateTime();

            foreach (var sourceFile in sourceFiles)
            {
                maxSourceWriteTime = sourceFile.LastWriteTime > maxSourceWriteTime ? sourceFile.LastWriteTime : maxSourceWriteTime;
            }

            using (Mutex mutex = new Mutex(false, DestinationFile))
            {
                mutex.WaitOne(5000);

                if (File.Exists(DestinationFile))
                {
                    if (File.GetLastWriteTime(DestinationFile) < maxSourceWriteTime)
                    {
                        WriteFile();
                        Log.LogMessage(MessageImportance.High, $"{DestinationFile} Updated");
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.High, $"{DestinationFile} UpToDate");
                    }
                }
                else
                {
                    WriteFile();
                    Log.LogMessage(MessageImportance.High, $"{DestinationFile} Created");
                }

                mutex.ReleaseMutex();
            }

            GeneratedFile = new TaskItem(DestinationFile);
            return true;
        }

        private void WriteFile()
        {
            var sourceFiles = new DirectoryInfo(SourceDirectory).GetFiles("*.js");
            var combinedJS = new StringBuilder();
            foreach (var sourceFile in sourceFiles)
            {
                var fileText = File.ReadAllText(sourceFile.FullName);
                combinedJS.Append(fileText);
            }
            var compressedJS = JavaScriptCompressor.Compress(combinedJS.ToString());
            File.WriteAllText(DestinationFile, compressedJS);
        }
    }
}
