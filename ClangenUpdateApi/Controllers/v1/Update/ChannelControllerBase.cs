using ClangenUpdateApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClangenUpdateApi.Controllers.v1.Update;

public abstract class UpdateControllerBase : Controller
{
    [NonAction]
    public ActionResult? ValidateChannel(Channel channel)
    {
        if (!channel.Exists)
        {
            return NotFound(new { success = false, messages = new[] { $"""No channel with name "{channel.ChannelName}" found.""" } });
        }

        return null;
    }
}
