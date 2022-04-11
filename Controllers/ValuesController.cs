using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;

namespace WebApplicationSupermercado.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public DataTable Get(string mitabla)
        {
            ConsultaMasiva obj = new ConsultaMasiva();
            string[] campos = new string[1];
            campos[0] = "*";
            DataTable tablaInfo;
           
            tablaInfo = obj.RealizarMasivo(mitabla, campos, new string[1], new string[1], new string[1], new string[1], new string[1],"","",false,"");

            return tablaInfo;
        }

        public DataTable GetRangoFecha(string fechauno, string fechados)
        {
            List<Array> vardata = new List<Array>();
            return HerramientasSQL.consultarDosSEG("CALL supermercado.consultar_promedio_venta_producto('" + fechauno + "','"+ fechados + "')", vardata); ;
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        public DataTable GetFecha(string fecha)
        {
            List<Array> vardata = new List<Array>();
            return HerramientasSQL.consultarDosSEG("CALL supermercado.consultar_por_fecha('" + fecha + "')", vardata); ;
        }
    }
}
