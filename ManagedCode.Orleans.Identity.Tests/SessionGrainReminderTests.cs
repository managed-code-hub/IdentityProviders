using FluentAssertions;
using ManagedCode.Orleans.Identity.Interfaces;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests;

[Collection(nameof(TestClusterApplication))]
public class SessionGrainReminderTests 
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly TestClusterApplication _testApp;

    private CreateSessionModel GetTestCreateSessionModel(string sessionId)
    {
        var userId = Guid.NewGuid().ToString();

        var userGrainId = GrainId.Create("UserGrain", userId);

        var createSessionModel = new CreateSessionModel
        {
            UserData = SessionHelper.SetTestClaims(sessionId),
            UserGrainId = userGrainId
        };

        return createSessionModel;
    }
    
    public SessionGrainReminderTests(ITestOutputHelper outputHelper, TestClusterApplication testApp)
    {
        _outputHelper = outputHelper;
        _testApp = testApp;
    }

    [Fact]
    public async Task CloseSession_WhenSessionIsNotDeleted_RegisterAndReceiveReminder()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var createSessionModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
        TestSiloOptions.SessionOption.SessionLifetime = TimeSpan.FromMinutes(2);
        TestSiloOptions.SessionOption.ClearStateOnClose = false;
        await sessionGrain.CloseAsync();

        // Act
        await Task.Delay(TimeSpan.FromMinutes(2));
        var sessionResult = await sessionGrain.GetSessionAsync();
        
        // Assert
        sessionResult.IsSuccess.Should().BeFalse();
        sessionResult.IsFailed.Should().BeTrue();

        TestSiloOptions.SessionOption.ClearStateOnClose = true;
        TestSiloOptions.SessionOption.SessionLifetime = TimeSpan.FromDays(20);
    }

    [Fact]
    public async Task CloseSession_WhenDeleteSessionOnCloseIsTrue_DoNotRegisterReminder()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var createSessionModel = GetTestCreateSessionModel(sessionId);
        var sessionGrain = _testApp.Cluster.Client.GetGrain<ISessionGrain>(sessionId);
        await sessionGrain.CreateAsync(createSessionModel);
        await sessionGrain.CloseAsync();

        // Act
        var sessionResult = await sessionGrain.GetSessionAsync();

        // Assert
        sessionResult.IsSuccess.Should().BeFalse();
        sessionResult.IsFailed.Should().BeTrue();
    }
}