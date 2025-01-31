//---------------------------------------------------------------------
// This code was automatically generated by Linq2GraphQL
// Please don't edit this file
// Github:https://github.com/linq2graphql/linq2graphql.client
// Url: https://linq2graphql.com
//---------------------------------------------------------------------

using Linq2GraphQL.Client;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WarcraftLogs.Public;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum HardModeLevelRankFilter
{
    [EnumMember(Value = "Any")]
    Any,
    [EnumMember(Value = "Highest")]
    Highest,
    [EnumMember(Value = "NormalMode")]
    Normalmode,
    [EnumMember(Value = "Level0")]
    Level0,
    [EnumMember(Value = "Level1")]
    Level1,
    [EnumMember(Value = "Level2")]
    Level2,
    [EnumMember(Value = "Level3")]
    Level3,
    [EnumMember(Value = "Level4")]
    Level4,
}