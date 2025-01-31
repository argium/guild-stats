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


/// <summary>
/// A way to obtain your current rate limit usage.
/// </summary>
public partial class RateLimitData : GraphQLTypeBase
{
    /// <summary>
    /// The total amount of points this API key can spend per hour.
    /// </summary>
    [JsonPropertyName("limitPerHour")]
    public int LimitPerHour { get; set; }

    /// <summary>
    /// The total amount of points spent during this hour.
    /// </summary>
    [JsonPropertyName("pointsSpentThisHour")]
    public double PointsSpentThisHour { get; set; }

    /// <summary>
    /// The number of seconds remaining until the points reset.
    /// </summary>
    [JsonPropertyName("pointsResetIn")]
    public int PointsResetIn { get; set; }

}
