using System;
using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Models;

[GenerateSerializer]
public class CreateTokenModel
{
    [Id(0)]
    public string Value { get; set; }

    [Id(1)]
    public GrainId UserGrainId { get; set; }

    [Id(2)]
    public TimeSpan Lifetime { get; set; }
}