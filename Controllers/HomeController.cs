using Microsoft.AspNetCore.Mvc;
using CareSynq.Data;
using CareSynq.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSynq.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Check if patient
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == email && p.IsActive);

            if (patient != null)
            {
                HttpContext.Session.SetInt32("UserId", patient.PatientId);
                HttpContext.Session.SetString("UserType", "Patient");
                HttpContext.Session.SetString("UserName", $"{patient.FirstName} {patient.LastName}");
                return RedirectToAction("Dashboard", "Patient");
            }

            // Check if clinician
            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);

            if (clinician != null)
            {
                HttpContext.Session.SetInt32("UserId", clinician.ClinicianId);
                HttpContext.Session.SetString("UserType", "Clinician");
                HttpContext.Session.SetString("UserName", $"{clinician.FirstName} {clinician.LastName}");
                return RedirectToAction("Dashboard", "Clinician");
            }

            // Check if admin (hardcoded for now)
            if (email == "admin@test.com")
            {
                HttpContext.Session.SetString("UserType", "Admin");
                HttpContext.Session.SetString("UserName", "Admin User");
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewBag.Error = "Invalid credentials";
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}