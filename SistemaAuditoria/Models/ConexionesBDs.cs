using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaAuditoria.Models
{
    public class ConexionesBDs
    {
        public static MySqlConnection ObtenerConexion(int tipo)
        {
            try
            {
                //< add key = "dba" value = "Data Source=127.0.0.1; database=indicadores_pjem_laboral; Port=3306; User ID=root;Password=;" />
                   //Tipo 1 = PENAL *htsj_sigejupe  -  Tipo 2 = die_equivalencias_catalogos -  Tipo 3 = estadistica - Tipo 4 = htsj_laboral
                   MySqlConnection conectar = new MySqlConnection();
                if (tipo == 1)
                {
                    conectar = new MySqlConnection("server = 10.22.157.98; database =htsj_sigejupe; Uid =estadistica; pwd =3Stad1stiC4_2021;");
                    conectar.Open();
                }
                else if (tipo == 2)
                {
                    conectar = new MySqlConnection("server = 127.0.0.1; Port=3306; database =die_equivalencias_catalogos; Uid =root; pwd =;");
                    conectar.Open();
                }
                else if (tipo == 3)
                {
                    conectar = new MySqlConnection("server = 10.22.157.67; database =estadistica; Uid =estadistica; pwd =3Stad1stiC4_2021;");
                    conectar.Open();
                }
                else if (tipo == 4)
                {
                    conectar = new MySqlConnection("server = 10.22.157.67; database =htsj_laboral; Uid =estadistica; pwd =3Stad1stiC4_2021;");
                    conectar.Open();
                }
                else
                    throw new Exception("Error ID fuera de rango");

                return conectar;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}