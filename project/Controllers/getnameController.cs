using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace project.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class getnameController : ControllerBase
    {
        [HttpGet("{role}")]
        public IEnumerable<buyitems> Get(string role)
        {
            List<buyitems> li = new List<buyitems>();
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("projectContext");
            SqlConnection conn1 = new SqlConnection(conStr);
            string sql;
            sql = "SELECT * FROM usersaccounts where role ='" + role + "' ";
            SqlCommand comm = new SqlCommand(sql, conn1);
            conn1.Open();
            SqlDataReader reader = comm.ExecuteReader();
            while (reader.Read())
            {
                li.Add(new buyitems
                {
                    name = (string)reader["name"],
                });
            }
            reader.Close();
            conn1.Close();
            return li;
        }
    }
}
