using DynamicEngLoading;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class DataChunking : EngineerCommand
    {
        public override string Name => "DataChunking";

        public override async Task Execute(EngineerTask task)
        {
            if(task.Arguments.ContainsKey("/Enable"))
            {
                Program.IsDataChunked = true;
                if (task.Arguments.ContainsKey("/Size"))
                {
                    Program.ChunkSize = int.Parse(task.Arguments["/Size"]);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Data Chunking Enabled & Chunk Size Set To: {Program.ChunkSize}", task, EngTaskStatus.Complete, TaskResponseType.String);
                    return;
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Data Chunking Enabled", task, EngTaskStatus.Complete, TaskResponseType.String);
                return;
            }
            else if(task.Arguments.ContainsKey("/Disable"))
            {
                Program.IsDataChunked = false;
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Data Chunking Disabled", task, EngTaskStatus.Complete, TaskResponseType.String);
                return;
            }
            else if(task.Arguments.ContainsKey("/Size"))
            {
                Program.ChunkSize = int.Parse(task.Arguments["/Size"]);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Chunk Size Set To: {Program.ChunkSize}", task, EngTaskStatus.Complete, TaskResponseType.String);
                return;
            }
            else
            {

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("DataChunking Status:");
                stringBuilder.AppendLine($"IsDataChunked: {Program.IsDataChunked}");
                stringBuilder.AppendLine($"ChunkSize: {Program.ChunkSize}");
                if (Program.typesWithModuleAttribute.Where(attr => attr.Name.Equals("DataChunk", StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    stringBuilder.AppendLine($"DataChunk Module: Loaded");
                }
                else
                {
                    stringBuilder.AppendLine($"DataChunk Module: Not Loaded");
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(stringBuilder.ToString(), task, EngTaskStatus.Complete, TaskResponseType.String);
                return;
            }

        }
    }
}
