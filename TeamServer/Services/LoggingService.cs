using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Formatting.Json;
using Serilog.Templates;
using System;
using System.IO;
using System.Reflection;
using TeamServer.Services.Extra;

namespace TeamServer.Services    
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
            EventLogger = new LoggerConfiguration().WriteTo.File(new ExpressionTemplate(
                "{ {Timestamp: ToUtc(@t), Message: @m, Level: @l, @x, ..@p} }\n", nameResolver: dateTimeFunctions ),$"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Event_log.json").CreateLogger();
            
            
            TaskLogger = new LoggerConfiguration().WriteTo.File(new ExpressionTemplate(
                "{ {Timestamp: ToUtc(@t), Message: @m, Level: @l, @x, ..@p} }\n", nameResolver: dateTimeFunctions), $"{pathSplit[0] + "logs"}{allPlatformPathSeperator}Task_log.json").CreateLogger();

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



    }
}
