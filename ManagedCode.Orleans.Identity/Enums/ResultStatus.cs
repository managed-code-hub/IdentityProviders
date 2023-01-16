﻿using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace ManagedCode.Orleans.Identity.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultStatus
{
    Unknown,
    PropertyWithThisKeyAlreadyExists,
    PropertyWithThisKeyDoesNotExist
}