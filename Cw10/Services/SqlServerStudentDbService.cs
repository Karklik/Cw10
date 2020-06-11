using CW10.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CW10.DAL
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        private readonly string connectionString = "Data Source=db-mssql;Initial Catalog=s16556;Integrated Security=True";
        private SqlConnection SqlConnection => new SqlConnection(connectionString);

        private readonly SqlServerStudentDbContext _dbContext;

        public SqlServerStudentDbService(SqlServerStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int CreateRefreshToken(RefreshToken refreshToken)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "INSERT INTO RefreshToken " +
                "VALUES(@id, @indexNumber)"
            };

            command.Parameters.AddWithValue("id", refreshToken.Id);
            command.Parameters.AddWithValue("indexNumber", refreshToken.IndexNumber);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int CreateStudent(Student student)
        {
            if (_dbContext.Student.Where(s => string.Equals(s.IndexNumber, student.IndexNumber))
                .FirstOrDefault() != null)
                return 0;
            _dbContext.Student.Add(student);
            if (student.Password == null || student.Salt == null)
            {
                byte[] randomBytes = new Byte[16];
                RandomNumberGenerator.Create().GetNonZeroBytes(randomBytes);
                student.Salt = Convert.ToBase64String(new HMACSHA512().ComputeHash(randomBytes));
                student.Password = Convert.ToBase64String(
                        KeyDerivation.Pbkdf2(
                            password: "defaultPassword2020",
                            salt: Encoding.UTF8.GetBytes(student.Salt),
                            prf: KeyDerivationPrf.HMACSHA512,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8
                        )
                    );
            }
            return _dbContext.SaveChanges();
        }

        public Enrollment CreateStudentEnrollment(
            string indexNumber, string firstName, string lastName, DateTime birthDate, string studiesName)
        {
            var studies = GetStudies(studiesName);
            // If studies dosen't exists, stop
            if (studies == null)
                throw new ArgumentException("Studies dosen't exists");

            var enrollment = GetEnrollment(studies.IdStudy, 1);
            // If enrollment dosen't exists, create it
            if (enrollment == null)
            {
                enrollment = new Enrollment
                {
                    IdEnrollment = _dbContext.Enrollment.Max(e => e.IdEnrollment) + 1,
                    IdStudy = studies.IdStudy,
                    Semester = 1,
                    StartDate = DateTime.Now
                };
                _dbContext.Attach(enrollment);
                _dbContext.Add(enrollment);
            }

            var student = GetStudent(indexNumber);
            // If student already exists, stop
            if (student != null)
                throw new ArgumentException("Student with specific IndexNumber already exists");
            student = new Student
            {
                IndexNumber = indexNumber,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                IdEnrollment = enrollment.IdEnrollment
            };

            if (CreateStudent(student) == 0)
                return null;
            else
                return enrollment;
        }

        public int DeleteRefreshToken(string refreshToken)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "DELETE FROM RefreshToken WHERE Id = @refreshToken"
            };
            command.Parameters.AddWithValue("refreshToken", refreshToken);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int DeleteStudent(string indexNumber)
        {
            var student = new Student
            {
                IndexNumber = indexNumber
            };
            _dbContext.Attach(student);
            _dbContext.Remove(student);
            try
            {
                return _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return 0;
            }
        }

        public Enrollment GetEnrollment(int idEnrollment)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM Enrollment WHERE IdEnrollment = @idEnrollment"
            };
            command.Parameters.AddWithValue("idEnrollment", idEnrollment);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var enrollment = new Enrollment
                {
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Semester = IntegerType.FromObject(dataReader["Semester"]),
                    StartDate = DateTime.Parse(dataReader["StartDate"].ToString()),
                    IdStudy = IntegerType.FromObject(dataReader["IdStudy"])
                };
                return enrollment;
            }
            return null;
        }

        public Enrollment GetEnrollment(int idStudy, int semester)
        {
            return _dbContext.Enrollment
                .Where(e => e.IdStudy == idStudy)
                .Where(e => e.Semester == semester)
                .FirstOrDefault();
        }

        public Student GetRefreshTokenOwner(string refreshToken)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM RefreshToken WHERE Id = @refreshToken"
            };
            command.Parameters.AddWithValue("refreshToken", refreshToken);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var refreshTokenModel = new RefreshToken
                {
                    Id = dataReader["Id"].ToString(),
                    IndexNumber = dataReader["IndexNumber"].ToString()
                };
                return GetStudent(refreshTokenModel.IndexNumber);
            }
            return null;
        }

        public Student GetStudent(string indexNumber)
        {
            return _dbContext.Student
                .Where(student => string.Equals(student.IndexNumber, indexNumber))
                .FirstOrDefault();
        }

        public Student GetStudent(string indexNumber, string password)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM Student WHERE IndexNumber = @indexNumber AND Password = @password"
            };
            command.Parameters.AddWithValue("indexNumber", indexNumber);
            command.Parameters.AddWithValue("password", password);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var student = new Student
                {
                    IndexNumber = dataReader["IndexNumber"].ToString(),
                    FirstName = dataReader["FirstName"].ToString(),
                    LastName = dataReader["LastName"].ToString(),
                    BirthDate = DateTime.Parse(dataReader["BirthDate"].ToString()),
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Password = dataReader["Password"].ToString(),
                    Salt = dataReader["Salt"].ToString()
                };
                return student;
            }
            return null;
        }

        public Enrollment GetStudentEnrollment(string indexNumber)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT Enrollment.IdEnrollment, Semester, StartDate, Name " +
                "FROM Student " +
                "INNER JOIN Enrollment ON Student.IdEnrollment = Enrollment.IdEnrollment " +
                "INNER JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy " +
                "WHERE IndexNumber = @indexNumber"
            };
            command.Parameters.AddWithValue("indexNumber", indexNumber);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var enrollment = new Enrollment
                {
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Semester = IntegerType.FromObject(dataReader["Semester"]),
                    StartDate = DateTime.Parse(dataReader["StartDate"].ToString()),
                    //Name = dataReader["Name"].ToString(),
                };
                return enrollment;
            }
            return new Enrollment();
        }

        public IEnumerable<Student> GetStudents(string orderBy)
        {
            if (orderBy == null)
                orderBy = "indexnumber";
            return (orderBy.Trim().ToLower()) switch
            {
                "firstname" => _dbContext.Student.OrderBy(student => student.FirstName).ToList(),
                "lastname" => _dbContext.Student.OrderBy(student => student.LastName).ToList(),
                "birthdate" => _dbContext.Student.OrderBy(student => student.BirthDate).ToList(),
                "idenrollment" => _dbContext.Student.OrderBy(student => student.IdEnrollment).ToList(),
                _ => _dbContext.Student.OrderBy(student => student.IndexNumber).ToList(),
            };
        }

        public Studies GetStudies(string studiesName)
        {
            return _dbContext.Studies.Where(study => string.Equals(study.Name, studiesName)).FirstOrDefault();
        }

        public Enrollment SemesterPromote(int idStudy, int semester)
        {
            var currentEnrollment = GetEnrollment(idStudy, semester);
            // If current enrollment dosen't exists, stop 
            if (currentEnrollment == null) return null;

            var nextEnrollment = GetEnrollment(idStudy, semester + 1);
            // If enrollment for next semester dosen't exists, create it
            if (nextEnrollment == null)
            {
                nextEnrollment = new Enrollment
                {
                    IdEnrollment = _dbContext.Enrollment.Max(e => e.IdEnrollment) + 1,
                    IdStudy = currentEnrollment.IdStudy,
                    Semester = currentEnrollment.Semester + 1,
                    StartDate = DateTime.Now
                };
                _dbContext.Attach(nextEnrollment);
                _dbContext.Add(nextEnrollment);
            }

            // Promote students
            _dbContext.Student
                .Where(s => s.IdEnrollment == currentEnrollment.IdEnrollment)
                .Select(s => s)
                .ToList()
                .ForEach(s =>
                {
                    s.IdEnrollment = nextEnrollment.IdEnrollment;
                    _dbContext.Attach(s);
                    _dbContext.Entry(s).State = EntityState.Modified;
                });

            _dbContext.SaveChanges();

            return nextEnrollment;
        }

        public int UpdateStudent(string indexNumber, Student student)
        {
            var originalStudent = _dbContext.Student.Where(s => string.Equals(s.IndexNumber, indexNumber)).FirstOrDefault();
            if (originalStudent == null)
                return 0;
            originalStudent.IndexNumber = student.IndexNumber;
            originalStudent.FirstName = student.FirstName;
            originalStudent.LastName = student.LastName;
            originalStudent.BirthDate = student.BirthDate;
            originalStudent.IdEnrollment = student.IdEnrollment;
            return _dbContext.SaveChanges();
        }
    }
}