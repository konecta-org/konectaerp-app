package com.reporting.ReportingService.dto;

public record HrSummaryDto(
    int totalEmployees,
    int activeEmployees,
    int departments,
    int pendingResignations
) {}
