using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestAnonymousHub : Hub
{
    public Task<int> DoTest()
    {
        return Task.FromResult(new Random().Next());
    }
}