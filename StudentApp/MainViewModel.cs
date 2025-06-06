using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace StudentApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Student _currentStudent;
        public Student CurrentStudent
        {
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStudentLoaded));
                OnPropertyChanged(nameof(StudentFullName));
                OnPropertyChanged(nameof(StudentCourse));
                OnPropertyChanged(nameof(StudentGroup));
                OnPropertyChanged(nameof(StudentDateOfBirth));
                UpdateGradesDisplay();
                UpdateDebtsDisplay();
                CalculatedOverallAverageGrade = null;
                CalculatedSubjectAverageGrade = null;
            }
        }

        // --- Свойства для создания студента ---
        private string _inputLastName = "";
        public string InputLastName { get => _inputLastName; set { _inputLastName = value; OnPropertyChanged(); } }

        private string _inputFirstName = "";
        public string InputFirstName { get => _inputFirstName; set { _inputFirstName = value; OnPropertyChanged(); } }

        private int _inputBirthDay = 1;
        public int InputBirthDay { get => _inputBirthDay; set { _inputBirthDay = value; OnPropertyChanged(); } }

        private int _inputBirthMonth = 1;
        public int InputBirthMonth { get => _inputBirthMonth; set { _inputBirthMonth = value; OnPropertyChanged(); } }

        private int _inputBirthYear = 2025;
        public int InputBirthYear { get => _inputBirthYear; set { _inputBirthYear = value; OnPropertyChanged(); } }

        private int _inputCourse = 1;
        public int InputCourse { get => _inputCourse; set { _inputCourse = value; OnPropertyChanged(); } }

        private string _inputGroup = "Группа";
        public string InputGroup { get => _inputGroup; set { _inputGroup = value; OnPropertyChanged(); } }

        public bool IsStudentLoaded => CurrentStudent != null;
        public string StudentFullName => CurrentStudent != null ? $"{CurrentStudent.LastName} {CurrentStudent.FirstName}" : "Студент не загружен";
        public string StudentCourse => CurrentStudent != null ? CurrentStudent.Course.ToString() : "-";
        public string StudentGroup => CurrentStudent != null ? CurrentStudent.Group : "-";
        public string StudentDateOfBirth => CurrentStudent != null ? CurrentStudent.DateOfBirth.ToString("dd.MM.yyyy") : "-";

        private string _gradesDisplay;
        public string GradesDisplay { get => _gradesDisplay; private set { _gradesDisplay = value; OnPropertyChanged(); } }


        private int _inputGradeSemester = 1;
        public int InputGradeSemester { get => _inputGradeSemester; set { _inputGradeSemester = value; OnPropertyChanged(); UpdateAvailableSubjectsForGrade(); } }

        private string _selectedSubjectForGrade;
        public string SelectedSubjectForGrade { get => _selectedSubjectForGrade; set { _selectedSubjectForGrade = value; OnPropertyChanged(); } }

        private int? _inputGradeValue;
        public int? InputGradeValue { get => _inputGradeValue; set { _inputGradeValue = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _availableSubjectsForGrade;
        public ObservableCollection<string> AvailableSubjectsForGrade
        {
            get => _availableSubjectsForGrade;
            set { _availableSubjectsForGrade = value; OnPropertyChanged(); }
        }

        private string _inputSubjectForAverage;
        public string InputSubjectForAverage { get => _inputSubjectForAverage; set { _inputSubjectForAverage = value; OnPropertyChanged(); } }

        private double? _calculatedSubjectAverageGrade;
        public double? CalculatedSubjectAverageGrade { get => _calculatedSubjectAverageGrade; private set { _calculatedSubjectAverageGrade = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _allAvailableSubjects;
        public ObservableCollection<string> AllAvailableSubjects
        {
            get => _allAvailableSubjects;
            set { _allAvailableSubjects = value; OnPropertyChanged(); }
        }

        private double? _calculatedOverallAverageGrade;
        public double? CalculatedOverallAverageGrade { get => _calculatedOverallAverageGrade; private set { _calculatedOverallAverageGrade = value; OnPropertyChanged(); } }

        private string _debtsDisplay;
        public string DebtsDisplay { get => _debtsDisplay; private set { _debtsDisplay = value; OnPropertyChanged(); } }

        public ICommand CreateStudentCommand { get; }
        public ICommand SetGradeCommand { get; }
        public ICommand CalculateOverallAverageCommand { get; }
        public ICommand CalculateSubjectAverageCommand { get; }
        public ICommand ShowDebtsCommand { get; }

        private Dictionary<int, List<string>> _defaultSubjectsBySemester = new Dictionary<int, List<string>>
        {
            { 1, new List<string> { "Математика", "Физика", "История" } },
            { 2, new List<string> { "Математика", "Физика", "Программирование" } },
            { 3, new List<string> { "Алгоритмы", "Базы данных", "Философия" } },
            { 4, new List<string> { "Алгоритмы", "Базы данных", "Операционные системы" } }
        };

        public MainViewModel()
        {
            CreateStudentCommand = new RelayCommand(OnCreateStudent);
            SetGradeCommand = new RelayCommand(OnSetGrade, () => IsStudentLoaded && !string.IsNullOrEmpty(SelectedSubjectForGrade));
            CalculateOverallAverageCommand = new RelayCommand(OnCalculateOverallAverage, () => IsStudentLoaded);
            CalculateSubjectAverageCommand = new RelayCommand(OnCalculateSubjectAverage, () => IsStudentLoaded && !string.IsNullOrEmpty(InputSubjectForAverage));
            ShowDebtsCommand = new RelayCommand(UpdateDebtsDisplay, () => IsStudentLoaded);

            AvailableSubjectsForGrade = new ObservableCollection<string>();
            AllAvailableSubjects = new ObservableCollection<string>();

        }

        private void OnCreateStudent()
        {
            try
            {
                DateTime dob = new DateTime(InputBirthYear, InputBirthMonth, InputBirthDay);
                CurrentStudent = new Student(
                    InputLastName, InputFirstName, dob,
                    InputCourse, InputGroup, _defaultSubjectsBySemester
                );
                UpdateAvailableSubjectsForGrade();
                UpdateAllAvailableSubjects();
                MessageBox.Show($"Студент {CurrentStudent.FirstName} {CurrentStudent.LastName} создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка создания студента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentStudent = null;
            }
        }

        private void UpdateGradesDisplay()
        {
            if (CurrentStudent == null)
            {
                GradesDisplay = "Студент не загружен или нет оценок.";
                return;
            }
            GradesDisplay = CurrentStudent.GetGradesReport();
        }

        private void UpdateDebtsDisplay()
        {
            if (CurrentStudent == null)
            {
                DebtsDisplay = "Студент не загружен.";
                return;
            }
            var debts = CurrentStudent.GetSubjectsWithDebt();
            DebtsDisplay = debts.Any() ? string.Join("\n", debts) : "Задолженностей нет.";
        }
        
        private void UpdateAvailableSubjectsForGrade()
        {
            AvailableSubjectsForGrade.Clear();
            SelectedSubjectForGrade = null;
            if (CurrentStudent != null && CurrentStudent.SubjectsBySemester.TryGetValue(InputGradeSemester, out var subjects))
            {
                foreach (var subject in subjects.OrderBy(s => s))
                {
                    AvailableSubjectsForGrade.Add(subject);
                }
                if (AvailableSubjectsForGrade.Any())
                {
                    SelectedSubjectForGrade = AvailableSubjectsForGrade.First();
                }
            }
        }

        private void UpdateAllAvailableSubjects()
        {
            AllAvailableSubjects.Clear();
            InputSubjectForAverage = null;
            if (CurrentStudent != null)
            {
                var allSubjects = CurrentStudent.SubjectsBySemester.Values
                                        .SelectMany(list => list)
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToList();
                foreach (var subject in allSubjects)
                {
                    AllAvailableSubjects.Add(subject);
                }
                if (AllAvailableSubjects.Any())
                {
                    InputSubjectForAverage = AllAvailableSubjects.First();
                }
            }
        }

        private void OnSetGrade()
        {
            if (CurrentStudent == null || string.IsNullOrEmpty(SelectedSubjectForGrade))
            {
                MessageBox.Show("Сначала создайте студента и выберите предмет.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CurrentStudent[InputGradeSemester, SelectedSubjectForGrade] = InputGradeValue;
                UpdateGradesDisplay();
                UpdateDebtsDisplay();
                InputGradeValue = null;
                MessageBox.Show($"Оценка по предмету '{SelectedSubjectForGrade}' (семестр {InputGradeSemester}) установлена/обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка установки оценки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCalculateOverallAverage()
        {
            if (CurrentStudent == null) return;
            CalculatedOverallAverageGrade = CurrentStudent.CalculateAverageGrade();
            if (CalculatedOverallAverageGrade.HasValue)
            {
                MessageBox.Show($"Общий средний балл: {CalculatedOverallAverageGrade:F2}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Нет оценок для расчета среднего балла.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnCalculateSubjectAverage()
        {
            if (CurrentStudent == null || string.IsNullOrEmpty(InputSubjectForAverage))
            {
                 MessageBox.Show("Сначала создайте студента и выберите предмет для расчета.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CalculatedSubjectAverageGrade = CurrentStudent.CalculateAverageGradeForSubject(InputSubjectForAverage);
            if (CalculatedSubjectAverageGrade.HasValue)
            {
                 MessageBox.Show($"Средний балл по предмету '{InputSubjectForAverage}': {CalculatedSubjectAverageGrade:F2}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Нет оценок по предмету '{InputSubjectForAverage}' для расчета.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}