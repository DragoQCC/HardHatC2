using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HardHatCore.ApiModels.Aspects.ContractorSystem_InvocationPoints;

namespace HardHatCore.ContractorSystemGenerator
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom(args[0]);

            // Analyze the assembly
            Program program = new Program();
            program.AnalyzeAssembly(assembly);
        }

        public void AnalyzeAssembly(Assembly assembly)
        {
            StringBuilder markdownContent = new StringBuilder();

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    foreach (var attribute in method.GetCustomAttributes(inherit: false))
                    {
                        if (attribute is FuncCallAspect funcCallAspect) // Check and cast the attribute
                        {
                            // Now you can directly access the properties of FuncCallAspect
                            string contextName = funcCallAspect.ContextName;
                            string contextDescription = funcCallAspect.ContextDescription;
                            string sourceLocation = funcCallAspect.SourceLocation;

                            // Gather method info
                            var methodInfo = new ContractorSystemHookedMethodModel
                            {
                                Name = method.Name,
                                ContextName = contextName,
                                Description = contextDescription,
                                SourceLocation = sourceLocation,
                            };

                            // Generate Markdown content
                            markdownContent.AppendLine($"## {methodInfo.ContextName}");
                            markdownContent.AppendLine($"### Start Context:\n {methodInfo.ContextName}");
                            markdownContent.AppendLine($"### End Context:\n {methodInfo.ContextName}_End");
                            markdownContent.AppendLine($"### Source Location:\n {methodInfo.SourceLocation}");
                            markdownContent.AppendLine($"### Description:\n {methodInfo.Description}");

                            //try to analyze parameters to break down complex types
                            var parametersInfo = new Dictionary<string, TypeDetails>();
                            markdownContent.AppendLine($"### Parameters:");
                            foreach (var param in method.GetParameters())
                            {
                                var paramTypeDetails = AnalyzeType(param.ParameterType);
                                parametersInfo.TryAdd(param.Name, paramTypeDetails);
                                markdownContent.AppendLine($"- {param.Name} : {paramTypeDetails.TypeName}");
                                // Include detailed type information if necessary
                                if (paramTypeDetails.Properties.Count > 0)
                                {
                                    markdownContent.AppendLine("\t- **Properties:**");
                                    foreach (var prop in paramTypeDetails.Properties)
                                    {
                                        markdownContent.AppendLine($"\t\t- {prop.PropName} : {prop.TypeName}");
                                    }
                                }
                            }
                            methodInfo.Parameters = parametersInfo;
                            //try to analyze the return type to break down complex types
                            var returnTypeDetails = AnalyzeType(method.ReturnType);
                            var returnInfo = new Dictionary<string, TypeDetails>();
                            markdownContent.AppendLine($"### Returns:\n - {returnTypeDetails.TypeName}");
                            // Include detailed type information if necessary
                            if (returnTypeDetails.Properties.Count > 0)
                            {
                                markdownContent.AppendLine("\t- **Properties:**");
                                returnInfo.TryAdd(returnTypeDetails.TypeName, returnTypeDetails);
                                foreach (var prop in returnTypeDetails.Properties)
                                {
                                    markdownContent.AppendLine($"\t\t- {prop.PropName} : {prop.TypeName}");
                                }
                            }
                            methodInfo.ReturnType = returnInfo;
                            
                            //pretty print the json
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            // Serialize JSON and append to markdownContent
                            string jsonContent = JsonSerializer.Serialize(methodInfo, options);

                            //remove unicode characters like \uxxxx using regex
                            jsonContent = Regex.Unescape(jsonContent);
                            //remove the extra ` around the type names
                            jsonContent = jsonContent.Replace("`", "");
                            markdownContent.AppendLine("### Json Content");
                            markdownContent.AppendLine("```json");
                            markdownContent.AppendLine(jsonContent);
                            markdownContent.AppendLine("```");

                            // Write markdownContent to a file
                            string docsPath = Path.Combine(Environment.CurrentDirectory, "Docs");
                            Directory.CreateDirectory(docsPath); // Ensure directory exists
                            string markdownFilePath = Path.Combine(docsPath, "GeneratedDocumentation.md");
                            File.WriteAllText(markdownFilePath, markdownContent.ToString());
                        }
                    }
                }
            }
        }

        public static TypeDetails AnalyzeType(Type type, string probName = null)
        {

            // Skip analysis for specific types or namespaces
            if (ShouldSkipTypeAnalysis(type))
            {
                //if the type is a collection the typeName should include the type of the collection for example List<string>
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericArguments()[0];
                    string typename = $"`{type.Name}<{genericType.Name}>`";
                    // remove the `1 that gets added to the typename
                    typename = typename.Replace("`1", "");
                    return new TypeDetails {TypeName = typename , PropName = probName };
                }
                return new TypeDetails { TypeName = $"`{type.Name}`" , PropName = probName };
            }
            //otherwise the type comes from HardHat and we need to break it down
            var typeDetails = new TypeDetails { TypeName = $"`{type.Name}`", PropName = probName };
            foreach (var prop in type.GetProperties())
            {
                var propDetails = AnalyzeType(prop.PropertyType,prop.Name);
                typeDetails.Properties.Add(propDetails);
            }
            return typeDetails;
        }

        public static bool ShouldSkipTypeAnalysis(Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
            {
                return true;
            }
            else if (type.Namespace.Contains("HardHat",StringComparison.CurrentCultureIgnoreCase) is false)
            {
                return true;
            }
            
            return false;
        }
    }


    public class ContractorSystemHookedMethodModel
    {
        public string Name { get; set; }
        public string ContextName { get; set; }
        public string Description { get; set; }
        public string SourceLocation { get; set; }
        public Dictionary<string, TypeDetails> Parameters { get; set; } = new();
        public Dictionary<string, TypeDetails> ReturnType { get; set; } = new();
    }

    public class TypeDetails
    {
        public string TypeName { get; set; }
        public string PropName { get; set; }
        public List<TypeDetails> Properties { get; set; } = new List<TypeDetails>();
    }
}