using Amazon.Lambda.Core;
using LambdaNotificacion.Models;
using LambdaNotificacion.Repositories;
using LambdaNotificacion.TemplateModels;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.NotificacionBuilders {
    internal class ResumenSemanalBuilder(IHostEnvironment env, CuotaUfComisionDAO cuotaUfComisionDAO) : INotificacionBuilder {
        private const string AFP = "CAPITAL,CUPRUM,HABITAT,MODELO,PLANVITAL,PROVIDA,UNO";
        private const string FONDO = "A,B,C,D,E";

        public async Task<InformacionNotificacion> ObtenerInformacionNotificacion() {
            string strTemplate;
            if (env.IsDevelopment()) {
                strTemplate = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ResumenSemanal.html"));
            } else {
                strTemplate = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Templates", "ResumenSemanal.html"));
            }
            CultureInfo cultureInfo = new("es-CL");
            Template template = Template.Parse(strTemplate);
            TemplateContext context = new();
            context.PushCulture(cultureInfo);

            ResumenSemanalModel model = new() {
                FechaHasta = (await cuotaUfComisionDAO.UltimaFechaTodas()).ToDateTime(TimeOnly.MinValue)
            };
            // Se modifica FechaHasta para que coincida con un domingo...
            model.FechaHasta = model.FechaHasta.Value.AddDays(-1 * (short)model.FechaHasta.Value.DayOfWeek);

            model.FechaDesde = model.FechaHasta.Value.AddDays(-6);

            List<SalObtenerUltimaCuota> valoresCuotaIniciales = await cuotaUfComisionDAO.ObtenerUltimaCuota(
                AFP,
                FONDO,
                model.FechaDesde.Value.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                1
            );
            List<SalObtenerUltimaCuota> valoresCuotaFinales = await cuotaUfComisionDAO.ObtenerUltimaCuota(
                AFP,
                FONDO,
                model.FechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                1
            );

            foreach (string fondo in model.RentabilidadSemanal.Keys) {
                List<decimal> rentabilidades = [];
                foreach (string afp in AFP.Split(',')) {
                    decimal valorCuotaInicial = valoresCuotaIniciales.FirstOrDefault(c => c.Afp == afp && c.Fondo == fondo)!.Valor;
                    decimal valorCuotaFinal = valoresCuotaFinales.FirstOrDefault(c => c.Afp == afp && c.Fondo == fondo)!.Valor;
                    decimal rentabilidad = 100 * (valorCuotaFinal - valorCuotaInicial) / valorCuotaInicial;
                    
                    if (model.PremioRentabilidad["Rentabilidad"] == null || (decimal)model.PremioRentabilidad["Rentabilidad"]! < rentabilidad) {
                        model.PremioRentabilidad["AFP"] = afp;
                        model.PremioRentabilidad["Fondo"] = fondo;
                        model.PremioRentabilidad["Rentabilidad"] = rentabilidad;
                    }

                    rentabilidades.Add(rentabilidad);
                }

                model.RentabilidadSemanal[fondo] = rentabilidades.Sum() / rentabilidades.Count;
            }

            foreach (string afp in model.ValorCuotaFinal.Keys.ToArray()) {
                foreach (string fondo in model.ValorCuotaFinal[afp].Keys.ToArray()) {
                    model.ValorCuotaFinal[afp][fondo] = valoresCuotaFinales.FirstOrDefault(c => c.Afp == afp && c.Fondo == fondo)?.Valor;
                }
            }

            ScriptObject scriptObject = [];
            scriptObject.Import(model);
            context.PushGlobal(scriptObject);

            return new InformacionNotificacion() { 
                Asunto = $"¡Ha llegado tu resumen semanal! - Semana del {model.FechaDesde.Value.ToString("d 'de' MMMM", cultureInfo)}.",
                Cuerpo = template.Render(context),
            };
        }
    }
}
