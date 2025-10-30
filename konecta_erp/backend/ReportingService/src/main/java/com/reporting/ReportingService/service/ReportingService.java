package com.reporting.ReportingService.service;

import java.io.ByteArrayInputStream;

import com.reporting.ReportingService.dto.FinanceSummaryDto;
import com.reporting.ReportingService.dto.HrSummaryDto;
import com.reporting.ReportingService.dto.InventorySummaryDto;
import com.reporting.ReportingService.dto.OverviewDto;

public interface ReportingService {
    OverviewDto buildOverview();

    FinanceSummaryDto getFinanceSummary();

    HrSummaryDto getHrSummary();

    InventorySummaryDto getInventorySummary();

    ByteArrayInputStream generatePdfReport();

    ByteArrayInputStream generateExcelReport();
}
