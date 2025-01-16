using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace project.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class customer_searchController : ControllerBase
    {
        [HttpGet("{cat}")]
        public IEnumerable<customer_search> Get(int cat)
        {
            List<customer_search> li = new List<customer_search>();
            // SqlConnection conn1 = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog = mynewdb; Integrated Security = True; Pooling = False");
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("projectContext");
            SqlConnection conn1 = new SqlConnection(conStr);
            string sql;
            sql = "SELECT * FROM usersaccounts where role ='" + cat + "' ";
            SqlCommand comm = new SqlCommand(sql, conn1);
            conn1.Open();
            SqlDataReader reader = comm.ExecuteReader();
            while (reader.Read())
            {
                li.Add(new customer_search
                {
                    name = (string)reader["name"],
                    password = (int)reader["password"],
                    role = (string)reader["role"],
                });
            }
            reader.Close();
            conn1.Close();
            return li;
        }
    }
}
