using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.TeamServer.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Formatting;
using Serilog.Templates;

namespace HardHatCore.TeamServer.Services
{
    public class LoggingService
    {
        public static Logger EventLogger { get; set;}
        public static Logger TaskLogger { get; set; }

        public static string LogFolderPath {get; private set;}
        
        public static void Init()
        {
            var dateTimeFunctions = new StaticMemberNameResolver(typeof(LoggingService));
            
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //split path at bin keyword
            string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
            //update each string in the array to replace \\ with the correct path seperator
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

            //if the log folder does not exist, create it
            DirectoryInfo logDirectory;
            if (!Directory.Exists(pathSplit[0] + "logs"))
            {
                logDirectory =  Directory.CreateDirectory(pathSplit[0] + "logs");
            }
            LogFolderPath = pathSplit[0] + "logs";
            //EventLogger = new LoggerConfiguration().WriteTo.File(new ExpressionTemplate(
            //    "{ {Timestamp: ToUtc(@t), Message: @m, Level: @l, @x, ..@p} }\n", nameResolver: dateTimeFunctions ),$"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Event_log.json").CreateLogger();


            //TaskLogger = new LoggerConfiguration().WriteTo.File(new ExpressionTemplate(
            //    "{ {Timestamp: ToUtc(@t), Message: @m, Level: @l, @x, ..@p} }\n", nameResolver: dateTimeFunctions), $"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Task_log.json").CreateLogger();

            var expressionTemplate = new ExpressionTemplate("{ {Timestamp: ToUtc(@t), Message: @m, Level: @l, @x, ..@p} }\n", nameResolver: dateTimeFunctions);

            EventLogger = new LoggerConfiguration().WriteTo.File(new IndentedJsonTextFormatter(expressionTemplate),$"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Event_log.json").CreateLogger();
            TaskLogger = new LoggerConfiguration().WriteTo.File(new IndentedJsonTextFormatter(expressionTemplate),$"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Task_log.json").CreateLogger();

            EventLogger.Information("Logging Service Started");
            TaskLogger.Information("Logging Service Started");
        }

        public static LogEventPropertyValue? ToUtc(LogEventPropertyValue? value)
        {
            if (value is ScalarValue scalar)
            {
                //make it so that the timestamp looks like yyyy-MM-dd HH:mm:ss
                if (scalar.Value is DateTimeOffset dto)
                {
                    return new ScalarValue(dto.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                //make it so that the timestamp looks like yyyy-MM-dd HH:mm:ss
                if (scalar.Value is DateTime dt)
                    return new ScalarValue(dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return null;
        }

        public static void ToPretty()
        {
            //read all the files at LogFolderPath and save a copy of the files as indented json, the files will be open by other programs so open it with readwrite access
            string[] files = Directory.GetFiles(LogFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                JArray jsonArray = JArray.Parse(json);
                string prettyJson = jsonArray.ToString(Newtonsoft.Json.Formatting.Indented);
                string filename = file.Split(".json")[0];
                File.WriteAllText($"{filename}_pretty.json", prettyJson);
            }
        }

        public static string HandleComplexTaskResultTypes(ExtImplantTaskResult_Base taskResult)
        {
            string result = "";
            //if the function is not a basic string then it got sent here and we should perform special formatting of the data result based on the type
            if(taskResult.ResponseType == ExtImplantTaskResponseType.FileSystemItem)
            {
                //deserialize the data into a list of file system items
                List<FileSystemItem> fileSystemItems = taskResult.Result.Deserialize<List<FileSystemItem>>();
                //get the string output for logging 
                result = HandleFileSystemItemResult(fileSystemItems);
            }
            return result;
        }

        private static string HandleFileSystemItemResult(List<FileSystemItem> taskResult)
        {
            //take the list of file system item and create a string version of output similiar to an ls command
            StringBuilder result = new StringBuilder();
            //create a header for the output
            result.AppendLine("Name Length CreationTimeUtc LastAccessTimeUtc LastWriteTimeUtc Owner");
            foreach (FileSystemItem item in taskResult)
            {
                result.AppendLine($"{item.Name} {item.Length} {item.CreationTimeUtc} {item.LastAccessTimeUtc} {item.LastWriteTimeUtc} {item.Owner}");
            }
            return result.ToString(); 
        }
    }


    public class IndentedJsonTextFormatter : ITextFormatter
    {
        private readonly ExpressionTemplate _expressionTemplate;

        public IndentedJsonTextFormatter(ExpressionTemplate expressionTemplate)
        {
            _expressionTemplate = expressionTemplate;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            using (var stringWriter = new StringWriter())
            {
                _expressionTemplate.Format(logEvent, stringWriter);
                var json = JObject.Parse(stringWriter.ToString());
                output.WriteLine(json.ToString(Formatting.Indented));
            }
        }
    }

}
