﻿using FluentAssertions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Tests.Cluster;
using ManagedCode.Orleans.Identity.Tests.Cluster.Grains.Interfaces.UserGrains;
using ManagedCode.Orleans.Identity.Tests.Helpers;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Orleans.Identity.Tests.TokenGrainTests.UserGrainTests
{
    [Collection(nameof(TestClusterApplication))]
    public abstract class BaseUserGrainsTests<TTokenGrain, TUserGrain>
        where TTokenGrain : IBaseTokenGrain
        where TUserGrain : IBaseTestUserGrain
    {
        protected readonly ITestOutputHelper _outputHelper;
        protected readonly TestClusterApplication _testApp;

        public BaseUserGrainsTests(ITestOutputHelper outputHelper, TestClusterApplication testApp)
        {
            _outputHelper = outputHelper;
            _testApp = testApp;
        }

        #region Create TokenGrain methods

        private async ValueTask<TTokenGrain> CreateAndGetTokenGrainAsync(CreateTokenModel createTokenModel)
        {
            var tokenGrain = _testApp.Cluster.Client.GetGrain<TTokenGrain>(createTokenModel.Value);
            await tokenGrain.CreateAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(CreateTokenModel createTokenModel)
        {
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }


        #endregion

        #region CreateToken_VerifyToken

        [Fact]
        public virtual async Task VerifyToken_WhenTokenIsNotExpired_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            var userGrainId = createTokenModel.UserGrainId.Key.ToString();
            await tokenGrain.VerifyAsync();
            var userGrain = _testApp.Cluster.Client.GetGrain<TUserGrain>(userGrainId);

            // Act
            var result = await userGrain.IsTokenValid();
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public virtual async Task VerifyToken_WhenTokenExpired_ReturnTrue()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromSeconds(40));
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            var userGrainId = createTokenModel.UserGrainId.Key.ToString();
            var userGrain = _testApp.Cluster.Client.GetGrain<TUserGrain>(userGrainId);

            await Task.Delay(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(20)));

            // Act
            var result = await userGrain.IsTokenExpired();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public virtual async Task VerifyToken_WhenTokenDoesNotExists_ReturnFail()
        {
            // Arrange
            var tokenValue = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var tokenGrain = _testApp.Cluster.GrainFactory.GetGrain<TTokenGrain>(tokenValue);
            var userGrain = _testApp.Cluster.GrainFactory.GetGrain<TUserGrain>(userId);
            
            // Act
            await tokenGrain.VerifyAsync();
            var result = await userGrain.IsTokenValid();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public virtual async Task VerifyToken_WhenUserGrainIsDefault_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            createTokenModel.UserGrainId = new GrainId();
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            
            // Act
            var result = await tokenGrain.VerifyAsync();
            
            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        #endregion

        #region NotifyUserGrain_WhenTokenIsExpired

        [Fact]
        public virtual async Task NotifyUserGrain_WhenTokenExpiredWithReminder_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(30)));
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            var userGrain = _testApp.Cluster.GrainFactory.GetGrain<TUserGrain>(createTokenModel.UserGrainId.Key.ToString());

            // Act
            await Task.Delay(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(34)));
            var result = await userGrain.IsTokenExpired();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public virtual async Task NotifyUserGrain_WhenTokenExpiredWithTimer_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromSeconds(30));
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            var userGrain = _testApp.Cluster.GrainFactory.GetGrain<TUserGrain>(createTokenModel.UserGrainId.Key.ToString());

            // Act
            await Task.Delay(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(10)));
            var result = await userGrain.IsTokenExpired();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task NotifyUserGrain_WhenTokenExpiredAndUserGrainIdIsDefault_ReturnSuccess()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            createTokenModel.UserGrainId = new GrainId();
            var tokenGrain = await CreateTokenAsync(createTokenModel);

            // Act
            await Task.Delay(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(10)));
            var result = await tokenGrain.VerifyAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public virtual async Task NotifyUserGrainWithTimer_WhenTokenExpiredAndUserGrainIdIsDefault_ReturnFail()
        {
            // Arrange
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(TimeSpan.FromSeconds(30));
            createTokenModel.UserGrainId = new GrainId();
            var tokenGrain = await CreateTokenAsync(createTokenModel);
            
            // Act
            await Task.Delay(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(10)));
            var result = await tokenGrain.VerifyAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailed.Should().BeTrue();
        }
        
        #endregion
    }

}
