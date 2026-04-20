using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.TorrServer.Api;

/// <inheritdoc />
[ApiController]
[Route("plugins/torrserver")]
public class PluginController() : ControllerBase
{
    /// <summary>
    /// Get all resources.
    /// </summary>
    /// <returns>result.</returns>
    [HttpGet]
    [Route("status")]
    public IActionResult Status()
        => Ok();
}
