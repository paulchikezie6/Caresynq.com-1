using Microsoft.AspNetCore.Mvc;
using CareSynq.Data;
using CareSynq.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CareSynq.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Admin")
                return RedirectToAction("Index", "Home");

            var stats = new
            {
                totalPatients = await _context.Patients.CountAsync(),
                activePatients = await _context.Patients.CountAsync(p => p.IsActive),
                totalClinicians = await _context.Clinicians.CountAsync(),
                activeClinicians = await _context.Clinicians.CountAsync(c => c.IsActive),
                pendingAlerts = await _context.SensorDataRecords.CountAsync(sd => sd.IsHighPressureAlert),
                totalAppointments = await _context.Appointments.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var patients = await _context.Patients
                .Select(p => new
                {
                    id = p.PatientId,
                    name = $"{p.FirstName} {p.LastName}",
                    email = p.Email,
                    role = "Patient",
                    status = p.IsActive ? "Active" : "Inactive",
                    type = "patient"
                })
                .ToListAsync();

            var clinicians = await _context.Clinicians
                .Select(c => new
                {
                    id = c.ClinicianId,
                    name = $"{c.FirstName} {c.LastName}",
                    email = c.Email,
                    role = "Clinician",
                    status = c.IsActive ? "Active" : "Inactive",
                    type = "clinician"
                })
                .ToListAsync();

            var allUsers = patients.Cast<object>().Concat(clinicians.Cast<object>()).ToList();
            return Json(new { success = true, users = allUsers });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            try
            {
                // Remove validation errors for navigation properties
                ModelState.Remove("Appointments");
                ModelState.Remove("SensorDataRecords");
                ModelState.Remove("Comments");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();

                    var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                    return Json(new { success = false, message = $"Validation failed: {errorMessage}" });
                }

                // Check if email already exists
                var existingPatient = await _context.Patients.AnyAsync(p => p.Email == patient.Email);
                if (existingPatient)
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                patient.CreatedDate = DateTime.Now;
                patient.IsActive = true;

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                return Json(new { success = true, patientId = patient.PatientId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClinician([FromBody] Clinician clinician)
        {
            try
            {
                // Remove validation errors for navigation properties
                ModelState.Remove("Appointments");
                ModelState.Remove("Comments");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();

                    var errorMessage = string.Join("; ", errors.SelectMany(e => e.Errors));
                    return Json(new { success = false, message = $"Validation failed: {errorMessage}" });
                }

                // Check if email already exists
                var existingClinician = await _context.Clinicians.AnyAsync(c => c.Email == clinician.Email);
                if (existingClinician)
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                clinician.CreatedDate = DateTime.Now;
                clinician.IsActive = true;

                _context.Clinicians.Add(clinician);
                await _context.SaveChangesAsync();

                return Json(new { success = true, clinicianId = clinician.ClinicianId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userType, int userId)
        {
            if (userType == "patient")
            {
                var patient = await _context.Patients.FindAsync(userId);
                if (patient != null)
                {
                    patient.IsActive = !patient.IsActive;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }
            else if (userType == "clinician")
            {
                var clinician = await _context.Clinicians.FindAsync(userId);
                if (clinician != null)
                {
                    clinician.IsActive = !clinician.IsActive;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false, message = "User not found" });
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorMachines()
        {
            var machines = await _context.SensorMachines
                .Include(sm => sm.AssignedPatient)
                .Select(sm => new
                {
                    id = sm.SensorMachineId,
                    serialNumber = sm.SerialNumber,
                    modelName = sm.ModelName,
                    isActive = sm.IsActive,
                    assignedTo = sm.AssignedPatient != null
                        ? $"{sm.AssignedPatient.FirstName} {sm.AssignedPatient.LastName}"
                        : "Unassigned"
                })
                .ToListAsync();

            return Json(new { success = true, machines });
        }

        [HttpPost]
        public async Task<IActionResult> AssignSensorMachine(int machineId, int patientId)
        {
            var machine = await _context.SensorMachines.FindAsync(machineId);
            if (machine != null)
            {
                machine.AssignedPatientId = patientId;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Machine not found" });
        }

        // Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userType, int userId)
        {
            try
            {
                if (userType == "patient")
                {
                    var patient = await _context.Patients
                        .Include(p => p.SensorDataRecords)
                        .Include(p => p.Appointments)
                        .Include(p => p.Comments)
                        .FirstOrDefaultAsync(p => p.PatientId == userId);

                    if (patient == null)
                        return Json(new { success = false, message = "Patient not found." });

                    // Check if patient has active sensor assignments
                    var hasSensor = await _context.SensorMachines
                        .AnyAsync(sm => sm.AssignedPatientId == userId && sm.IsActive);

                    if (hasSensor)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Cannot delete patient with active sensor assignments. Please unassign sensors first."
                        });
                    }

                    // Unassign any sensor machines
                    var assignedMachines = await _context.SensorMachines
                        .Where(sm => sm.AssignedPatientId == userId)
                        .ToListAsync();

                    foreach (var machine in assignedMachines)
                    {
                        machine.AssignedPatientId = null;
                    }

                    // Delete patient
                    _context.Patients.Remove(patient);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Patient deleted successfully." });
                }
                else if (userType == "clinician")
                {
                    var clinician = await _context.Clinicians
                        .Include(c => c.Appointments)
                        .Include(c => c.Comments)
                        .FirstOrDefaultAsync(c => c.ClinicianId == userId);

                    if (clinician == null)
                        return Json(new { success = false, message = "Clinician not found." });

                    // Check for upcoming appointments
                    var hasUpcomingAppointments = await _context.Appointments
                        .AnyAsync(a => a.ClinicianId == userId &&
                                       a.AppointmentDate > DateTime.Now &&
                                       a.Status == "Scheduled");

                    if (hasUpcomingAppointments)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Cannot delete clinician with upcoming appointments. Please cancel or reassign appointments first."
                        });
                    }

                    // Delete clinician
                    _context.Clinicians.Remove(clinician);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Clinician deleted successfully." });
                }

                return Json(new { success = false, message = "Invalid user type." });
            }
            catch (DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete this user because there are associated records. Please remove or reassign related data first."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Unexpected error occurred: {ex.Message}" });
            }
        }


        // Get All Appointments
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    id = a.AppointmentId,
                    patientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    clinicianName = $"{a.Clinician.FirstName} {a.Clinician.LastName}",
                    appointmentDate = a.AppointmentDate,
                    status = a.Status,
                    notes = a.Notes,
                    createdDate = a.CreatedDate
                })
                .ToListAsync();

            return Json(new { success = true, appointments });
        }

        // Delete Appointment
        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return Json(new { success = false, message = "Appointment not found" });

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Appointment deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Cancel Appointment (Soft delete)
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return Json(new { success = false, message = "Appointment not found" });

                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}