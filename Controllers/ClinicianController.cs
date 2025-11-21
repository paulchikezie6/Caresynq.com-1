using Microsoft.AspNetCore.Mvc;
using CareSynq.Data;
using CareSynq.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSynq.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicianController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var clinician = await _context.Clinicians
                .Include(c => c.Appointments)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(c => c.ClinicianId == userId.Value);

            return View(clinician);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    id = p.PatientId,
                    name = $"{p.FirstName} {p.LastName}",
                    email = p.Email,
                    hasAlerts = p.SensorDataRecords.Any(sd => sd.IsHighPressureAlert)
                })
                .ToListAsync();

            return Json(new { success = true, patients });
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientData(int patientId, int hours = 24)
        {
            var cutoffTime = DateTime.Now.AddHours(-hours);

            var sensorData = await _context.SensorDataRecords
                .Where(sd => sd.PatientId == patientId && sd.Timestamp >= cutoffTime)
                .OrderBy(sd => sd.Timestamp)
                .Include(sd => sd.Comments)
                .Select(sd => new
                {
                    id = sd.SensorDataId,
                    timestamp = sd.Timestamp,
                    peakPressure = sd.PeakPressureIndex,
                    contactArea = sd.ContactAreaPercentage,
                    isAlert = sd.IsHighPressureAlert,
                    comments = sd.Comments.Select(c => new
                    {
                        id = c.CommentId,
                        text = c.CommentText,
                        author = c.PatientId.HasValue ? "Patient" : "Clinician",
                        date = c.CreatedDate
                    })
                })
                .ToListAsync();

            return Json(new { success = true, data = sensorData });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int sensorDataId, string commentText)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            var comment = new Comment
            {
                SensorDataId = sensorDataId,
                ClinicianId = userId.Value,
                CommentText = commentText,
                CreatedDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, commentId = comment.CommentId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(int patientId, DateTime appointmentDate, string notes)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            var appointment = new Appointment
            {
                PatientId = patientId,
                ClinicianId = userId.Value,
                AppointmentDate = appointmentDate,
                Notes = notes,
                Status = "Scheduled",
                CreatedDate = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, appointmentId = appointment.AppointmentId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false });

            var appointments = await _context.Appointments
                .Where(a => a.ClinicianId == userId.Value)
                .Include(a => a.Patient)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new
                {
                    id = a.AppointmentId,
                    patientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    date = a.AppointmentDate,
                    status = a.Status,
                    notes = a.Notes
                })
                .ToListAsync();

            return Json(new { success = true, appointments });
        }
    }
}