using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Universal_API_v7.Models;

namespace Universal_API_v7.Controllers
{
    [Route("local")]
    [ApiController]
    public class InsertSPController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public InsertSPController(IConfiguration config)
        {
            Configuration = config;
        }

        public static Object SafeDbObject(Object input)
        {
            if (input == null)
            {
                return DBNull.Value;
            }
            else
            {
                return input;
            }
        }

        string procedure_name = null;

        [HttpPost]
        [Route("InsertSP")]
        public ActionResult<IEnumerable<InsertSPReturn>> GetAllCategories([FromBody] InsertSP value)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("KLConnect_DB"))) //KLConnectDev
                {
                    var query = "UAPI_SP_INS_20211217";

                    SqlCommand getcommand = new SqlCommand(query, conn);
                    getcommand.CommandType = CommandType.StoredProcedure;
                    getcommand.Parameters.AddWithValue("@Module", SafeDbObject(value.Module));
                    getcommand.Parameters.AddWithValue("@Object", SafeDbObject(value.Object));
                    getcommand.Parameters.AddWithValue("@Func", SafeDbObject(value.Function));
                    getcommand.Parameters.AddWithValue("@Schema", SafeDbObject(value.Schema));
                    getcommand.Parameters.AddWithValue("@Conn", SafeDbObject(value.ConnectionString));

                    //List<InsertSPReturn> data = new List<InsertSPReturn>();

                    conn.Open();
                    //int result = insertcommand.ExecuteNonQuery();

                    //if (result > 0)
                    //{
                    //    return Content("1 SP Updated");
                    //}
                    //else
                    //{
                    //    return Content("No SP Updated");
                    //}

                    SqlDataReader dataReader = getcommand.ExecuteReader();

                    while (dataReader.Read())
                    {
                        //InsertSPReturn procedurename = new InsertSPReturn();
                        //procedurename.Procedure = dataReader["Procedure"] as string;
                        procedure_name = dataReader["Procedure"] as string;

                        //data.Add(procedurename);
                    }
                    //return data;

                }
                
                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString(value.ConnectionString)))
                {
                    var newquery = $"create procedure [{value.Schema}].[{procedure_name}] as";

                    SqlCommand insertcommand = new SqlCommand(newquery, conn);

                    conn.Open();
                    int result = insertcommand.ExecuteNonQuery();

                    if (result != 0)
                    {
                        return Content("1 New Procedure Created");
                    }
                    else
                    {
                        return Content("No New Procedure Created");
                    }
                }
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
