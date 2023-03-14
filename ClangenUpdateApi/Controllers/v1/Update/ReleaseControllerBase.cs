using ClangenUpdateApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.v1.Update;

public abstract class ReleaseControllerBase : UpdateControllerBase
{
    [NonAction]
    public ActionResult? ValidateRelease(Release release)
    {
        if (!release.Exists)
        {
            return NotFound(new { success = false, messages = new[] { $"""No release with name "{release.ReleaseName}" found.""" } });
        }

        return null;
    }
}
