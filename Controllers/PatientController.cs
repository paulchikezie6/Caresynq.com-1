using Microsoft.AspNetCore.Mvc;
using CareSynq.Data;
using CareSynq.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CareSynq.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var patient = await _context.Patients
                .Include(p => p.SensorDataRecords)
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Clinician)
                .FirstOrDefaultAsync(p => p.PatientId == userId.Value);

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSensorData(IFormFile csvFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            if (csvFile == null || csvFile.Length == 0)
                return Json(new { success = false, message = "No file uploaded" });

            try
            {
                using var reader = new StreamReader(csvFile.OpenReadStream());
                var csvContent = await reader.ReadToEndAsync();

                // Parse CSV and create sensor data records
                var sensorDataList = ParseCsvToSensorData(csvContent, userId.Value);

                if (sensorDataList.Count == 0)
                    return Json(new { success = false, message = "No valid data found in CSV" });

                _context.SensorDataRecords.AddRange(sensorDataList);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Successfully uploaded {sensorDataList.Count} readings",
                    recordCount = sensorDataList.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSensorData(int hours = 24)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false });

            var cutoffTime = DateTime.Now.AddHours(-hours);
            var data = await _context.SensorDataRecords
                .Where(sd => sd.PatientId == userId.Value && sd.Timestamp >= cutoffTime)
                .OrderByDescending(sd => sd.Timestamp)
                .Select(sd => new
                {
                    id = sd.SensorDataId,
                    timestamp = sd.Timestamp,
                    peakPressure = sd.PeakPressureIndex,
                    contactArea = sd.ContactAreaPercentage,
                    isAlert = sd.IsHighPressureAlert
                })
                .ToListAsync();

            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int sensorDataId, string commentText)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false });

            var comment = new Comment
            {
                SensorDataId = sensorDataId,
                PatientId = userId.Value,
                CommentText = commentText,
                CreatedDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, commentId = comment.CommentId });
        }

        private List<SensorData> ParseCsvToSensorData(string csvContent, int patientId)
        {
            var sensorDataList = new List<SensorData>();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Get sensor machine for this patient
            var sensorMachine = _context.SensorMachines
                .FirstOrDefault(sm => sm.AssignedPatientId == patientId);

            // If no sensor machine assigned, create a default one
            if (sensorMachine == null)
            {
                sensorMachine = new SensorMachine
                {
                    SerialNumber = $"SEN-{patientId:D4}",
                    ModelName = "Sensore Mat v1.0",
                    ManufactureDate = DateTime.Now,
                    IsActive = true,
                    AssignedPatientId = patientId
                };
                _context.SensorMachines.Add(sensorMachine);
                _context.SaveChanges(); // Save to get the ID
            }

            // Process each 32x32 frame
            for (int frameIndex = 0; frameIndex < lines.Length; frameIndex += 32)
            {
                if (frameIndex + 32 > lines.Length) break;

                // Parse the 32x32 matrix
                var matrix = new int[32, 32];
                bool validFrame = true;

                for (int row = 0; row < 32; row++)
                {
                    var lineIndex = frameIndex + row;
                    if (lineIndex >= lines.Length)
                    {
                        validFrame = false;
                        break;
                    }

                    var values = lines[lineIndex].Split(',');
                    if (values.Length < 32)
                    {
                        validFrame = false;
                        break;
                    }

                    for (int col = 0; col < 32; col++)
                    {
                        if (int.TryParse(values[col].Trim(), out int value))
                        {
                            matrix[row, col] = value;
                        }
                        else
                        {
                            matrix[row, col] = 1; // Default to 1 (no pressure)
                        }
                    }
                }

                if (!validFrame) continue;

                // Analyze the pressure map
                var (peakPressure, contactArea, isAlert) = AnalyzePressureMap(matrix);

                // Convert 2D array to jagged array for JSON serialization
                var jaggedArray = new int[32][];
                for (int i = 0; i < 32; i++)
                {
                    jaggedArray[i] = new int[32];
                    for (int j = 0; j < 32; j++)
                    {
                        jaggedArray[i][j] = matrix[i, j];
                    }
                }

                var sensorData = new SensorData
                {
                    PatientId = patientId,
                    SensorMachineId = sensorMachine.SensorMachineId,
                    Timestamp = DateTime.Now.AddMinutes(-(lines.Length - frameIndex) / 32), // Earlier times for earlier frames
                    PressureMapData = JsonSerializer.Serialize(jaggedArray), // Serialize jagged array instead
                    PeakPressureIndex = peakPressure,
                    ContactAreaPercentage = contactArea,
                    IsHighPressureAlert = isAlert
                };

                sensorDataList.Add(sensorData);
            }

            return sensorDataList;
        }

        private (double peakPressure, double contactArea, bool isAlert) AnalyzePressureMap(int[,] matrix)
        {
            int totalPixels = 32 * 32;
            int activePixels = 0;
            int maxPressure = 0;
            var highPressureRegions = new List<(int row, int col)>();
            int highPressurePixelCount = 0;

            // First pass: find all pressure values
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int value = matrix[i, j];

                    // Count active pixels (above threshold of 1)
                    if (value > 1)
                    {
                        activePixels++;
                    }

                    // Track max pressure
                    if (value > maxPressure)
                    {
                        maxPressure = value;
                    }

                    // Identify high pressure points (above 200)
                    if (value > 200)
                    {
                        highPressureRegions.Add((i, j));
                        highPressurePixelCount++;
                    }
                }
            }

            // Calculate Peak Pressure Index (excluding regions < 10 pixels)
            // For simplicity, we're using the max pressure found
            // In a more sophisticated version, you'd cluster high-pressure regions
            double peakPressureIndex = maxPressure;

            // Calculate Contact Area Percentage
            double contactAreaPercentage = (activePixels / (double)totalPixels) * 100;

            // Generate alert if:
            // 1. Peak pressure is very high (>220)
            // 2. There are sustained high pressure regions (10+ pixels above 200)
            bool isAlert = (maxPressure > 220) || (highPressurePixelCount >= 10);

            return (peakPressureIndex, contactAreaPercentage, isAlert);
        }
    }
}