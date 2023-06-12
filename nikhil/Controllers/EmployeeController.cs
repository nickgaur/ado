using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using nikhil.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace nikhil.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string createToken(Employee emp)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, emp.Email),
                new Claim(ClaimTypes.Role, emp.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "JWTAuthenticationServer",
                audience: "JWTServicePostmanClient",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signIn);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine("JWT TOKEN : " + jwtToken);
            return jwtToken;
        }
        private List<Employee> EmpList()
        {

            List<Employee> empModels = new List<Employee>();
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("connStr"));
            SqlCommand cmd = new SqlCommand("Select * from Employees", con);
            SqlDataAdapter adapterr = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapterr.Fill(dt);



            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Employee emp = new Employee();
                emp.Id = (int)dt.Rows[i]["Id"];
                emp.Name = dt.Rows[i]["Name"].ToString();
                emp.Email = dt.Rows[i]["Email"].ToString();
                emp.Role = dt.Rows[i]["Role"].ToString();
                emp.Password = dt.Rows[i]["Password"].ToString();



                empModels.Add(emp);
            }
            return empModels;
        }

        [HttpGet]
        [Route("getEmployees")]
        public List<Employee> getEmployees()
        {
            return EmpList();
        }

        [HttpPost]
        [Route("AddEmployee")] [Authorize(Roles = "admin")]
        public string AddEmployee(Employee emp)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("connStr"));
            SqlCommand cmd = new SqlCommand("Insert into Employees values ('" + emp.Name + "', '" + emp.Email + "', '" + emp.Role + "', '" + emp.Password + "')", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return "Employee Added";
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] login user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var userInDb = EmpList().Where(employee => employee.Email == user.Email && employee.Password == user.Password).ToList();


            if (userInDb == null)
            {
                return NotFound();
            }

            string token = createToken(userInDb.FirstOrDefault());
            var response = new
            {
                token = token,
                user = userInDb
            };
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateEmployee/{Id}"), Authorize(Roles = "admin")]
        public string UpdateEmployee([FromRoute] int Id,UpdateEmployee emp)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("connStr"));
            SqlCommand cmd = new SqlCommand("update Employees set name = '" + emp.Name + "', email = '" + emp.Email + "' where id = " + Id , conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return "Employee Updated";
        }

        [HttpDelete]
        [Route("DeleteEmployee/{Id}"), Authorize(Roles = "admin")]
        public string DeleteEmployee([FromRoute] int Id)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("connStr"));
            SqlCommand cmd = new SqlCommand("delete from Employees where id = " + Id, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return "Employee Deleted";
        }

    }
}
