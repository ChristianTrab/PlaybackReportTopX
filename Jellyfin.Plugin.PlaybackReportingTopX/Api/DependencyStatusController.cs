using Jellyfin.Plugin.PlaybackReportingTopX.Services;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Api;

/// <summary>
/// API endpoints for plugin status.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Route("PlaybackReportingTopX")]
public class DependencyStatusController : ControllerBase
{
    private readonly IPlaybackReportingDependencyChecker _dependencyChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyStatusController"/> class.
    /// </summary>
    /// <param name="dependencyChecker">Dependency checker.</param>
    public DependencyStatusController(IPlaybackReportingDependencyChecker dependencyChecker)
    {
        _dependencyChecker = dependencyChecker;
    }

    /// <summary>
    /// Gets the current Playback Reporting dependency status.
    /// </summary>
    /// <returns>Dependency status payload.</returns>
    [HttpGet("DependencyStatus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<DependencyStatusResponse> GetDependencyStatus()
    {
        var result = _dependencyChecker.Check();
        return Ok(new DependencyStatusResponse
        {
            IsReady = result.IsReady,
            State = result.State.ToString(),
            Message = result.Message
        });
    }

    /// <summary>
    /// Dependency status response payload.
    /// </summary>
    public sealed class DependencyStatusResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the dependency is ready.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Gets or sets the dependency state name.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
