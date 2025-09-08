using Microsoft.AspNetCore.WebUtilities;
using LambdaNotificacion.Entities;
using LambdaNotificacion.Helpers;
using LambdaNotificacion.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string requestUri = QueryHelpers.AddQueryString(_baseUrl + "Correo/Enviar", parameters);

            using HttpClient client = new(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.GetAsync(requestUri);
            return JsonConvert.DeserializeObject<List<Notificacion>>(await response.Content.ReadAsStringAsync())!;
        }
        
    }
}
