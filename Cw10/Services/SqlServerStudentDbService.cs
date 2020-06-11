using CW10.Models;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CW10.DAL
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        private readonly string connectionString = "Data Source=db-mssql;Initial Catalog=s16556;Integrated Security=True";
        private SqlConnection SqlConnection => new SqlConnection(connectionString);

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
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "INSERT INTO Student " +
                "VALUES(@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)"
            };
            command.Parameters.AddWithValue("indexNumber", student.IndexNumber);
            command.Parameters.AddWithValue("firstName", student.FirstName);
            command.Parameters.AddWithValue("lastName", student.LastName);
            command.Parameters.AddWithValue("birthDate", student.BirthDate);
            command.Parameters.AddWithValue("idEnrollment", student.IdEnrollment);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public Enrollment CreateStudentEnrollment(
            string indexNumber, string firstName, string lastName, DateTime birthDate, string studiesName)
        {
            using var connection = SqlConnection;
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new SqlCommand
            {
                Connection = connection,
                Transaction = transaction
            };
            try
            {
                // Checking if studies exists
                command.CommandText = "SELECT * FROM Studies WHERE Name = @name";
                command.Parameters.AddWithValue("name", studiesName);
                using var studiesDataReader = command.ExecuteReader();
                if (!studiesDataReader.Read())
                {
                    studiesDataReader.Close();
                    transaction.Rollback();
                    throw new ArgumentException("Studies dosen't exists");
                }
                var studies = new Studies
                {
                    IdStudy = IntegerType.FromObject(studiesDataReader["IdStudy"]),
                    Name = studiesDataReader["Name"].ToString()
                };
                studiesDataReader.Close();

                // Checking if enrollment exisits
                command.CommandText = "SELECT * " +
                                    "FROM Enrollment " +
                                    "WHERE IdStudy = @idStudy AND Semester = 1";
                command.Parameters.AddWithValue("idStudy", studies.IdStudy);
                using var entrollmentDataReader = command.ExecuteReader();
                Enrollment enrollment;
                static Enrollment enrollmentMapper(SqlDataReader reader)
                {
                    var enrollment = new Enrollment
                    {
                        IdEnrollment = IntegerType.FromObject(reader["IdEnrollment"]),
                        Semester = IntegerType.FromObject(reader["Semester"]),
                        StartDate = reader["StartDate"].ToString(),
                        IdStudy = IntegerType.FromObject(reader["IdStudy"])
                    };
                    reader.Close();
                    return enrollment;
                }
                // If enrollment dosen't exist create new one
                if (!entrollmentDataReader.Read())
                {
                    entrollmentDataReader.Close();
                    // Get IdEnrollment as max +1 since DB don't have auto increment on PK
                    command.CommandText = "select max(IdEnrollment) + 1 from Enrollment";
                    var IdEnrollment = 0;
                    using var idEntrollmentDataReader = command.ExecuteReader();
                    if (idEntrollmentDataReader.Read())
                        IdEnrollment = IntegerType.FromObject(idEntrollmentDataReader[0]);
                    idEntrollmentDataReader.Close();

                    // Prepare Enrollment object for insert
                    enrollment = new Enrollment
                    {
                        IdEnrollment = IdEnrollment,
                        Semester = 1,
                        IdStudy = studies.IdStudy,
                        StartDate = DateTime.Now.ToString("yyyy-MM-dd")
                    };
                    command.CommandText = "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) " +
                                        "VALUES(@idEnrollment, @semester, @idStudy, @startDate)";
                    command.Parameters.AddWithValue("idEnrollment", enrollment.IdEnrollment);
                    command.Parameters.AddWithValue("semester", enrollment.Semester);
                    command.Parameters.AddWithValue("startDate", enrollment.StartDate);
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT * " +
                                        "FROM Enrollment " +
                                        "WHERE IdStudy = @idStudy AND Semester = 1";
                    using var entrollmentDataReader2 = command.ExecuteReader();
                    entrollmentDataReader2.Read();
                    enrollment = enrollmentMapper(entrollmentDataReader2);
                }
                else
                {
                    enrollment = enrollmentMapper(entrollmentDataReader);
                    command.Parameters.AddWithValue("idEnrollment", enrollment.IdEnrollment);
                }

                // Chceking if index number is unique
                command.CommandText = "SELECT * FROM Student WHERE IndexNumber = @indexNumber";
                command.Parameters.AddWithValue("indexNumber", indexNumber);
                using var studentDataReader = command.ExecuteReader();
                if (studentDataReader.Read())
                {
                    studentDataReader.Close();
                    transaction.Rollback();
                    throw new ArgumentException("Student with specific IndexNumber already exists");
                }
                studentDataReader.Close();

                // Create new student
                command.CommandText = "INSERT INTO Student " +
                    "VALUES(@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)";
                command.Parameters.AddWithValue("firstName", firstName);
                command.Parameters.AddWithValue("lastName", lastName);
                command.Parameters.AddWithValue("birthDate", birthDate.ToString("yyyy-MM-dd"));

                if (command.ExecuteNonQuery() == 0)
                {
                    transaction.Rollback();
                    return null;
                }
                transaction.Commit();
                return enrollment;
            }
            catch (NotFiniteNumberException)
            {
                transaction.Rollback();
            }
            return null;
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
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "DELETE FROM Student WHERE IndexNumber = @indexNumber"
            };
            command.Parameters.AddWithValue("indexNumber", indexNumber);
            connection.Open();
            return command.ExecuteNonQuery();
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
                    StartDate = dataReader["StartDate"].ToString(),
                    IdStudy = IntegerType.FromObject(dataReader["IdStudy"])
                };
                return enrollment;
            }
            return null;
        }

        public Enrollment GetEnrollment(int idStudy, int semester)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM Enrollment WHERE IdStudy = @idStudy AND Semester = @semester"
            };
            command.Parameters.AddWithValue("idStudy", idStudy);
            command.Parameters.AddWithValue("semester", semester);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var enrollment = new Enrollment
                {
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Semester = IntegerType.FromObject(dataReader["Semester"]),
                    StartDate = dataReader["StartDate"].ToString(),
                    IdStudy = IntegerType.FromObject(dataReader["IdStudy"])
                };
                return enrollment;
            }
            return null;
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
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM Student WHERE IndexNumber = @indexNumber"
            };
            command.Parameters.AddWithValue("indexNumber", indexNumber);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var student = new Student
                {
                    IndexNumber = dataReader["IndexNumber"].ToString(),
                    FirstName = dataReader["FirstName"].ToString(),
                    LastName = dataReader["LastName"].ToString(),
                    BirthDate = dataReader["BirthDate"].ToString(),
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Password = dataReader["Password"].ToString(),
                    Salt = dataReader["Salt"].ToString()
                };
                return student;
            }
            return null;
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
                    BirthDate = dataReader["BirthDate"].ToString(),
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
                    StartDate = dataReader["StartDate"].ToString(),
                    Name = dataReader["Name"].ToString(),
                };
                return enrollment;
            }
            return new Enrollment();
        }

        public IEnumerable<Student> GetStudents(string orderBy)
        {
            if (orderBy == null)
                orderBy = "IndexNumber";
            List<Student> students = new List<Student>();
            using var connection = SqlConnection;
            using var command = new SqlCommand()
            {
                Connection = connection,
                CommandText = $"SELECT * FROM Student ORDER BY {orderBy}"
            };
            connection.Open();
            using var dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                var student = new Student
                {
                    IndexNumber = dataReader["IndexNumber"].ToString(),
                    FirstName = dataReader["FirstName"].ToString(),
                    LastName = dataReader["LastName"].ToString(),
                    BirthDate = dataReader["BirthDate"].ToString(),
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Password = dataReader["Password"].ToString(),
                    Salt = dataReader["Salt"].ToString()
                };
                students.Add(student);
            }
            return students;
        }

        public Studies GetStudies(string studiesName)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "SELECT * FROM Studies WHERE Name = @studiesName"
            };
            command.Parameters.AddWithValue("studiesName", studiesName);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                Studies studies = new Studies
                {
                    IdStudy = IntegerType.FromObject(dataReader["IdStudy"]),
                    Name = dataReader["Name"].ToString()
                };
                return studies;
            }
            return null;
        }

        public Enrollment SemesterPromote(int idStudy, int semester)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "sp_SemesterPromote",
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("id_study", idStudy);
            command.Parameters.AddWithValue("semester", semester);
            connection.Open();
            using var dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var enrollment = new Enrollment
                {
                    IdEnrollment = IntegerType.FromObject(dataReader["IdEnrollment"]),
                    Semester = IntegerType.FromObject(dataReader["Semester"]),
                    StartDate = dataReader["StartDate"].ToString(),
                    IdStudy = IntegerType.FromObject(dataReader["IdStudy"])
                };
                return enrollment;
            }
            return null;
        }

        public int UpdateStudent(string indexNumber, Student student)
        {
            using var connection = SqlConnection;
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandText = "UPDATE Student " +
                "SET IndexNumber = @newIndexNumber, FirstName = @firstName, " +
                "LastName = @lastName, BirthDate = @birthDate, " +
                "IdEnrollment = @idEnrollment " +
                "WHERE IndexNumber = @oldIndexNumber"
            };
            command.Parameters.AddWithValue("newIndexNumber", student.IndexNumber);
            command.Parameters.AddWithValue("firstName", student.FirstName);
            command.Parameters.AddWithValue("lastName", student.LastName);
            command.Parameters.AddWithValue("birthDate", student.BirthDate);
            command.Parameters.AddWithValue("idEnrollment", student.IdEnrollment);
            command.Parameters.AddWithValue("oldIndexNumber", indexNumber);
            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}