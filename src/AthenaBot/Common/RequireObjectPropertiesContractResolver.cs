﻿#nullable disable
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AthenaBot.Common;

public class RequireObjectPropertiesContractResolver : DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var contract = base.CreateObjectContract(objectType);
        contract.ItemRequired = Required.DisallowNull;
        return contract;
    }
}