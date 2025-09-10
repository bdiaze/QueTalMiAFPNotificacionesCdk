﻿using LambdaNotificacion.Enums;
using LambdaNotificacion.Models;
using LambdaNotificacion.NotificacionBuilders;
using LambdaNotificacion.Repositories;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaNotificacion.Helpers {
    internal class NotificacionBuilderHelper(IHostEnvironment env, CuotaUfComisionDAO cuotaUfComisionDAO) {
        private readonly Dictionary<TipoNotificacion, INotificacionBuilder> _notificacionBuilders = [];

        public async Task<InformacionNotificacion> GenerarInformacion(short idTipoNotificacion) {
            TipoNotificacion tipoNotificacion = (TipoNotificacion)idTipoNotificacion;

            if (!_notificacionBuilders.TryGetValue(tipoNotificacion, out INotificacionBuilder? builder)) {
                builder = tipoNotificacion switch {
                    TipoNotificacion.ResumenSemanal => new ResumenSemanalBuilder(env, cuotaUfComisionDAO),
                    _ => throw new NotSupportedException($"No se tiene un builder para el tipo de notificación {idTipoNotificacion}."),
                };
                _notificacionBuilders.Add(tipoNotificacion, builder);
            }
            
            return await builder.ObtenerInformacionNotificacion();
        }
    }
}
