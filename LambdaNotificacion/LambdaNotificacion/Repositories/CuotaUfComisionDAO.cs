using LambdaNotificacion.Helpers;
using LambdaNotificacion.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Repositories {
    internal class CuotaUfComisionDAO(VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _baseUrl = parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_API_URL")).Result;
        private readonly string _xApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_API_KEY_ID")).Result).Result;

        public async Task<DateOnly> UltimaFechaTodas() {
            using HttpClient client = new(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.GetAsync(_baseUrl + "CuotaUfComision/UltimaFechaTodas");
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener la fecha donde se tiene valor cuota para todas las AFP. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return DateOnly.Parse((await response.Content.ReadAsStringAsync()).Replace("\"", ""), CultureInfo.InvariantCulture);
        }

        public async Task<List<SalObtenerUltimaCuota>> ObtenerUltimaCuota(string listaAFPs, string listaFondos, string listaFechas, int tipoComision) {
            EntObtenerUltimaCuota entradaSanitizada = new() {
                ListaAFPs = WebUtility.HtmlEncode(listaAFPs),
                ListaFondos = WebUtility.HtmlEncode(listaFondos),
                ListaFechas = WebUtility.HtmlEncode(listaFechas),
                TipoComision = tipoComision
            };

            using HttpClient client = new(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Add("x-api-key", _xApiKey);
            HttpResponseMessage response = await client.PostAsync(_baseUrl + "CuotaUfComision/ObtenerUltimaCuota", new StringContent(JsonConvert.SerializeObject(entradaSanitizada), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Ocurrió un error al obtener los últimos valores cuotas - ListaAFPs: {listaAFPs} - ListaFondos: {listaFondos} - ListaFechas: {listaFechas} - TipoComision: {tipoComision}. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonConvert.DeserializeObject<List<SalObtenerUltimaCuota>>(await response.Content.ReadAsStringAsync())!;
        }
    }
}
