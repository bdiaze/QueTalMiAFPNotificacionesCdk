using LambdaNotificacion.Entities;
using LambdaNotificacion.Helpers;
using LambdaNotificacion.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Repositories {
    internal class NotificacionDAO(VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _baseUrl = parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_API_URL")).Result;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_API_KEY_ID")).Result).Result;
                
        public async Task<List<Notificacion>> ObtenerNotificacionesPorTipo(short idTipoNotificacion) {
            Dictionary<string, string?> parameters = new() {
                { "idTipoNotificacion",  idTipoNotificacion.ToString() },
                { "habilitado", "1" }
            };
            string requestUri = QueryHelpers.AddQueryString(_baseUrl + "Notificacion/ObtenerPorTipoNotificacion", parameters);

            using HttpClient client = new(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener las notificaciones por tipo. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<Notificacion>>(await response.Content.ReadAsStringAsync())!;
        }
        
        public async Task<HistorialNotificacion> IngresarHistorialNotificacion(EntIngresarHistorialNotificacion entrada) {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);

            HttpResponseMessage response = await client.PostAsync(_baseUrl + "HistorialNotificacion/Ingresar", new StringContent(JsonConvert.SerializeObject(entrada), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al ingresar el historial de notificación. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<HistorialNotificacion>(await response.Content.ReadAsStringAsync())!;
        }
    }
}
