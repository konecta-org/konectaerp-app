package com.reporting.ReportingService.dto;

import java.time.Instant;

public record OverviewDto(
    FinanceSummaryDto finance,
    HrSummaryDto hr,
    InventorySummaryDto inventory,
    Instant lastUpdatedUtc
) {}
