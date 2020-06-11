using CW10.OldModels;
using System;
using System.Collections.Generic;

namespace CW10.DAL
{
    public interface IStudentDbService
    {
        public IEnumerable<Student> GetStudents(string orderBy);
        public Student GetStudent(string indexNumber);
        public Student GetStudent(string indexNumber, string password);
        public Student GetRefreshTokenOwner(string refreshToken);
        public int CreateStudent(Student student);
        public int CreateRefreshToken(RefreshToken refreshToken);
        public int UpdateStudent(string indexNumber, Student student);
        public int DeleteStudent(string indexNumber);
        public int DeleteRefreshToken(string refreshToken);
        public Enrollment GetStudentEnrollment(string indexNumber);
        public Studies GetStudies(string studiesName);
        public Enrollment CreateStudentEnrollment(
            string indexNumber, string firstName, string lastName, DateTime birthDate, string studiesName);
        public Enrollment GetEnrollment(int idEnrollment);
        public Enrollment GetEnrollment(int idStudy, int semester);
        public Enrollment SemesterPromote(int idStudy, int semester);
    }
}