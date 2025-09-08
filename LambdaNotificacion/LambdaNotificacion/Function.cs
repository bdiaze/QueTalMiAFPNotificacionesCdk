using Amazon.APIGateway;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using LambdaNotificacion.Entities;
using LambdaNotificacion.Helpers;
using LambdaNotificacion.Models;
using LambdaNotificacion.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaNotificacion;

public class Function
{
    private readonly IServiceProvider serviceProvider;

    public Function() {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((context, services) => {
            #region Singleton AWS Services
            services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
            services.AddSingleton<IAmazonAPIGateway, AmazonAPIGatewayClient>();
            #endregion

            #region Singleton Helpers
            services.AddSingleton<VariableEntornoHelper>();
            services.AddSingleton<ParameterStoreHelper>();
            services.AddSingleton<ApiKeyHelper>();
            services.AddSingleton<HermesHelper>();
            #endregion

            #region Singleton Repositories
            services.AddSingleton<NotificacionDAO>();
            #endregion
        });

        var app = builder.Build();

        serviceProvider = app.Services;
    }

    public async Task FunctionHandler(JObject input, ILambdaContext context)
    {
        if (input["IdTipoNotificacion"] == null) {
            throw new Exception("No se recibió el ID del tipo de notificación que se debe procesar.");
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - " +
            $"Se inicia proceso de envío de notificaciones.");

        VariableEntornoHelper variableEntorno = serviceProvider.GetRequiredService<VariableEntornoHelper>();
        // ParameterStoreHelper parameterStore = serviceProvider.GetRequiredService<ParameterStoreHelper>();
        HermesHelper hermesHelper = serviceProvider.GetRequiredService<HermesHelper>();
        NotificacionDAO notificacionDAO = serviceProvider.GetRequiredService<NotificacionDAO>();

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se obtendran los parametros necesarios para el envío de notificaciones.");

        string appName = variableEntorno.Obtener("APP_NAME");

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se obtendran las notificaciones a procesar.");
        List<Notificacion> notificaciones = await notificacionDAO.ObtenerNotificacionesPorTipo(input.Value<short>("IdTipoNotificacion"));

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Comenzando a procesar las {notificaciones.Count} notificaciones.");

        foreach (Notificacion notificacion in notificaciones) {
            try {

                await hermesHelper.EnviarCorreo(new HermesCorreo {
                    Para = [ 
                        new DireccionCorreo { 
                            Correo = notificacion.CorreoNotificacion
                        } 
                    ],
                    Asunto = $"Asunto de ejemplo desde notificaciones {appName}",
                    Cuerpo = "Cuerpo de ejemplo"
                });
            } catch(Exception ex) {
                LambdaLogger.Log(LogLevel.Error, $"Ocurrió un error al procesar la notificación ID {notificacion.Id}. {ex}");
            }
        }
    }
}
