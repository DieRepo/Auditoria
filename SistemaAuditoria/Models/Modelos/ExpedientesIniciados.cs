using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaAuditoria.Models.Modelos
{
    public class ExpedientesIniciados
    {
        public bool isRadicado { get; set; }
        public string Materia { get; set; }
        public string Expediente { get; set; }
        public string Juzgado { get; set; }
        public Nullable<System.DateTime> fechaRadicacion { get; set; }
        public Nullable<System.DateTime> fechaTermino { get; set; }

    }
}