using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using TeamServer.Plugin_BaseClasses;
using TeamServer.Utilities;

namespace TeamServer.Plugin_Management
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
        }

        public static void InitPlugins()
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            string assemblyBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] pathSplit = assemblyBasePath.Split("bin"); // [0] is the main path HardHatC2\Teamserver\ 
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            string  PluginFolderfilepath = pathSplit[0] + "Plugins";

            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            directoryCatalog = new DirectoryCatalog(PluginFolderfilepath, "*.dll");
            catalog.Catalogs.Add(directoryCatalog);
            _container = new CompositionContainer(catalog);
            pluginHub = _container.GetExportedValue<IPluginHub>();
        }

        public static ExtImplantService_Base GetImpServicePlugin(string pluginName)
        {
            var svc_plugins = pluginHub.implant_servicePlugins;
            var svc_plugin = svc_plugins.GetPluginEnumerableResult(pluginName);
            ExtImplantService_Base extImplantService_Base = svc_plugin.Value;
            return extImplantService_Base;
        }

        public static ExtImplantHandleComms_Base GetImpCommsPlugin(string pluginName)
        {
            var comms_plugins = pluginHub.implant_commsPlugins;
            var comms_plugin = comms_plugins.GetPluginEnumerableResult(pluginName);
            ExtImplantHandleComms_Base extImplantComms_Base = comms_plugin.Value;
            return extImplantComms_Base;
        }
        public static ExtImplant_TaskPreProcess_Base GetImpPreProcPlugin(string pluginName)
        {
            var preProc_plugins = pluginHub.implant_preProcPlugins;
            var preProc_plugin = preProc_plugins.GetPluginEnumerableResult(pluginName);
            ExtImplant_TaskPreProcess_Base extImplantprePorc_Base = preProc_plugin.Value;
            return extImplantprePorc_Base;
        }
        public static ExtImplant_TaskPostProcess_Base GetImpPostProcPlugin(string pluginName)
        {
            var prePost_plugins = pluginHub.implant_postProcPlugins;
            var prePost_plugin = prePost_plugins.GetPluginEnumerableResult(pluginName);
            ExtImplant_TaskPostProcess_Base extImplantPostProc_Base = prePost_plugin.Value;
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
