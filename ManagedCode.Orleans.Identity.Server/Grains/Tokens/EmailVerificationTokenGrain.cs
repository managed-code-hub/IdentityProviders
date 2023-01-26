using System;
using System.Threading.Tasks;
using ManagedCode.Communication;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces.TokenGrains;
using ManagedCode.Orleans.Identity.Models;
using ManagedCode.Orleans.Identity.Server.Constants;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Server.Grains.Tokens;

public class EmailVerificationTokenGrain : Grain, IEmailVerificationTokenGrain, IRemindable
{
    private readonly IPersistentState<TokenModel> _tokenState;

    public EmailVerificationTokenGrain(
        [PersistentState("emailVerificationToken", OrleansIdentityConstants.TOKEN_STORAGE_NAME)]
        IPersistentState<TokenModel> tokenState)
    {
        _tokenState = tokenState;
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
        // TODO: add to the method description that if token lifetime less than 1 minute, timer with period 1 minute will be registered 
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
            await this.RegisterOrUpdateReminder(TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME, _tokenState.State.Lifetime, _tokenState.State.Lifetime);
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

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (_tokenState.RecordExists is false)
        {
            DeactivateOnIdle();
            return;
        }

        if (reminderName == TokenGrainConstants.EMAIL_VERIFICATION_TOKEN_REMINDER_NAME)
        {
            await _tokenState.ClearStateAsync();
        }
    }
}