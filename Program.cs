using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

class Program
{

    enum LogFormat {
        AspNetJson,
        Generic
    }

    static Task Main(string[] args) {
        // Create a root command with some options
        var rootCommand = new RootCommand
        {
            new Option<LogFormat>(
                "--format",
                description: "The format to be used"),
            new Option<int>(
                "--rateSeconds",
                description: "The number of seconds to output the log entries")
        };

        rootCommand.Description = "wd-logapp";

        // Note that the parameters of the handler method are matched according to the names of the options
        rootCommand.Handler = CommandHandler.Create<LogFormat, int>((format, rate) =>
            {
                switch(format)             {
                    case LogFormat.AspNetJson: return StartAspNet(args);
                    case LogFormat.Generic: return StartGeneric();
                    default: throw new NotSupportedException();
                }
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args);
        }

    static Task StartAspNet(string[] args) => 
        CreateHostBuilder(args).Build().RunAsync();

    static async Task StartGeneric() {
        for(;;){
            Console.WriteLine("This is a stdout message");
            Console.Error.WriteLine("This is a stderr message");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    static bool RunsOnKubernetes => !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));

    static IHostBuilder CreateHostBuilder(string[] args) =>

        Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
        .ConfigureServices(services => services.AddHostedService<BackgroundLogger>())
        .ConfigureLogging(builder =>{
            if (RunsOnKubernetes)
            {
                builder.AddJsonConsole(c => { c.IncludeScopes = true; });
            }
            else
            {
                builder.AddConsole();
            }
        });
}
