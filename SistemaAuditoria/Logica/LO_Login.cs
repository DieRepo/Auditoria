using MySql.Data.MySqlClient;
using SistemaAuditoria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaAuditoria.Logica
{
    public class LO_Login
    {
        public Usuario CheckLogin(string correo, string password)
        {
            try
            {


                Usuario ReturnUsuario = new Usuario();
                MySqlConnection con = new MySqlConnection();
                con = new MySqlConnection(System.Configuration.ConfigurationManager.AppSettings["4"]);
                con.Open();
                byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(password);
                string passwordEnco = Convert.ToBase64String(encbuff);
                using (con)
                {
                    string sql = "";
                    sql = string.Format("select * from tbl_usuarios where correo = '{0}' and contraseña = '{1}' and activo = 1; ", correo, passwordEnco);
                    MySqlCommand _comando = new MySqlCommand(sql, con);
                    MySqlDataReader _reader = _comando.ExecuteReader();
                    if (_reader.Read())
                    {
                        ReturnUsuario.Nombres = (_reader[1].ToString());
                        ReturnUsuario.ApellidoPaterno = (_reader[2].ToString());
                        ReturnUsuario.ApellidoPaterno = (_reader[3].ToString());
                        ReturnUsuario.Correo = (_reader[4].ToString());
                        ReturnUsuario.Contraseña = System.Text.Encoding.UTF8.GetString((Convert.FromBase64String((_reader[5].ToString()))));
                        ReturnUsuario.idRol = ((Rol)_reader[6]);
                    }
                }
                return ReturnUsuario;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al buscar usuario" + ex.Message);
            }
        }
    }
}