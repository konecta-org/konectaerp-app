package com.reporting.ReportingService.service;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

import com.reporting.ReportingService.client.*;
import com.reporting.ReportingService.dto.*;
import com.reporting.ReportingService.util.ExcelReportGenerator;
import com.reporting.ReportingService.util.PdfReportGenerator;

import java.io.ByteArrayInputStream;
import java.time.Instant;

@Service
public class ReportingServiceImpl implements ReportingService {
    private final FinanceClient financeClient;
    private final HrClient hrClient;
    private final InventoryClient inventoryClient;
    private final Logger logger = LoggerFactory.getLogger(ReportingServiceImpl.class);

    public ReportingServiceImpl(FinanceClient financeClient, HrClient hrClient, InventoryClient inventoryClient) {
        this.financeClient = financeClient;
        this.hrClient = hrClient;
        this.inventoryClient = inventoryClient;
    }

    @Override
    public OverviewDto buildOverview() {
        FinanceSummaryDto finance = safeCallFinance();
        HrSummaryDto hr = safeCallHr();
        InventorySummaryDto inventory = safeCallInventory();

        return new OverviewDto(finance, hr, inventory, Instant.now());
    }

    @Override
    public FinanceSummaryDto getFinanceSummary() {
        return safeCallFinance();
    }

    @Override
    public HrSummaryDto getHrSummary() {
        return safeCallHr();
    }

    @Override
    public InventorySummaryDto getInventorySummary() {
        return safeCallInventory();
    }

    @Override
    public ByteArrayInputStream generatePdfReport() {
        OverviewDto overview = buildOverview();
        return PdfReportGenerator.generatePdf(overview);
    }

    @Override
    public ByteArrayInputStream generateExcelReport() {
        OverviewDto overview = buildOverview();
        return ExcelReportGenerator.generateExcel(overview);
    }

    private FinanceSummaryDto safeCallFinance() {
        try {
            return financeClient.getFinanceSummary();
        } catch (Exception ex) {
            logger.warn("Failed to call finance service summary: {}", ex.getMessage());
            return new FinanceSummaryDto(
                    java.math.BigDecimal.ZERO,
                    java.math.BigDecimal.ZERO,
                    java.math.BigDecimal.ZERO,
                    java.math.BigDecimal.ZERO,
                    java.math.BigDecimal.ZERO);
        }
    }

    private HrSummaryDto safeCallHr() {
        try {
            return hrClient.getSummary();
        } catch (Exception ex) {
            logger.warn("Failed to call HR service summary: {}", ex.getMessage());
            return new HrSummaryDto(0, 0, 0, 0);
        }
    }

    private InventorySummaryDto safeCallInventory() {
        try {
            return inventoryClient.getSummary();
        } catch (Exception ex) {
            logger.warn("Failed to call Inventory service summary: {}", ex.getMessage());
            return new InventorySummaryDto(0, 0, java.math.BigDecimal.ZERO, java.math.BigDecimal.ZERO, 0);
        }
    }
}
