using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Fasterflect;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_Management
{
    public class PluginService
    {
        public static PluginHub pluginHub = new PluginHub();
        public static Dictionary<AssemblyLoadContext,WeakReference> PluginRefs = new();


        public static void UnZipBuildTools()
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin");
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            //build Tools folder 
            string buildToolsFolder = pathSplit[0] +Helpers.PathingTraverseUpString + Helpers.PlatPathSeperator + "Assets" + Helpers.PlatPathSeperator + "BuildTools";
            //for each Zip file in the build tools folder unzip it with the same name as the zip file
            foreach (string file in Directory.EnumerateFiles(buildToolsFolder, "*.zip"))
            {
                string zipPath = file;
                string extractPath = buildToolsFolder;
                ZipFile.ExtractToDirectory(zipPath, extractPath,overwriteFiles:true);
            }
        }

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
                Console.WriteLine("Unzipping build tools");
                UnZipBuildTools();
                Console.WriteLine("Unzipping build tools complete");
                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
                string PluginFolderfilepath = pathSplit[0] + "Plugins";
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

        public static IExtImplantService GetImpServicePlugin(string pluginName)
        {
            var svc_plugins = pluginHub.implant_servicePlugins;
            var svc_plugin = svc_plugins.GetPluginEnumerableResult(pluginName);
            return svc_plugin;
        }

        public static IExtimplantHandleComms GetImpCommsPlugin(string pluginName)
        {
            var comms_plugins = pluginHub.implant_commsPlugins;
            var comms_plugin = comms_plugins.GetPluginEnumerableResult(pluginName);
            return comms_plugin;
        }
        public static IExtImplant_TaskPreProcess GetImpPreProcPlugin(string pluginName)
        {
            var preProc_plugins = pluginHub.implant_preProcPlugins;
            var preProc_plugin = preProc_plugins.GetPluginEnumerableResult(pluginName);
            return preProc_plugin;
        }
        public static IExtImplant_TaskPostProcess GetImpPostProcPlugin(string pluginName)
        {
            var prePost_plugins = pluginHub.implant_postProcPlugins;
            var prePost_plugin = prePost_plugins.GetPluginEnumerableResult(pluginName);
            return prePost_plugin;
        }
        public static ExtImplant_Base GetImpPlugin(string pluginName)
        {
            var ModelPlugins = pluginHub.implant_ModelPlugins;
            var ModelPlugin = ModelPlugins.GetPluginEnumerableResult(pluginName);
            return (ExtImplant_Base)ModelPlugin;
        }

        public static IAssetNotificationService GetAssetNotifService(string pluginName)
        {
            var AssetNotifPlugins = pluginHub.Asset_NotificationServicePlugins;
            var AssetNotifPlugin = AssetNotifPlugins.GetPluginEnumerableResult(pluginName);
            return AssetNotifPlugin;
        }

        //public static T GetPluginImplementation<T>(string pluginName) where T : IPluginMetadata
        //{
        //    //Correct the property name based on the type T
        //    string propertyName = GetPropertyName<T>();
        //    var plugins = pluginHub.GetType().GetProperty(propertyName).GetValue(pluginHub) as IEnumerable<T>;
        //    var plugin = plugins.GetPluginEnumerableResult<T>(pluginName);
        //    return plugin;
        //}

        private static string GetPropertyName<T>() where T : IPluginMetadata
        {
            // Map the type T to the correct property name in PluginHub
            if (typeof(T) == typeof(IExtImplantService))
                return "implant_servicePlugins";
            if (typeof(T) == typeof(IExtimplantHandleComms))
                return "implant_commsPlugins";
            if (typeof(T) == typeof(IExtImplant_TaskPreProcess))
                return "implant_preProcPlugins";
            if (typeof(T) == typeof(IExtImplant_TaskPostProcess))
                return "implant_postProcPlugins";
            if (typeof(T) == typeof(IExtImplant))
                return "implant_ModelPlugins";
            if (typeof(T) == typeof(IAssetNotificationService))
                return "Asset_NotificationServicePlugins";


            throw new InvalidOperationException("Unknown plugin type.");
        }
    }
}
