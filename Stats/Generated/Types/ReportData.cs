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


public static class ReportDataExtensions
{
    [GraphMethod("report")]
    public static Report Report(this ReportData  reportData, [GraphArgument("String")] string code = null)
    {
        return reportData.GetMethodValue<Report>("report", code);
    }

    [GraphMethod("reports")]
    public static ReportPagination Reports(this ReportData  reportData, [GraphArgument("Float")] double? endTime = null, [GraphArgument("Int")] int? guildID = null, [GraphArgument("String")] string guildName = null, [GraphArgument("String")] string guildServerSlug = null, [GraphArgument("String")] string guildServerRegion = null, [GraphArgument("Int")] int? guildTagID = null, [GraphArgument("Int")] int? userID = null, [GraphArgument("Int")] int? limit = null, [GraphArgument("Int")] int? page = null, [GraphArgument("Float")] double? startTime = null, [GraphArgument("Int")] int? zoneID = null, [GraphArgument("Int")] int? gameZoneID = null)
    {
        return reportData.GetMethodValue<ReportPagination>("reports", endTime, guildID, guildName, guildServerSlug, guildServerRegion, guildTagID, userID, limit, page, startTime, zoneID, gameZoneID);
    }

}

/// <summary>
/// The ReportData object enables the retrieval of single reports or filtered collections of reports.
/// </summary>
public partial class ReportData : GraphQLTypeBase
{
    private LazyProperty<Report> _report = new();
    /// <summary>
    /// Do not use in Query, only to retrive result
    /// </summary>
    [GraphShadowProperty]
    public Report Report => _report.Value(() => GetFirstMethodValue<Report>("report"));

    private LazyProperty<ReportPagination> _reports = new();
    /// <summary>
    /// Do not use in Query, only to retrive result
    /// </summary>
    [GraphShadowProperty]
    public ReportPagination Reports => _reports.Value(() => GetFirstMethodValue<ReportPagination>("reports"));

}
