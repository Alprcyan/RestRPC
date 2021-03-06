﻿using CommandLine;
using System;
using System.IO;
using System.Threading;
using RestRPC.Framework;
using RestRPC.Framework.Plugins;
using RestRPC.Service.Plugins;

namespace RestRPC.Service
{
    class Program
    {
        static RestRPCComponent wshComponent;

        static void Main(string[] args)
        {
            string componentName = Guid.NewGuid().ToString();
            Uri remoteUri;

            var options = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (!string.IsNullOrWhiteSpace(options.Name)) componentName = options.Name;
                remoteUri = new Uri(options.ServerUriString);
            }
            else
            {
                return;
            }

            wshComponent = new RestRPCComponent(componentName, remoteUri, TimeSpan.FromMilliseconds(30), 
                options.Username, options.Password, Console.Out, LogType.All);
            // Register custom plugins
            wshComponent.PluginManager.RegisterPlugin(new PrintToScreen(), "print");
            // Load plugins in plugins directory if dir exists
            if (Directory.Exists("plugins"))
            {
                var plugins = PluginLoader.LoadAllPluginsFromDir("plugins", "*.dll");
                foreach (var plug in plugins)
                {
                    wshComponent.PluginManager.RegisterPlugin(plug, plug.GetType().FullName);
                }
            }

            // Start WSH component
            wshComponent.Start();

            // TODO: Use a timer instead of Sleep
            while (true)
            {
                wshComponent.Update();
                Thread.Sleep(100);
            }
        }
    }
}
