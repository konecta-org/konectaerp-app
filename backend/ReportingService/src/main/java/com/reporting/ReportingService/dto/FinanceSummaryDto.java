package com.reporting.ReportingService.dto;

import java.math.BigDecimal;

public record FinanceSummaryDto(
    BigDecimal outstandingReceivables,
    BigDecimal overdueReceivables,
    BigDecimal currentMonthExpenseTotal,
    BigDecimal budgetUtilization,
    BigDecimal upcomingPayrollCommitment
) {}
