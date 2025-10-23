using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EmployeeManagement.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace EmployeeManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        //  LOGIN CHECK
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        //  LIST (READ)
        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            var list = new List<Employee>();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("GetEmployees", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Employee
                {
                    Eid = (int)reader["Eid"],
                    Name = reader["Name"].ToString(),
                    Age = (int)reader["Age"],
                    Position = reader["Position"].ToString(),
                    Salary = (decimal)reader["Salary"]
                });
            }
            con.Close();

            return View(list);
        }

        //  CREATE (GET)
        public IActionResult Create()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            return View();
        }

        //  CREATE (POST)
        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            try
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("AddEmployee", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@Age", emp.Age);
                cmd.Parameters.AddWithValue("@Position", emp.Position);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while adding employee: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //  EDIT (GET)
        public IActionResult Edit(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            Employee emp = null;
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("GetEmployees", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if ((int)reader["Eid"] == id)
                {
                    emp = new Employee
                    {
                        Eid = (int)reader["Eid"],
                        Name = reader["Name"].ToString(),
                        Age = (int)reader["Age"],
                        Position = reader["Position"].ToString(),
                        Salary = (decimal)reader["Salary"]
                    };
                    break;
                }
            }
            con.Close();

            return View(emp);
        }

        //  EDIT (POST)
        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            try
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("UpdateEmployee", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Eid", emp.Eid);
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@Age", emp.Age);
                cmd.Parameters.AddWithValue("@Position", emp.Position);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while updating employee: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //  DELETE (GET) - confirm delete
        public IActionResult Delete(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login");

            Employee emp = null;
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("GetEmployees", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if ((int)reader["Eid"] == id)
                {
                    emp = new Employee
                    {
                        Eid = (int)reader["Eid"],
                        Name = reader["Name"].ToString(),
                        Age = (int)reader["Age"],
                        Position = reader["Position"].ToString(),
                        Salary = (decimal)reader["Salary"]
                    };
                    break;
                }
            }
            con.Close();

            return View(emp); // Delete.cshtml will handle AJAX
        }

        //  DELETE (POST) - AJAX
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Not logged in!" });

            try
            {
                using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("DeleteEmployee", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Eid", id);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                con.Close();

                if (rowsAffected == 0)
                    return Json(new { success = false, message = "No employee found with this ID." });

                return Json(new { success = true, message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while deleting employee: " + ex.Message });
            }
        }
    }
}
