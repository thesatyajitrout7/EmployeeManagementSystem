using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT * FROM Users WHERE Username=@Username AND Password=@Password", con);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password); // plain text password

            con.Open();
            using var reader = cmd.ExecuteReader();  // reader ko using block me rakha
            if (reader.Read())
            {
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", reader["Role"].ToString());
                return RedirectToAction("Index", "Employee");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
