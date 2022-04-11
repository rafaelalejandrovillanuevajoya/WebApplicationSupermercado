using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;

namespace WebApplicationSupermercado
{
    public class Conexion
    {
        private string servidor;
        private string puerto;
        private string usuario;
        private string contrasena;
        private string baseDados;
        private MySqlConnection conexionBD;

        public Conexion()
        {
            servidor = ConfigurationManager.AppSettings["servidor"]; 
            puerto = ConfigurationManager.AppSettings["puerto"];
            usuario = ConfigurationManager.AppSettings["usuario"];
            contrasena = ConfigurationManager.AppSettings["contrasena"];
            baseDados = ConfigurationManager.AppSettings["baseDados"];
        }

        public MySqlConnection ConexionBD { get => conexionBD; }
        public string Servidor { get => servidor; }
        public string Puerto { get => puerto; }
        public string Usuario { get => usuario; }
        public string Contrasena { get => contrasena; }
        public string BaseDados { get => baseDados; }

        public bool ConectarBaseDatos()
        {
            string cadenaConexion = "server=" + servidor + "; port=" + puerto + "; userid=" + usuario + "; password=" + contrasena + "; database=" + baseDados + ";";

            try
            {
                conexionBD = new MySqlConnection(cadenaConexion);
                conexionBD.Open();
            }
            catch (MySqlException ex)
            {
                return false;
            }
            return true;
        }
    }

    public class ConsultaMasiva 
    {//obj consultaTabla
        public DataTable RealizarMasivo(string tabla, string[] campos, string[] campoCondiciones, string[] operadorCondiciones, string[] valorCondiciones, string[] conectores, string[] condicionOrden, string limiteA, string limiteB, bool validarStatus, string status)
        {
            ///Los parametros:  campoCondiciones, operadorCondiciones, valorCondiciones, conectores: deben tener las mismas cantidades de objetos.
            ///conectores: AND o OR
            ///condicionOrden : campo + ASC o DESC
            ///filtra status "status" si "validarStatus" = true

            List<Array> vardata = new List<Array>();
            string[] condiciones = new string[1];

            if (!(operadorCondiciones.Length == 1 && operadorCondiciones[0] == null))//verificar operadores
            {
                for (int c = 0; c < operadorCondiciones.Length; c++)
                {
                    if (!(operadorCondiciones[c] == " = " || operadorCondiciones[c] == " != " || operadorCondiciones[c] == " < " || operadorCondiciones[c] == " <= " || operadorCondiciones[c] == " > " || operadorCondiciones[c] == " >= "))
                    {
                        operadorCondiciones = new string[1];
                        break;
                    }
                }
            }

            if (!((campoCondiciones.Length == 1 && campoCondiciones[0] == null) || (operadorCondiciones.Length == 1 && operadorCondiciones[0] == null) || (valorCondiciones.Length == 1 && valorCondiciones[0] == null) || (conectores.Length == 1 && conectores[0] == null)))
            {
                if (campoCondiciones.Length == operadorCondiciones.Length && campoCondiciones.Length == valorCondiciones.Length && campoCondiciones.Length == conectores.Length)
                {
                    condiciones = new string[campoCondiciones.Length];

                    for (int a = 0; a < campoCondiciones.Length; a++)
                    {
                        string[] registroValor = new string[2];
                        registroValor[0] = "@vardata" + (a + 1).ToString();
                        registroValor[1] = valorCondiciones[a];
                        vardata.Add(registroValor);

                        condiciones[a] = campoCondiciones[a] + operadorCondiciones[a] + "@vardata" + (a + 1).ToString();
                    }
                }
                else
                {
                    conectores = new string[1];
                }
            }
            else
            {
                conectores = new string[1];
            }

            if (!(campos.Length == 1 && campos[0] == "*"))
            {
                if (validarStatus)
                {
                    //consultar campos de tabla
                    //compararlos y agregar la condicion, conector
                    for (int a = 0; a < campos.Length; a++)
                    {
                        string[] registroValor = new string[2];
                        if (campos[a] == "id_estado_registro")
                        {
                            registroValor[0] = "@vardatas1";
                            registroValor[1] = status;
                            vardata.Add(registroValor);
                            //condiciones[0] = "id_estado_registro = @vardatas1";
                            condiciones = agregarElemento(condiciones, "id_estado_registro = @vardatas1");
                            conectores = agregarElemento(conectores, "AND");
                            break;
                        }
                        if (campos[a] == "id_estado_documento")
                        {
                            registroValor[0] = "@vardatas1";
                            registroValor[1] = status;
                            vardata.Add(registroValor);
                            //condiciones[0] = "id_estado_documento = @vardatas1";
                            condiciones = agregarElemento(condiciones, "id_estado_documento = @vardatas1");
                            conectores = agregarElemento(conectores, "AND");
                            break;
                        }
                    }
                }
            }

            string sentencia = HerramientasSQL.generarSQLConsulta(campos, tabla, condiciones, conectores, condicionOrden, limiteA, limiteB);
            return HerramientasSQL.consultarDosSEG(sentencia, vardata);
        }

        static public string[] agregarElemento(string[] cadena, string elemento)
        {
            string[] salida;
            if (cadena.Length == 1 && cadena[0] == null)
            {
                salida = new string[1];
                salida[0] = elemento;
            }
            else
            {
                salida = new string[cadena.Length + 1];
                for (int a = 0; a < cadena.Length; a++)
                {
                    salida[a] = cadena[a];
                }
                salida[cadena.Length] = elemento;
            }
            return salida;
        }
    }

    public class HerramientasSQL
    {
        static public string generarSQLConsulta(string[] campos, string tabla, string[] condiciones, string[] conectores, string[] condicionOrden, string limiteA, string limiteB)
        {
            string sqlselect = "SELECT ";

            for (int a = 0; a < campos.Length; a++)
            {
                sqlselect += campos[a];
                if (a + 1 < campos.Length)
                {
                    sqlselect += ", ";
                }
            }

            string sqlFrom = " FROM " + tabla;

            string sqlWhere = " WHERE ";

            if (!(condiciones.Length == 1 && condiciones[0] == null))
            {
                ////////////////////////////////////////////
                for (int a = 0; a < condiciones.Length; a++)
                {
                    sqlWhere += condiciones[a];
                    if (a + 1 < condiciones.Length)
                    {
                        sqlWhere += " " + conectores[a] + " ";
                    }
                }
                ////////////////////////////////////////////
            }
            else
            {
                sqlWhere = "";
            }

            string sqlOrderBy = " ORDER BY ";

            if (!(condicionOrden.Length == 1 && condicionOrden[0] == null))
            {
                ////////////////////////////////////////////
                for (int a = 0; a < condicionOrden.Length; a++)
                {
                    sqlOrderBy += condicionOrden[a];
                    if (a + 1 < condicionOrden.Length)
                    {
                        sqlOrderBy += ", ";
                    }
                }
                ////////////////////////////////////////////
            }
            else
            {
                sqlOrderBy = "";
            }

            string sqlLimit = " LIMIT ";

            if (limiteA != "" && limiteB != "" && verificacionNumero(limiteA) && verificacionNumero(limiteB))
            {
                sqlLimit += limiteA + "," + limiteB;
            }
            else
            {
                sqlLimit = "";
            }

            return sqlselect + sqlFrom + sqlWhere + sqlOrderBy + sqlLimit;
        }

        static public DataTable consultarDosSEG(string sql, List<Array> vardata)
        {
            Conexion con = new Conexion();
            bool resul = con.ConectarBaseDatos();
            DataTable tabla = new DataTable();

            if (resul == true)
            {
                try
                {
                    MySqlCommand comando = new MySqlCommand(sql, con.ConexionBD);
                    foreach (string[] pares in vardata)
                    {
                        comando.Parameters.Add(new MySqlParameter(pares[0], pares[1]));
                    }
                    MySqlDataAdapter Adaptador = new MySqlDataAdapter(comando);

                    DataSet DS = new DataSet();
                    Adaptador.Fill(DS);
                    tabla = DS.Tables[0];
                }
                catch (MySqlException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.ConexionBD.Close();
                }
            }
            else
            {
                //MessageBox.Show("Conexión no establecida.");
            }
            return tabla;
        }

        static public bool verificacionNumero(string dato)
        {
            bool resultado = true;

            if (dato == "")
            {
                return false;
            }

            foreach (char r1 in dato)
            {
                if (!(char.IsDigit(r1)))
                {
                    resultado = false;
                    break;
                }
            }

            return resultado;
        }
    }
}