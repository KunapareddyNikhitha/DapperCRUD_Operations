using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace PieChartDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class piechartController : ControllerBase
    {
        private readonly IConfiguration _config;

        public piechartController(IConfiguration config)
        {
            _config = config;
        }
        [HttpGet]
        [Route("getPieInfo")]
        public async Task<IEnumerable<PieResponse>> GetPieInfo()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var pieInfo = await connection.QueryAsync<PieResponse>($"SELECT (select COUNT(*) from piechart where IsInProject = 1) as IsInProject," +
                $"(select COUNT(*) from piechart where IsInProject = 0) as IsInBench," +
                $"(select COUNT(*) from piechart where IsInProject = 1 and IsInUpskilling = 1) as IsInProjandUpskilling");
            return pieInfo;
        }

        [HttpGet]
        [Route("getByIdPieInfo/{empId}")]
        public async Task<ActionResult<piechart>> GetByIdPieInfo(string empId)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var pieInfoById = await connection.QueryFirstAsync<piechart>("select * from piechart where EmployeeId = @Id", 
                new {Id = empId});
            return Ok(pieInfoById);
        }

        [HttpPost]
        [Route("postPieInfo")]
        public async Task<ActionResult<piechart>> PostPieInfo(piechart pieChart)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into piechart (EmployeeId, IsInProject, IsInUpskilling) values (@EmployeeId, @IsInProject, @IsInUpskilling)", pieChart);
            return Ok(await SelectAllPieInfo(connection));
        }

        [HttpPost]
        [Route("updatePieInfo")]
        public async Task<ActionResult<piechart>> UpdatePieInfo(piechart pieChart)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("update piechart set IsInProject = @isInProject, IsInUpskilling = @isInUpskilling where EmployeeId = @employeeId", new { employeeId = pieChart.EmployeeId , isInProject = pieChart.IsInProject, isInUpskilling = pieChart.IsInUpskilling});          
            return Ok(await SelectAllPieInfo(connection));
        }

        [HttpDelete]
        [Route("DeletePieInfo/{empId}")]
        public async Task<ActionResult<piechart>> DeletePieInfo(string empId)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("delete from piechart where EmployeeId = @Id",
                new { Id = empId });
            return Ok(await SelectAllPieInfo(connection));
        }
        private static async Task<IEnumerable<piechart>> SelectAllPieInfo(SqlConnection connection)
        {
            return await connection.QueryAsync<piechart>("select * from piechart");
        }
        
        
    }
}
