using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_Management
{
    //should make it so that plugins can be reloaded at runtime
    class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext() : base(isCollectible: true)
        {
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }

    public class PluginService
    {
        public static CompositionContainer _container;
        public static AggregateCatalog catalog = new AggregateCatalog();
        public static DirectoryCatalog directoryCatalog; // Reference to DirectoryCatalog
        public static IPluginHub pluginHub;


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
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
        }

        public static void RefreshPlugins()
        {
            try
            {
                // Dispose of the existing container
                if (_container != null)
                {
                    _container.Dispose();
                }

                // Recreate the DirectoryCatalog to ensure it picks up new changes
                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] pathSplit = assemblyBasePath.Split("bin");
                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
                string PluginFolderfilepath = pathSplit[0] + "Plugins";

                directoryCatalog = new DirectoryCatalog(PluginFolderfilepath, "*.dll");

                // Recreate the AggregateCatalog
                catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
                catalog.Catalogs.Add(directoryCatalog);

                // Create a new CompositionContainer
                _container = new CompositionContainer(catalog);

                // Get the exported value again
                pluginHub = _container.GetExportedValue<IPluginHub>();
                Console.WriteLine("Plugins Refreshed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error refreshing plugins: " + e.Message);
            }
        }

        public static void InitPlugins()
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

                catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
                directoryCatalog = new DirectoryCatalog(PluginFolderfilepath, "*.dll");
                catalog.Catalogs.Add(directoryCatalog);
                _container = new CompositionContainer(catalog);
                pluginHub = _container.GetExportedValue<IPluginHub>();
                Console.WriteLine("Plugins loaded");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static IExtImplantService GetImpServicePlugin(string pluginName)
        {
            var svc_plugins = pluginHub.implant_servicePlugins;
            var svc_plugin = svc_plugins.GetPluginEnumerableResult(pluginName);
            IExtImplantService extImplantService_Base = svc_plugin.Value;
            return extImplantService_Base;
        }

        public static IExtimplantHandleComms GetImpCommsPlugin(string pluginName)
        {
            var comms_plugins = pluginHub.implant_commsPlugins;
            var comms_plugin = comms_plugins.GetPluginEnumerableResult(pluginName);
            IExtimplantHandleComms extImplantComms_Base = comms_plugin.Value;
            return extImplantComms_Base;
        }
        public static IExtImplant_TaskPreProcess GetImpPreProcPlugin(string pluginName)
        {
            var preProc_plugins = pluginHub.implant_preProcPlugins;
            var preProc_plugin = preProc_plugins.GetPluginEnumerableResult(pluginName);
            IExtImplant_TaskPreProcess extImplantprePorc_Base = preProc_plugin.Value;
            return extImplantprePorc_Base;
        }
        public static IExtImplant_TaskPostProcess GetImpPostProcPlugin(string pluginName)
        {
            var prePost_plugins = pluginHub.implant_postProcPlugins;
            var prePost_plugin = prePost_plugins.GetPluginEnumerableResult(pluginName);
            IExtImplant_TaskPostProcess extImplantPostProc_Base = prePost_plugin.Value;
            return extImplantPostProc_Base;
        }
        public static ExtImplant_Base GetImpPlugin(string pluginName)
        {
            var ModelPlugins = pluginHub.implant_ModelPlugins;
            var ModelPlugin = ModelPlugins.GetPluginEnumerableResult(pluginName);
            ExtImplant_Base ModelPlugins_Base = ModelPlugin.Value;
            return ModelPlugins_Base;
        }
    }
}
