using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Universal_API_v7.Models;

namespace Universal_API_v7.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    
    public class UniversalAPIController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        public UniversalAPIController(IConfiguration config)
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

        public static SqlDbType GetDataType(string input)
        {
            switch (input)
            {
                case "bigint":
                    return SqlDbType.BigInt;
                case "binary":
                    return SqlDbType.Binary;
                case "bit":
                    return SqlDbType.Bit;
                case "char":
                    return SqlDbType.Char;
                case "date":
                    return SqlDbType.Date;
                case "datetime":
                    return SqlDbType.DateTime;
                case "datetime2":
                    return SqlDbType.DateTime2;
                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;
                case "decimal":
                    return SqlDbType.Decimal;
                case "float":
                    return SqlDbType.Float;
                case "image":
                    return SqlDbType.Image;
                case "int":
                    return SqlDbType.Int;
                case "money":
                    return SqlDbType.Money;
                case "nchar":
                    return SqlDbType.NChar;
                case "ntext":
                    return SqlDbType.NText;
                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "real":
                    return SqlDbType.Real;
                case "smalldatetime":
                    return SqlDbType.SmallDateTime;
                case "smallint":
                    return SqlDbType.SmallInt;
                case "smallmoney":
                    return SqlDbType.SmallMoney;
                case "text":
                    return SqlDbType.Text;
                case "time":
                    return SqlDbType.Time;
                case "tinyint":
                    return SqlDbType.TinyInt;
                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;
                case "varbinary":
                    return SqlDbType.VarBinary;
                case "varchar":
                    return SqlDbType.VarChar;
                case "xml":
                    return SqlDbType.Xml;
                default:
                    return SqlDbType.VarChar;
            }
        }
        SqlCommand apiprocedure;
        //[HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<UniversalAPI>> Get([FromBody] JSONString passedstring)
        {
            try
            {
                string JSON = passedstring.Data;
                string decodedJSON = ETIL.Core.Security.Base64Decode(JSON);
                var api = JsonConvert.DeserializeObject<UniversalAPI>(decodedJSON);

                string schema_procedure = null;
                string procedure_name = null; //procedure name from Wapi
                string connection = null; //connectrionstring from Wapi
                string[] procedure_parameters = null;
                string[] parameter_datatype = null;
                int[] datatype_length = null;
                string[] parameter_direction = null;
                List<string> parameters_names = new List<string>();
                List<string> parameters_datatypes = new List<string>();
                List<int> datatype_lengths = new List<int>();
                List<string> parameter_directions = new List<string>();

                ArrayList values = api.Values;

                List<SP_Name_Parameters> data = new List<SP_Name_Parameters>();

                SqlParameter outPutParameter;

                using (SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("KLConnectDev"))) //KLConnectDev
                {
                    var query = "KLCONNECT_UAPI_GET_PROCEDURE_AND_PARAMETERS_202109"; //UNIVERSAL_API_GET_PROCEDURE_AND_PARAMETERS_202109
                    SqlCommand getcommand = new SqlCommand(query, conn);
                    getcommand.CommandType = CommandType.StoredProcedure;

                    getcommand.Parameters.AddWithValue("@program_code", SafeDbObject(api.Program_Code));
                    getcommand.Parameters.AddWithValue("@key_code", SafeDbObject(api.Key_Code));
                    getcommand.Parameters.AddWithValue("@module", SafeDbObject(api.Module));
                    getcommand.Parameters.AddWithValue("@object", SafeDbObject(api.Object));
                    getcommand.Parameters.AddWithValue("@function", SafeDbObject(api.Function));
                    getcommand.Parameters.AddWithValue("@version", SafeDbObject(api.Version));

                    outPutParameter = getcommand.Parameters.Add("@sql_error_message", SqlDbType.VarChar, 200);
                    outPutParameter.Direction = ParameterDirection.Output;

                    var insertauditquery = "KLCONNECT_UAPI_AUDIT_LOG_INSERT_202108";
                    SqlCommand insertaudit = new SqlCommand(insertauditquery, conn);
                    insertaudit.CommandType = CommandType.StoredProcedure;

                    insertaudit.Parameters.AddWithValue("@program_code", SafeDbObject(api.Program_Code));
                    insertaudit.Parameters.AddWithValue("@module", SafeDbObject(api.Module));
                    insertaudit.Parameters.AddWithValue("@object", SafeDbObject(api.Object));
                    insertaudit.Parameters.AddWithValue("@function", SafeDbObject(api.Function));
                    insertaudit.Parameters.AddWithValue("@version", SafeDbObject(api.Version));
                    insertaudit.Parameters.AddWithValue("@date_created", SafeDbObject(DateTime.Now));
                    insertaudit.Parameters.AddWithValue("@location_created", SafeDbObject(api.Location_Created));

                    conn.Open();
                    SqlDataReader dataReader = getcommand.ExecuteReader();

                    while (dataReader.Read())
                    {
                        SP_Name_Parameters uapi = new SP_Name_Parameters();

                        uapi.Schema = dataReader["Schema"] as string;
                        uapi.Procedure = dataReader["Procedure"] as string;
                        uapi.ConnectionString = dataReader["ConnectionString"] as string;

                        schema_procedure = dataReader["Schema"].ToString();
                        procedure_name = dataReader["Procedure"].ToString();
                        connection = dataReader["ConnectionString"].ToString();

                        data.Add(uapi);
                    }

                    if (api.Location_Created == null)
                    {
                        string encodedmessage = ETIL.Core.Security.Base64Encode("Location Created is null");
                        return BadRequest(encodedmessage);
                    }
                    else
                    {
                        conn.Close();
                        conn.Open();
                        insertaudit.ExecuteNonQuery();
                    }
                }

                if (data.Any())
                {
                    using (SqlConnection conn2 = new SqlConnection(Configuration.GetConnectionString(connection))) //change "KLConnectConnection" to connection
                    {
                        //conn.Close();
                        var getparametersp = "SP_GET_PARAMETERS_BY_SCHEMA_AND_PROCEDURE";
                        SqlCommand getparametercommand = new SqlCommand(getparametersp, conn2);
                        getparametercommand.CommandType = CommandType.StoredProcedure;

                        getparametercommand.Parameters.AddWithValue("@SCHEMA", SafeDbObject(schema_procedure));
                        getparametercommand.Parameters.AddWithValue("@SP", SafeDbObject(procedure_name));

                        //List<SP_Name_Parameters> data2 = new List<SP_Name_Parameters>();

                        conn2.Open();
                        SqlDataReader dataReader2 = getparametercommand.ExecuteReader();

                        while (dataReader2.Read())
                        {
                            //SP_Name_Parameters uapi = new SP_Name_Parameters();

                            //uapi.PARAMETER_NAME = dataReader2["PARAMETER_NAME"] as string;

                            parameters_names.Add(dataReader2["PARAMETER_NAME"].ToString());
                            parameters_datatypes.Add(dataReader2["DATA_TYPE"].ToString());
                            datatype_lengths.Add(dataReader2["CHARACTER_MAXIMUM_LENGTH"] as int? ?? default(int));
                            parameter_directions.Add(dataReader2["PARAMETER_MODE"].ToString());
                            //data.Add(uapi);
                        }
                    }
                }
                else
                {
                    string output = outPutParameter.Value.ToString();
                    string encodedmessage = ETIL.Core.Security.Base64Encode(output);
                    return BadRequest(encodedmessage);
                }

                procedure_parameters = parameters_names.ToArray();
                parameter_datatype = parameters_datatypes.ToArray();
                datatype_length = datatype_lengths.ToArray();
                parameter_direction = parameter_directions.ToArray();
                
                using (SqlConnection conn3 = new SqlConnection(Configuration.GetConnectionString(connection))) //change "KLConnectConnection" to connection
                {
                    SqlParameter[] parameters = null;
                    string full_procedure_name = $"[{schema_procedure}].[{procedure_name}]";
                    apiprocedure = new SqlCommand(full_procedure_name, conn3);
                    apiprocedure.CommandType = CommandType.StoredProcedure;
                    conn3.Open();
                    if (procedure_parameters.Length != 0)
                    {
                        if (/*procedure_parameters.Length != 0 &&*/ values == null)
                        {
                            string encodedmessage = ETIL.Core.Security.Base64Encode("Value is null");
                            return BadRequest(encodedmessage);
                        }

                        else if (procedure_parameters.Length == values.Count)
                        {
                            //for (int i = 0; i < procedure_parameters.Length; i++)
                            //{
                            //    apiprocedure.Parameters.AddWithValue($"{procedure_parameters[i]}", SafeDbObject(values[i]));
                            //    apiprocedure.Parameters.Add($"{procedure_parameters[i]}", GetDataType(parameters_datatypes[i]), datatype_length[i], SafeDbObject(values[i]));
                            //}

                            //getcommand.Parameters.AddWithValue("@version", SafeDbObject(api.Version));

                            //outPutParameter = getcommand.Parameters.Add("@sql_error_message", SqlDbType.VarChar, 200);
                            //outPutParameter.Direction = ParameterDirection.Output;

                            parameters = GetProcParameters(parameter_directions, procedure_parameters, parameters_datatypes, datatype_length, values);
                            apiprocedure.Parameters.AddRange(parameters);
                        }

                        else
                        {
                            //string message = "[{\"Message\":\"" + $"Number of Parameters and Values provided not matched up" +
                            //    $" Parameters: {string.Join(",", procedure_parameters)}" +
                            //    $" Values: {string.Join(",", values.ToArray())}" + "\"}]";
                            //var serializemessage = JsonConvert.SerializeObject(message);
                            //string encodedmessage = ETIL.Core.Security.Base64Encode(serializemessage);
                            string encodedmessage = ETIL.Core.Security.Base64Encode($"Number of Parameters and Values provided not matched up" +
                                $", Parameters: {string.Join(", ", procedure_parameters)}" +
                                $", Values: {string.Join(", ", values.ToArray())}");
                            return BadRequest(encodedmessage);
                        }
                    }

                    //IEnumerable<string> filteredOut = parameter_directions.Where(x => x.Contains("OUT") || x.Contains("INOUT"));
                    string column_names = null;
                    int[] location_outputs = parameter_directions.Select((x,i)=> x.Contains("OUT")?i:-1).Where(i=>i>0).ToArray();
                    //int number_output = location_outputs.Count();
                    
                    DataTable table = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(apiprocedure);
                    sda.Fill(table);

                    //SqlParameter params = new SqlParameter();
                    
                    foreach(int location_output in location_outputs)
                    {
                        column_names = procedure_parameters[location_output].ToString().TrimStart('@');
                        table.Columns.Add(column_names);
                        //DataRow row = table.Select(column_names.TrimStart('@')).FirstOrDefault();
                        foreach (DataRow row in table.Rows)
                        {
                            switch (parameters_datatypes[location_output])
                            {
                                case "bigint":
                                    row[column_names] = Convert.ToInt32(parameters[location_output].Value);
                                    break;
                                case "binary":
                                    row[column_names] = Convert.ToByte(parameters[location_output].Value);
                                    break;
                                case "bit":
                                    row[column_names] = Convert.ToBoolean(parameters[location_output].Value);
                                    break;
                                case "char":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "date":
                                    row[column_names] = Convert.ToDateTime(parameters[location_output].Value);
                                    break;
                                case "datetime":
                                    row[column_names] = Convert.ToDateTime(parameters[location_output].Value);
                                    break;
                                case "datetime2":
                                    row[column_names] = Convert.ToDateTime(parameters[location_output].Value);
                                    break;
                                case "datetimeoffset":
                                    row[column_names] = DateTimeOffset.Parse(parameters[location_output].Value.ToString());
                                    break;
                                case "decimal":
                                    row[column_names] = Convert.ToDecimal(parameters[location_output].Value);
                                    break;
                                case "float":
                                    row[column_names] = (float)parameters[location_output].Value;
                                    break;
                                case "image":
                                    row[column_names] = Convert.ToByte(parameters[location_output].Value);
                                    break;
                                case "int":
                                    row[column_names] = Convert.ToInt32(parameters[location_output].Value);
                                    break;
                                case "money":
                                    row[column_names] = Convert.ToDecimal(parameters[location_output].Value);
                                    break;
                                case "nchar":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "ntext":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "nvarchar":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "real":
                                    row[column_names] = Convert.ToSingle(parameters[location_output].Value);
                                    break;
                                case "smalldatetime":
                                    row[column_names] = Convert.ToDateTime(parameters[location_output].Value);
                                    break;
                                case "smallint":
                                    row[column_names] = Convert.ToInt32(parameters[location_output].Value);
                                    break;
                                case "smallmoney":
                                    row[column_names] = Convert.ToSingle(parameters[location_output].Value);
                                    break;
                                case "text":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "time":
                                    row[column_names] = Convert.ToDateTime(parameters[location_output].Value);
                                    break;
                                case "tinyint":
                                    row[column_names] = Convert.ToInt32(parameters[location_output].Value);
                                    break;
                                case "uniqueidentifier":
                                    row[column_names] = Guid.Parse(parameters[location_output].Value.ToString());
                                    break;
                                case "varbinary":
                                    row[column_names] = Convert.ToByte(parameters[location_output].Value);
                                    break;
                                case "varchar":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                case "xml":
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                                default:
                                    row[column_names] = parameters[location_output].Value.ToString();
                                    break;
                            } 
                        }
                    }
                    //table.Rows.Add(row);
                    //table.Rows.Add(outparams);
                    var result = JsonConvert.SerializeObject(table);
                    string encodedresult = ETIL.Core.Security.Base64Encode(result);

                    if (table.Rows.Count > 0/* table.Tables.Count > 0*/ )
                    {
                        return Ok(encodedresult); //This originally return Content
                    }
                    else
                    {
                        //string encodedmessage = ETIL.Core.Security.Base64Encode("No Return Value");
                        return NoContent();
                    }
                }
            }
            catch (Exception e)
            {
                string encodedmessage = ETIL.Core.Security.Base64Encode(e.Message);
                return BadRequest(encodedmessage);
            }
        }

        private SqlParameter[] GetProcParameters(List<string> parameter_directions, string[] procedure_parameters, List<string> parameters_datatypes, int[] datatype_length, ArrayList values)
        {
            var parameters = new List<SqlParameter>();
            for (int i = 0; i < procedure_parameters.Length; i++)
            {
                SqlParameter parameter = CreateSqlParameter(parameter_directions, procedure_parameters, parameters_datatypes, datatype_length, values, i);
                parameters.Add(parameter);
            }
            return parameters.ToArray();
        }

        private static SqlParameter CreateSqlParameter(List<string> parameter_directions, string[] procedure_parameters, List<string> parameters_datatypes, int[] datatype_length, ArrayList values, int i)
        {
            var parameter_direction = parameter_directions[i];
            var paraName = procedure_parameters[i];
            var paraType = GetDataType(parameters_datatypes[i]);
            var val = GetValue(values, i, paraType);
            var paraLength = datatype_length[i];

            SqlParameter parameter = paraLength > 0 
                ? new SqlParameter(paraName, paraType, paraLength) { Value = val } 
                : new SqlParameter(paraName, paraType) { Value = val };
            if (parameter_direction == "OUT" || parameter_direction == "INOUT")
            {
                parameter.Direction = ParameterDirection.Output;
            }
            return parameter; 
        }

        private static object GetValue(ArrayList values, int i, SqlDbType paraType)
        {
            var val = values[i];
            if ((paraType == SqlDbType.VarBinary || paraType == SqlDbType.Image) && val is string str)
            {
                try
                {
                    var span = Convert.FromBase64String(str);
                    return SafeDbObject(span);
                }
                catch (Exception)
                {

                }
            }
            return SafeDbObject(val);
        }

    }
}
