//---------------------------------------------------------------------
// This code was automatically generated by Linq2GraphQL
// Please don't edit this file
// Github:https://github.com/linq2graphql/linq2graphql.client
// Url: https://linq2graphql.com
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Linq2GraphQL.Client;
using Linq2GraphQL.Client.Common;

namespace WarcraftLogs.Public;


public partial class Rank : GraphQLTypeBase
{
    /// <summary>
    /// The ordinal rank (usually written "Rank N"). Rank 1 = highest.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// The percentile of the rank as an integer in [0, 100]. Always null for guild ranks.
    /// </summary>
    [JsonPropertyName("percentile")]
    public int? Percentile { get; set; }

    /// <summary>
    /// The color class used by the site for this rank.
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; }

}
