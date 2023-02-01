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
    public class BaseUserGrainsTests<TTokenGrain, TUserGrain>
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

        protected async Task<TTokenGrain> CreateTokenAsync()
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel();
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(CreateTokenModel createTokenModel)
        {
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(string tokenValue)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(tokenValue);
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(string tokenValue, TimeSpan timeSpan)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(tokenValue, timeSpan);
            var tokenGrain = await CreateAndGetTokenGrainAsync(createTokenModel);
            return tokenGrain;
        }

        protected async Task<TTokenGrain> CreateTokenAsync(TimeSpan timeSpan)
        {
            var createTokenModel = TokenHelper.GenerateCreateTestTokenModel(timeSpan);
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

        #endregion
    }

}
