using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MiniService.Data;
using MiniService.Models.Port;

namespace MiniService.Controllers.Port;

// port 1:1 Mst_VINModelOrginalController.ExportTemplate
[ApiController]
[Route("api/mst_vinmodelorginal")]
public class Mst_VINModelOrginalControllerPort : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public Mst_VINModelOrginalControllerPort(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("exporttemplate")]
    public async Task<IActionResult> ExportTemplate(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] object? req = null)
    {
        var lst = await _db.Set<Mst_VINModelOrginal>().AsNoTracking().Take(0).ToListAsync();
        var dicColNames = new Dictionary<string, string>()
        {
            { "VINCode", "VINCode (*)" },
            { "ModelCode", "ModelCode (*)" },
            { "OrginalCode", "OrginalCode (*)" },
        };

        var (physicalPath, virtualPath) = PortExcel.GenExcelExportFilePath(_env.WebRootPath, "Import_thiet_lap_nguon_goc-model_xe");
        PortExcel.ExportToExcelFromList(lst, dicColNames, physicalPath, "Mst_VINModelOrginalCode");

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var resultUrl = $"{baseUrl}{virtualPath}";

        return Ok(new
        {
            Success = true,
            Data = resultUrl
        });
    }
}
