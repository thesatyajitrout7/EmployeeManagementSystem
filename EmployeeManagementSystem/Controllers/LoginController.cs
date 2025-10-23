using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        //  GET: Login page
        public IActionResult Index()
        {
            return View();
        }

        //  POST: Verify login credentials
        [HttpPost]
        public IActionResult Verify(Login model)
        {
            if (ModelState.IsValid)
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("SELECT * FROM Users WHERE Username=@Username AND Password=@Password", con);
                cmd.Parameters.AddWithValue("@Username", model.username);
                cmd.Parameters.AddWithValue("@Password", model.password);

                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    //  Login success → set session
                    HttpContext.Session.SetString("Username", reader["Username"].ToString());
                    return RedirectToAction("Index", "Employee"); // redirect to employee dashboard
                }
                else
                {
                    //  Login failed
                    ViewBag.message = "Invalid username or password!";
                    return View("Index");
                }
            }
            return View("Index");
        }

        //  Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Index"); // redirect to login page
        }
    }
}
