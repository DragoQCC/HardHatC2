using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_Management
{
    public class PluginService
    {
        public static CompositionContainer _container;
        public static AggregateCatalog catalog = new AggregateCatalog();
        public static DirectoryCatalog directoryCatalog; // Reference to DirectoryCatalog
        public static IPluginHub pluginHub;


        public static void RefreshPlugins()
        {
            _container.Dispose();
            directoryCatalog.Refresh();
            _container = new CompositionContainer(catalog);
            pluginHub = _container.GetExportedValue<IPluginHub>();
            Console.WriteLine("Plugins Refreshed");
        }

        public static void InitPlugins()
        {
            try
            {
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
