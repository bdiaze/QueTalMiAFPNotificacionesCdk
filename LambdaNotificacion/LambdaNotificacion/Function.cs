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
            services.AddSingleton<NotificacionBuilderHelper>();
            #endregion

            #region Singleton Repositories
            services.AddSingleton<NotificacionDAO>();
            services.AddSingleton<CuotaUfComisionDAO>();
            #endregion
        });

        var app = builder.Build();

        serviceProvider = app.Services;
    }

    public async Task FunctionHandler(EntradaLambda input, ILambdaContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - " +
            $"Se inicia proceso de envio de notificaciones.");

        VariableEntornoHelper variableEntorno = serviceProvider.GetRequiredService<VariableEntornoHelper>();
        HermesHelper hermesHelper = serviceProvider.GetRequiredService<HermesHelper>();
        NotificacionBuilderHelper notificacionBuilder = serviceProvider.GetRequiredService<NotificacionBuilderHelper>();
        NotificacionDAO notificacionDAO = serviceProvider.GetRequiredService<NotificacionDAO>();

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se obtendran los parametros necesarios para el envio de notificaciones.");

        string appName = variableEntorno.Obtener("APP_NAME");

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se obtendran las notificaciones a procesar.");
        List<Notificacion> notificaciones = await notificacionDAO.ObtenerNotificacionesPorTipo(input.IdTipoNotificacion);

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Hay que procesar {notificaciones.Count} notificaciones, se comienza a armar el asunto y cuerpo del correo.");

        InformacionNotificacion info = await notificacionBuilder.GenerarInformacion(input.IdTipoNotificacion);

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se construye exitosamente el asunto y cuerpo de la notificaci�n, iniciando con los env�os.");

        int casosError = 0;
        foreach (Notificacion notificacion in notificaciones) {
            try {
                await hermesHelper.EnviarCorreo(new HermesCorreo {
                    Para = [ 
                        new DireccionCorreo { 
                            Correo = notificacion.CorreoNotificacion
                        } 
                    ],
                    Asunto = info.Asunto,
                    Cuerpo = info.Cuerpo
                });
            } catch(Exception ex) {
                casosError++;
                LambdaLogger.Log(LogLevel.Error, $"Ocurri� un error al procesar la notificaci�n ID {notificacion.Id}. {ex}");
            }
        }

        LambdaLogger.Log(
            $"[Function] - [FunctionHandler] - [{stopwatch.ElapsedMilliseconds} ms] - " +
            $"Se terminan de encolar exitosamente las notificaciones. Casos con error: {casosError}.");
    }
}
