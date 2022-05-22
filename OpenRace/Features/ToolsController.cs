using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRace.Data.Ef;

namespace OpenRace.Features;

[ApiController]
[Route("api/tools")]
public class ToolsController : ControllerBase
{
    private readonly ConnectionChecker _checker;
    private readonly ILogger<ToolsController> _logger;

    public ToolsController(ConnectionChecker checker, ILogger<ToolsController> logger)
    {
        _checker = checker;
        _logger = logger;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var ping = await _checker.Ping();
        _logger.LogDebug("Db Ping: {PingMs:N0} ms", ping.TotalMilliseconds);
        return Ok(new { DbPing = (int)ping.TotalMilliseconds });
    }
}