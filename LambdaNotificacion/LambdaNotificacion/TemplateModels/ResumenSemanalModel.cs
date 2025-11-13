using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.TemplateModels {
    public class ResumenSemanalModel {
        public Dictionary<string, string> UrlImagenes { get; } = new Dictionary<string, string> {
            { "CAPITAL", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPCapital.svg" },
            { "CUPRUM", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPCuprum.svg" },
            { "HABITAT", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPHabitat.svg" },
            { "MODELO", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPModelo.svg" },
            { "PLANVITAL", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPPlanvital.svg" },
            { "PROVIDA", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPProvida.svg" },
            { "UNO", "https://www.quetalmiafp.cl/images/logos_afps/LogoAFPUno.png" },
        };

        public DateTime? FechaDesde { get; set; }
        
        public DateTime? FechaHasta { get; set; }

        public Dictionary<string, object?> PremioRentabilidad { get; set; } = new Dictionary<string, object?> {
            { "AFP", null },
            { "Fondo", null },
            { "Rentabilidad", null },
        };

        public Dictionary<string, decimal?> RentabilidadSemanal { get; set; } = new Dictionary<string, decimal?> {
            { "A", null },
            { "B", null },
            { "C", null },
            { "D", null },
            { "E", null },
        };

        public SortedDictionary<string, SortedDictionary<string, decimal?>> ValorCuotaFinal { get; set; } = new SortedDictionary<string, SortedDictionary<string, decimal?>> {
            { "CAPITAL", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "CUPRUM", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "HABITAT", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "MODELO", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "PLANVITAL", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "PROVIDA", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
            { "UNO", new SortedDictionary<string, decimal?> {
                { "A",  null },
                { "B",  null },
                { "C",  null },
                { "D",  null },
                { "E",  null }
            } },
        };
    }
}
