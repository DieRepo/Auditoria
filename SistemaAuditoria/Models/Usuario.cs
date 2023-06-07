using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaAuditoria.Models
{
    public class Usuario
    {
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }

        public string Correo { get; set; }
        public string Contraseña { get; set; } 
        public Rol idRol { get; set; }

    }
}