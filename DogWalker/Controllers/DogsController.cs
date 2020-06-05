using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DogWalker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalker.Controllers
{
    public class DogsController : Controller
    {
        private readonly IConfiguration _config;

        // The constructor accepts an IConfiguration object as a parameter. This class comes from the ASP.NET framework and is useful for retrieving things out of the appsettings.json file like connection strings.
        public DogsController(IConfiguration config)
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

        // GET: Dogs
        public ActionResult Index()
        {
            int ownerId = GetCurrentUserId();

            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Breed, Notes, ImageUrl, OwnerId 
                        FROM Dog
                        WHERE OwnerId = @ownerId
                    ";

                    cmd.Parameters.AddWithValue("@ownerId", ownerId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog dog = new Dog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                        };

                        // Check if optional columns are null
                        if (reader.IsDBNull(reader.GetOrdinal("Notes")) == false)
                        {
                            dog.Notes = reader.GetString(reader.GetOrdinal("Notes"));
                        }
                        if (reader.IsDBNull(reader.GetOrdinal("ImageUrl")) == false)
                        {
                            dog.ImageUrl = reader.GetString(reader.GetOrdinal("Notes"));
                        }

                        dogs.Add(dog);
                    }
                    reader.Close();
                    return View(dogs);
                }
            }
        }

        // GET: Dogs/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Dogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Dogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using(SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT INTO Dog (Name, Breed, OwnerId, Notes, ImageUrl)
                            VALUES (@name, @breed, @ownerId, @notes, @imageUrl)
                        ";

                        cmd.Parameters.AddWithValue("@name", dog.Name);
                        cmd.Parameters.AddWithValue("@breed", dog.Breed);
                        cmd.Parameters.AddWithValue("@ownerId", GetCurrentUserId());

                        // optional columns
                        cmd.Parameters.AddWithValue("@notes", dog.Notes ?? "");
                        cmd.Parameters.AddWithValue("@imageUrl", dog.ImageUrl ?? "");

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));

                    }
                }

            }
            catch (Exception ex)
            {
                return View(dog);
            }
        }

        // GET: Dogs/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Dogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Dogs/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Dogs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private int GetCurrentUserId()
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(id);
        }
    }
}