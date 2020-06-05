using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DogWalker.Models;
using DogWalker.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalker.Controllers
{
    public class OwnersController : Controller
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public OwnersController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Owners
        public ActionResult Index()
        {
            return View();
        }

        // GET: Owners/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Owners/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Owners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT INTO Owner ([Name], Email, Phone, Address, NeighborhoodId)
                            VALUES (@name, @email, @phoneNumber, @address, @neighborhoodId);
                        ";

                        cmd.Parameters.AddWithValue("@name", owner.Name);
                        cmd.Parameters.AddWithValue("@email", owner.Email);
                        cmd.Parameters.AddWithValue("@phoneNumber", owner.PhoneNumber);
                        cmd.Parameters.AddWithValue("@address", owner.Address);
                        cmd.Parameters.AddWithValue("@neighborhoodId", owner.NeighborhoodId);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction("Index", "Walkers");
                    }
                }
            }
            catch(Exception ex)
            {
                return View(owner);
            }
        }

        // GET: Owners/Edit/5
        public ActionResult Edit(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Email, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        reader.Close();
                        return View(owner);
                    }

                    reader.Close();
                    return NotFound();
                }
            }
        }

        // POST: Owners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE Owner
                            SET 
                                [Name] = @name, 
                                Email = @email, 
                                Address = @address, 
                                Phone = @phone, 
                                NeighborhoodId = @neighborhoodId
                            WHERE Id = @id";

                        cmd.Parameters.AddWithValue("@name", owner.Name);
                        cmd.Parameters.AddWithValue("@email", owner.Email);
                        cmd.Parameters.AddWithValue("@address", owner.Address);
                        cmd.Parameters.AddWithValue("@phone", owner.PhoneNumber);
                        cmd.Parameters.AddWithValue("@neighborhoodId", owner.NeighborhoodId);
                        cmd.Parameters.AddWithValue("@id", owner.Id);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction("Index", "Walkers");
                    }
                }
            }
            catch(Exception ex)
            {
                return View(owner);
            }
        }

        // GET: Owners/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Email, NeighborhoodId, Address, Phone
                        FROM Owner
                        WHERE Id = @id
                    ";

                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
                        };

                        reader.Close();
                        return View(owner);
                    }

                    reader.Close();
                    return NotFound();
                }
            }
        }

        // POST: Owners/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Owner owner)
        {
            try
            {
                using(SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            DELETE FROM Owner
                            WHERE Id = @id
                        ";

                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction("Index", "Walkers");
                    }
                }
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel viewModel)
        {

            Owner owner = GetOwnerByEmail(viewModel.Email);

            if (owner == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),
                new Claim(ClaimTypes.Email, owner.Email),
                new Claim(ClaimTypes.Name, owner.Name),
                new Claim("NeighborhoodId", owner.NeighborhoodId.ToString()),
                new Claim(ClaimTypes.Role, "DogOwner"),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        private Owner GetOwnerByEmail(string email)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, [Name], Email, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Email = @email";

                    cmd.Parameters.AddWithValue("@email", email);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                        reader.Close();
                        return owner;
                    }

                    reader.Close();
                    return null;
                }
            }
        }
    }
}