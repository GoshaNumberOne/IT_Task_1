using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentApp
{
    public class Student
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Course { get; set; }
        public string Group { get; set; }

        public Dictionary<int, List<string>> SubjectsBySemester { get; private set; }

        private Dictionary<int, Dictionary<string, int?>> _grades;

        public Student(
            string lastName,
            string firstName,
            DateTime dateOfBirth,
            int course,
            string group,
            Dictionary<int, List<string>> subjectsBySemesterSetup)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Фамилия не может быть пустой.", nameof(lastName));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Имя не может быть пустым.", nameof(firstName));
            if (course < 1)
                throw new ArgumentOutOfRangeException(nameof(course), "Курс не может быть меньше 1.");
            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentException("Группа не может быть пустой.", nameof(group));
            if (subjectsBySemesterSetup == null || !subjectsBySemesterSetup.Any())
                throw new ArgumentException("Список предметов по семестрам должен быть предоставлен и не быть пустым.", nameof(subjectsBySemesterSetup));

            LastName = lastName;
            FirstName = firstName;
            DateOfBirth = dateOfBirth;
            Course = course;
            Group = group;
            SubjectsBySemester = new Dictionary<int, List<string>>(subjectsBySemesterSetup);

            _grades = new Dictionary<int, Dictionary<string, int?>>();
            InitializeGradesStructure();
        }

        private void InitializeGradesStructure()
        {
            foreach (var semesterEntry in SubjectsBySemester)
            {
                _grades[semesterEntry.Key] = new Dictionary<string, int?>();
                foreach (var subject in semesterEntry.Value)
                {
                    _grades[semesterEntry.Key][subject] = null;
                }
            }
        }

        public int? this[int semester, string subjectName]
        {
            get
            {
                if (!_grades.ContainsKey(semester) || !_grades[semester].ContainsKey(subjectName))
                {
                    throw new ArgumentException($"Предмет '{subjectName}' в семестре {semester} не найден в учебном плане.");
                }
                return _grades[semester][subjectName];
            }
            set
            {
                if (!_grades.ContainsKey(semester) || !_grades[semester].ContainsKey(subjectName))
                {
                    throw new ArgumentException($"Невозможно установить оценку: предмет '{subjectName}' в семестре {semester} не найден в учебном плане.");
                }
                if (value.HasValue && (value < 0 || value > 50))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Оценка должна быть в диапазоне от 0 до 10 (или не установлена).");
                }
                _grades[semester][subjectName] = value;
            }
        }

        public double? CalculateAverageGrade()
        {
            List<int> allGrades = new List<int>();
            foreach (var semesterGrades in _grades.Values)
            {
                foreach (var grade in semesterGrades.Values)
                {
                    if (grade.HasValue)
                    {
                        allGrades.Add(grade.Value);
                    }
                }
            }
            return allGrades.Any() ? allGrades.Average() : (double?)null;
        }

        public double? CalculateAverageGradeForSubject(string subjectName)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                throw new ArgumentException("Название предмета не может быть пустым.", nameof(subjectName));

            List<int> subjectGrades = new List<int>();
            foreach (var semesterGrades in _grades.Values)
            {
                if (semesterGrades.TryGetValue(subjectName, out int? grade) && grade.HasValue)
                {
                    subjectGrades.Add(grade.Value);
                }
            }
            return subjectGrades.Any() ? subjectGrades.Average() : (double?)null;
        }

        public List<string> GetSubjectsWithDebt(int passingGradeThreshold = 25)
        {
            var debts = new List<string>();
            foreach (var semesterEntry in _grades)
            {
                foreach (var subjectEntry in semesterEntry.Value)
                {
                    if (!subjectEntry.Value.HasValue || subjectEntry.Value < passingGradeThreshold)
                    {
                        debts.Add($"Семестр {semesterEntry.Key}, Предмет: {subjectEntry.Key} (Оценка: {(subjectEntry.Value.HasValue ? subjectEntry.Value.ToString() : "N/A")})");
                    }
                }
            }
            return debts;
        }

        public override string ToString()
        {
            return $"{LastName} {FirstName}, Курс: {Course}, Группа: {Group}";
        }

        public string GetGradesReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Отчет по оценкам для студента: {LastName} {FirstName}");
            foreach (var semesterEntry in _grades.OrderBy(s => s.Key))
            {
                sb.AppendLine($"\nСеместр {semesterEntry.Key}:");
                if (SubjectsBySemester.TryGetValue(semesterEntry.Key, out var subjectsForSemester))
                {
                    foreach (var subjectName in subjectsForSemester.OrderBy(s => s))
                    {
                        if (semesterEntry.Value.TryGetValue(subjectName, out int? grade))
                        {
                            sb.AppendLine($"  - {subjectName}: {(grade.HasValue ? grade.Value.ToString() : "Нет оценки")}");
                        }
                        else
                        {
                            sb.AppendLine($"  - {subjectName}: (Предмет не найден в оценках, хотя есть в плане)");
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}