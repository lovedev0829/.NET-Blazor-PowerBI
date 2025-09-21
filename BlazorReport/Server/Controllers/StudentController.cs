using BlazorReport.Shared;
using BlazorReport.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazorReport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentDataService _studentDataService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentDataService studentDataService, ILogger<StudentController> logger)
        {
            _studentDataService = studentDataService;
            _logger = logger;
        }

        [HttpPost("search")]
        public async Task<ActionResult<StudentSearchResult>> SearchStudents([FromBody] StudentSearchCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Searching students with criteria: {@Criteria}", criteria);
                
                var result = await _studentDataService.SearchStudentsAsync(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students");
                return BadRequest(new StudentSearchResult
                {
                    Students = new List<StudentInfo>(),
                    TotalCount = 0,
                    Success = false,
                    Message = $"Error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("dropdown-data")]
        public async Task<ActionResult<DropdownData>> GetDropdownData()
        {
            try
            {
                _logger.LogInformation("Getting dropdown data");
                
                var result = await _studentDataService.GetDropdownDataAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dropdown data");
                return BadRequest(new { Message = $"Error occurred: {ex.Message}" });
            }
        }

        [HttpPost("count")]
        public async Task<ActionResult<int>> GetStudentCount([FromBody] StudentSearchCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Getting student count with criteria: {@Criteria}", criteria);
                
                var count = await _studentDataService.GetStudentCountAsync(criteria);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student count");
                return BadRequest(0);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentInfo>> GetStudent(int id)
        {
            try
            {
                _logger.LogInformation("Getting student with ID: {Id}", id);
                
                // For now, we'll search by RowID
                var criteria = new StudentSearchCriteria();
                var result = await _studentDataService.SearchStudentsAsync(criteria);
                var student = result.Students.FirstOrDefault(s => s.RowID == id);
                
                if (student == null)
                {
                    return NotFound();
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student with ID: {Id}", id);
                return BadRequest(new { Message = $"Error occurred: {ex.Message}" });
            }
        }

        [HttpPost("search-individual")]
        public async Task<ActionResult<StudentSearchResult>> SearchIndividualStudent([FromBody] IndividualStudentSearchCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Searching individual student with criteria: {@Criteria}", criteria);
                
                var result = await _studentDataService.SearchIndividualStudentAsync(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching individual student");
                return BadRequest(new StudentSearchResult
                {
                    Students = new List<StudentInfo>(),
                    TotalCount = 0,
                    Success = false,
                    Message = $"Error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("{studentId}/detail")]
        public async Task<ActionResult<StudentDetailInfo>> GetStudentDetail(int studentId)
        {
            try
            {
                _logger.LogInformation("Getting student detail for ID: {StudentId}", studentId);
                
                var result = await _studentDataService.GetStudentDetailAsync(studentId);
                
                if (result == null)
                {
                    return NotFound($"Student with ID {studentId} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student detail for ID: {StudentId}", studentId);
                return BadRequest(new { Message = $"Error occurred: {ex.Message}" });
            }
        }

        [HttpGet("search-by-program")]
        public async Task<ActionResult<StudentSearchResult>> SearchStudentsByProgram(
            [FromQuery] string programType, 
            [FromQuery] string school = "All", 
            [FromQuery] string grade = "All")
        {
            try
            {
                _logger.LogInformation("Searching students by program: {ProgramType}, School: {School}, Grade: {Grade}", 
                    programType, school, grade);
                
                var result = await _studentDataService.SearchStudentsByProgramAsync(programType, school, grade);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students by program");
                return BadRequest(new StudentSearchResult
                {
                    Students = new List<StudentInfo>(),
                    TotalCount = 0,
                    Success = false,
                    Message = $"Error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("diagnose-database")]
        public async Task<ActionResult<Dictionary<string, string>>> DiagnoseDatabase()
        {
            try
            {
                _logger.LogInformation("Running database diagnostics");
                
                var diagnostics = await _studentDataService.DiagnoseDatabaseAsync();
                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running database diagnostics");
                return BadRequest(new Dictionary<string, string>
                {
                    {"Error", ex.Message},
                    {"Status", "Failed to run diagnostics"}
                });
            }
        }

        [HttpGet("cascading-dropdown/{level}")]
        public async Task<ActionResult<List<DropdownItem>>> GetCascadingDropdownData(string level, [FromQuery] string? school = null, [FromQuery] string? grade = null, [FromQuery] string? teacher = null)
        {
            try
            {
                _logger.LogInformation("Getting cascading dropdown data for level: {Level}, school: {School}, grade: {Grade}, teacher: {Teacher}", level, school, grade, teacher);
                
                var result = await _studentDataService.GetCascadingDropdownDataAsync(level, school, grade, teacher);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cascading dropdown data for level: {Level}, school: {School}, grade: {Grade}, teacher: {Teacher}", level, school, grade, teacher);
                return BadRequest(new List<DropdownItem> 
                { 
                    new DropdownItem { Value = "All", Text = $"All {level}" } 
                });
            }
        }
    }
} 