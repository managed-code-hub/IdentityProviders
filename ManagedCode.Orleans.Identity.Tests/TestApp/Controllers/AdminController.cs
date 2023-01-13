using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

[Authorize(Roles = "admin")]
[Route("adminController")]
public class AdminController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> AdminsOnly()
    {
        return "Admins only";
    }

    [HttpGet("adminsList")]
    [AllowAnonymous]
    public ActionResult<string> AdminsList()
    {
        return "adminsList";
    }

    [HttpGet("getAdmin")]
    [Authorize]
    public ActionResult<string> GetAdmin() 
    {
        return "admin";
    }

    [HttpGet("editAdmin")]
    [Authorize("moderator")]
    public ActionResult<string> EditAdmins()
    {
        return "edits admins";
    }
}