using BlazorReport.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BlazorReport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        // Mock data for demonstration - replace with actual database operations
        private static readonly List<Student> MockStudents = new()
        {
            new Student { Id = 1, StudentName = "Anika", School = "MMS", Grade = 6, Teacher = "Aaron, Andrea", Class = "Math 6A", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 2, StudentName = "Angel", School = "MMS", Grade = 6, Teacher = "Aaron, Andrea", Class = "Math 6A", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 3, StudentName = "Blake", School = "MMS", Grade = 7, Teacher = "Aaron, Andrea", Class = "Math 7A", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 4, StudentName = "Bobby", School = "MMS", Grade = 6, Teacher = "Aaron, Andrea", Class = "Math 6B", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 5, StudentName = "Brogan", School = "MMS", Grade = 8, Teacher = "Aaron, Andrea", Class = "Math 8A", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 6, StudentName = "Brooke", School = "MMS", Grade = 8, Teacher = "Aaron, Andrea", Class = "Math 8B", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" },
            new Student { Id = 7, StudentName = "Bryson", School = "MMS", Grade = 6, Teacher = "Aaron, Andrea", Class = "Math 6A", SPED = "✓", ML = "", Interventions = "✓", IntvNotes = "✓", Assessments = "", Profile = "" },
            new Student { Id = 8, StudentName = "Emilie", School = "MMS", Grade = 8, Teacher = "Aaron, Andrea", Class = "Math 8A", SPED = "", ML = "", Interventions = "", IntvNotes = "", Assessments = "", Profile = "" }
        };

        [HttpPost("search")]
        public async Task<ActionResult<StudentSearchResult>> SearchStudents([FromBody] StudentSearchCriteria criteria)
        {
            try
            {
                await Task.Delay(100); // Simulate database delay

                var filteredStudents = MockStudents.AsQueryable();

                // Apply filters based on criteria
                if (criteria.School != "All")
                {
                    filteredStudents = filteredStudents.Where(s => s.School == criteria.School);
                }

                if (criteria.Grade != "All")
                {
                    if (int.TryParse(criteria.Grade, out int grade))
                    {
                        filteredStudents = filteredStudents.Where(s => s.Grade == grade);
                    }
                }

                if (criteria.Teacher != "All")
                {
                    filteredStudents = filteredStudents.Where(s => s.Teacher.Contains(criteria.Teacher));
                }

                if (criteria.Class != "All Classes Selected")
                {
                    filteredStudents = filteredStudents.Where(s => s.Class == criteria.Class);
                }

                var result = filteredStudents.ToList();

                return Ok(new StudentSearchResult
                {
                    Students = result,
                    TotalCount = result.Count,
                    Success = true,
                    Message = "Search completed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new StudentSearchResult
                {
                    Students = new List<Student>(),
                    TotalCount = 0,
                    Success = false,
                    Message = $"Error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("dropdown-data")]
        public async Task<ActionResult<object>> GetDropdownData()
        {
            try
            {
                await Task.Delay(50); // Simulate database delay

                var schools = MockStudents.Select(s => s.School).Distinct().OrderBy(s => s).ToList();
                var grades = MockStudents.Select(s => s.Grade).Distinct().OrderBy(g => g).ToList();
                var teachers = MockStudents.Select(s => s.Teacher).Distinct().OrderBy(t => t).ToList();
                var classes = MockStudents.Select(s => s.Class).Distinct().OrderBy(c => c).ToList();

                return Ok(new
                {
                    Schools = schools.Select(s => new DropdownItem { Value = s, Text = s }).ToList(),
                    Grades = grades.Select(g => new DropdownItem { Value = g.ToString(), Text = g.ToString() }).ToList(),
                    Teachers = teachers.Select(t => new DropdownItem { Value = t, Text = t }).ToList(),
                    Classes = classes.Select(c => new DropdownItem { Value = c, Text = c }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            try
            {
                await Task.Delay(50); // Simulate database delay

                var student = MockStudents.FirstOrDefault(s => s.Id == id);
                if (student == null)
                {
                    return NotFound();
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Error occurred: {ex.Message}" });
            }
        }
    }
} 