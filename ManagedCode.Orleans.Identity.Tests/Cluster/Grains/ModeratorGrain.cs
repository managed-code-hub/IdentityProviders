﻿using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Tests.Cluster.Grains
{
    [Authorize(Roles = TestRoles.MODERATOR)]
    public class ModeratorGrain : Grain, IModeratorGrain
    {
        public Task<string> GetInfo()
        {
            return Task.FromResult("info");
        }

        [Authorize]
        public Task<string> GetModerators()
        {
            return Task.FromResult("moderators");
        }

        [AllowAnonymous]
        public Task<string> GetPublicInformation()
        {
            return Task.FromResult("public info");
        }
    }
}
