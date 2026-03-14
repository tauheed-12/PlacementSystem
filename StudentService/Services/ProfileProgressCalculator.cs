using StudentService.Entities;

namespace StudentService.Services
{
    public class ProfileProgressCalculator
    {
        public static decimal Calculate(Student student)
        {
            decimal progress = 0;

            // Academic Info (40%)
            if (!string.IsNullOrEmpty(student.RollNo) &&
                !string.IsNullOrEmpty(student.EnrollmentNo) &&
                !string.IsNullOrEmpty(student.Course) &&
                !string.IsNullOrEmpty(student.Branch) &&
                student.Year > 0 &&
                student.CGPA > 0)
            {
                progress += 40;
            }

            // Skills (20%)
            if (student.Skills != null && student.Skills.Any())
            {
                progress += 20;
            }

            // Contact (20%)
            if (!string.IsNullOrEmpty(student.Email) &&
                !string.IsNullOrEmpty(student.PhoneNumber))
            {
                progress += 20;
            }

            // Resume (20%)
            if (student.Documents.Any(d => d.DocumentType == "Resume"))
            {
                progress += 20;
            }

            return progress;
        }
    }
}
