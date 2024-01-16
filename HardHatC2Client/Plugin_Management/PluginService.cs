using System.Reflection;
using System.Runtime.Loader;
using Fasterflect.Extensions;
using HardHatCore.HardHatC2Client.Plugin_BaseClasses;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Utilities;

namespace HardHatCore.HardHatC2Client.Plugin_Management
{
    public class PluginService
    {
        public static PluginHub pluginHub = new PluginHub();
        public static Dictionary<AssemblyLoadContext, WeakReference> PluginRefs = new();


        public static bool RefreshPlugins()
        {
            try
            {
                Console.WriteLine("Refreshing plugins");

                // Attempt to unload all the plugin contexts
                foreach (var pluginRef in PluginRefs)
                {
                    if (pluginRef.Value.IsAlive)
                    {
                        pluginRef.Key.Unload();
                        // Optionally, wait for the unloading to complete
                        for (int i = 0; i < 10 && pluginRef.Value.IsAlive; i++)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }
                PluginRefs.Clear();

                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                char allPlatformPathSeparator = Path.DirectorySeparatorChar;
                string[] pathSplit = assemblyBasePath.Split("bin");
                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeparator.ToString());
                string pluginFolderPath = pathSplit[0] + "Plugins";

                // Get all plugin DLLs
                var pluginPaths = Directory.GetFiles(pluginFolderPath, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var pluginPath in pluginPaths)
                {
                    // Create a new context that can be unloaded later
                    var pluginContext = new AssemblyLoadContext(pluginPath, isCollectible: true);

                    // Load the plugin into the new context
                    var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginPath);

                    // Parse the modules in the plugin
                    ParsePlugin(pluginAssembly);

                    // Add the plugin context to the dictionary of contexts
                    PluginRefs.Add(pluginContext, new WeakReference(pluginContext));
                }

                Console.WriteLine("Plugins refreshed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool InitPluginsWithCustomContext()
        {
            try
            {
                string basePath = HelperFunctions.GetBaseFolderLocation();
                string PluginFolderfilepath = basePath + "Plugins";
                //dont search nested directories
                var pluginPaths = Directory.GetFiles(PluginFolderfilepath, "*.dll", SearchOption.TopDirectoryOnly);
                //parse the current assembly
                ParsePlugin(Assembly.GetExecutingAssembly());
                //parse the plugin assemblies
                foreach (var pluginPath in pluginPaths)
                {
                    //create a new context that can be reloaded later
                    var pluginContext = new AssemblyLoadContext(pluginPath, true);
                    //load the plugin into the new context
                    var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginPath);
                    //parse the modules in the plugin, adding types to the corresponding lists in the pluginHub
                    ParsePlugin(pluginAssembly);
                    //add the plugin context to the dictionary of contexts
                    PluginRefs.Add(pluginContext, new WeakReference(pluginContext));
                }
                Console.WriteLine("Plugins loaded");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool ParsePlugin(Assembly plugin)
        {
            try
            {
                //get a list of all the imported types from the plugin hub properties structure is public IEnumerable<PluginType> PropertyName
                // Save all of the plugin types, then add them to the corresponding list in the pluginHub
                var pluginHubProperties = pluginHub.GetType().GetProperties();
                List<Type> ImportablepluginTypes = new List<Type>();
                foreach (var pluginHubProperty in pluginHubProperties)
                {
                    //get the type of the pluginHub property ex(IEnumerable<PluginType>)
                    var pluginHubPropertyType = pluginHubProperty.PropertyType;
                    //get the generic type of the pluginHub property
                    var pluginHubPropertyGenericType = pluginHubPropertyType.GenericTypeArguments[0];
                    //add the generic type to the list of types to import
                    ImportablepluginTypes.Add(pluginHubPropertyGenericType);
                }
                // parse the types in the plugin assembly, adding types to the corresponding lists in the pluginHub
                foreach (var ExportedType in plugin.GetTypes())
                {
                    //check if the exported type is a subclass of any of the types in the ImportablepluginTypes list
                    var matchedType = ImportablepluginTypes.FirstOrDefault(ExportedType.Implements);
                    if (matchedType is not null)
                    {
                        //get the pluginHub property that corresponds to the type
                        var pluginHubProperty = pluginHubProperties.FirstOrDefault(x => x.PropertyType.GenericTypeArguments[0] == matchedType);
                        //get the pluginHub property value
                        var pluginHubPropertyValue = pluginHubProperty.GetValue(pluginHub);
                        //add a check to see if the type is already in the list and remove it so it can reload 
                        pluginHubPropertyValue.GetType().GetMethod("Remove").Invoke(pluginHubPropertyValue, new object[] { Activator.CreateInstance(ExportedType) });

                        //get the add method of the pluginHub property value
                        var pluginHubPropertyAddMethod = pluginHubPropertyValue.GetType().GetMethod("Add");
                        //add the exported type to the pluginHub property value
                        pluginHubPropertyAddMethod.Invoke(pluginHubPropertyValue, new object[] { Activator.CreateInstance(ExportedType) });
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static IImplantCommandValidation GetCommandValidationPlugin(string pluginName)
        {
            var taskVal_plugins = pluginHub.ImplantTaskValidation_Plugins;
            var taskVal_plugin = taskVal_plugins.GetPluginEnumerableResult(pluginName);
            return taskVal_plugin;
        }

        public static IimplantCreation GetImplantCreationPlugin(string pluginName)
        {
            var creation_plugins = pluginHub.ImplantCreation_Plugins;
            var creation_plugin = creation_plugins.GetPluginEnumerableResult(pluginName);
            return creation_plugin;
        }

        public static IAssetNotificationService_Client GetAssetNotifService(string pluginName)
        {
            var AssetNotifPlugins = pluginHub.Asset_NotificationServicePlugins;
            var AssetNotifPlugin = AssetNotifPlugins.GetPluginEnumerableResult(pluginName);
            return AssetNotifPlugin;
        }
    }
}
