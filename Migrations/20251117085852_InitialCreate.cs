using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareSynq.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinicians",
                columns: table => new
                {
                    ClinicianId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinicians", x => x.ClinicianId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ClinicianId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_Clinicians_ClinicianId",
                        column: x => x.ClinicianId,
                        principalTable: "Clinicians",
                        principalColumn: "ClinicianId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SensorMachines",
                columns: table => new
                {
                    SensorMachineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedPatientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorMachines", x => x.SensorMachineId);
                    table.ForeignKey(
                        name: "FK_SensorMachines_Patients_AssignedPatientId",
                        column: x => x.AssignedPatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "SensorDataRecords",
                columns: table => new
                {
                    SensorDataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    SensorMachineId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PressureMapData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeakPressureIndex = table.Column<double>(type: "float", nullable: false),
                    ContactAreaPercentage = table.Column<double>(type: "float", nullable: false),
                    IsHighPressureAlert = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorDataRecords", x => x.SensorDataId);
                    table.ForeignKey(
                        name: "FK_SensorDataRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SensorDataRecords_SensorMachines_SensorMachineId",
                        column: x => x.SensorMachineId,
                        principalTable: "SensorMachines",
                        principalColumn: "SensorMachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorDataId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    ClinicianId = table.Column<int>(type: "int", nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Clinicians_ClinicianId",
                        column: x => x.ClinicianId,
                        principalTable: "Clinicians",
                        principalColumn: "ClinicianId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId");
                    table.ForeignKey(
                        name: "FK_Comments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_SensorDataRecords_SensorDataId",
                        column: x => x.SensorDataId,
                        principalTable: "SensorDataRecords",
                        principalColumn: "SensorDataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clinicians",
                columns: new[] { "ClinicianId", "CreatedDate", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber", "Specialization" },
                values: new object[] { 1, new DateTime(2025, 11, 17, 11, 58, 50, 995, DateTimeKind.Local).AddTicks(4349), "clinician@test.com", "Paul", true, "Smith", "hashedpassword123", "5551234567", "Wound Care Specialist" });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "CreatedDate", "DateOfBirth", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 17, 11, 58, 50, 995, DateTimeKind.Local).AddTicks(4058), new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "patient@test.com", "Sarah", true, "Khan", "hashedpassword123", "1234567890" },
                    { 2, new DateTime(2025, 11, 17, 11, 58, 50, 995, DateTimeKind.Local).AddTicks(4063), new DateTime(1970, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "michael.green@test.com", "Michael", false, "Green", "hashedpassword123", "0987654321" }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "AppointmentId", "AppointmentDate", "ClinicianId", "CreatedDate", "Notes", "PatientId", "Status" },
                values: new object[] { 1, new DateTime(2025, 11, 24, 11, 58, 50, 995, DateTimeKind.Local).AddTicks(4447), 1, new DateTime(2025, 11, 17, 11, 58, 50, 995, DateTimeKind.Local).AddTicks(4477), "Regular checkup for pressure ulcer prevention", 1, "Scheduled" });

            migrationBuilder.InsertData(
                table: "SensorMachines",
                columns: new[] { "SensorMachineId", "AssignedPatientId", "IsActive", "LastCalibrationDate", "ManufactureDate", "ModelName", "SerialNumber" },
                values: new object[,]
                {
                    { 1, 1, true, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sensore Mat Pro", "SM-2024-001" },
                    { 2, 2, true, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sensore Mat Pro", "SM-2024-002" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClinicianId",
                table: "Appointments",
                column: "ClinicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinicians_Email",
                table: "Clinicians",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ClinicianId",
                table: "Comments",
                column: "ClinicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PatientId",
                table: "Comments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_SensorDataId",
                table: "Comments",
                column: "SensorDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Email",
                table: "Patients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorDataRecords_PatientId",
                table: "SensorDataRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorDataRecords_SensorMachineId",
                table: "SensorDataRecords",
                column: "SensorMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorMachines_AssignedPatientId",
                table: "SensorMachines",
                column: "AssignedPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorMachines_SerialNumber",
                table: "SensorMachines",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Clinicians");

            migrationBuilder.DropTable(
                name: "SensorDataRecords");

            migrationBuilder.DropTable(
                name: "SensorMachines");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
