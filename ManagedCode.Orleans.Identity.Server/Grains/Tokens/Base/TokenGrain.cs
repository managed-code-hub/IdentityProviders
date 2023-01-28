﻿using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Constants;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens.Base
{
    public abstract class TokenGrain : Grain, IBaseTokenGrain, IRemindable
    {
        private readonly string _reminderName;
        protected readonly IPersistentState<TokenModel> _tokenState;
        
        protected TokenGrain(IPersistentState<TokenModel> tokenState, string reminderName)
        {
            _tokenState = tokenState;
            _reminderName = reminderName;
        }
        
        private async Task OnTimerTicked(object args)
        {
            if (_tokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return;
            }

            // TODO: also notify UserGrain if token is expired
            DeactivateOnIdle();
            await _tokenState.ClearStateAsync();
        }

        public async ValueTask<Result> CreateAsync(CreateTokenModel createModel)
        {
            if (createModel.IsModelValid() is false)
            {
                DeactivateOnIdle();
                return Result.Fail();
            }

            _tokenState.State = new TokenModel
            {
                Lifetime = createModel.Lifetime,
                UserGrainId = createModel.UserGrainId,
                Value = createModel.Value,
            };

            await _tokenState.WriteStateAsync();

            if (createModel.Lifetime < TimeSpan.FromMinutes(1))
            {
                RegisterTimer(OnTimerTicked, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }
            else
            {
                await this.RegisterOrUpdateReminder(_reminderName, _tokenState.State.Lifetime, _tokenState.State.Lifetime);
            }

            return Result.Succeed();
        }


        public ValueTask<Result> VerifyAsync()
        {
            if (_tokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return Result.Fail().AsValueTask();    
            }

            return Result.Succeed().AsValueTask();
        }

        public ValueTask<Result<TokenModel>> GetTokenAsync()
        {
            if (_tokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return Result<TokenModel>.Fail().AsValueTask();
            }

            return Result<TokenModel>.Succeed(_tokenState.State).AsValueTask();
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (_tokenState.RecordExists is false)
            {
                DeactivateOnIdle();
                return;
            }

            if (reminderName == _reminderName)
            {
                await _tokenState.ClearStateAsync();
            }
        }
    }
}