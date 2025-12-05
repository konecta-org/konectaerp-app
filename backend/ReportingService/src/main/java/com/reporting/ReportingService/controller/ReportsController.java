package com.reporting.ReportingService.controller;

import java.io.ByteArrayInputStream;

import org.springframework.core.io.InputStreamResource;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.reporting.ReportingService.dto.*;
import com.reporting.ReportingService.service.ReportingService;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;

@RestController
@RequestMapping("/api/reports")
public class ReportsController {
    private final ReportingService reportingService;

    public ReportsController(ReportingService reportingService) {
        this.reportingService = reportingService;
    }

    @GetMapping("/overview")
    public ResponseEntity<OverviewDto> getOverview() {
        OverviewDto dto = reportingService.buildOverview();
        return ResponseEntity.ok(dto);
    }

    @GetMapping("/finance")
    public ResponseEntity<FinanceSummaryDto> getFinanceSummary() {
        return ResponseEntity.ok(reportingService.getFinanceSummary());
    }

    @GetMapping("/hr")
    public ResponseEntity<HrSummaryDto> getHrSummary() {
        return ResponseEntity.ok(reportingService.getHrSummary());
    }

    @GetMapping("/inventory")
    public ResponseEntity<InventorySummaryDto> getInventorySummary() {
        return ResponseEntity.ok(reportingService.getInventorySummary());
    }

    // PDF
    @GetMapping("/export/pdf")
    public ResponseEntity<InputStreamResource> exportPdf() {
        ByteArrayInputStream bis = reportingService.generatePdfReport();

        HttpHeaders headers = new HttpHeaders();
        headers.add("Content-Disposition", "inline; filename=ERP_Report.pdf");

        return ResponseEntity.ok()
                .headers(headers)
                .contentType(MediaType.APPLICATION_PDF)
                .body(new InputStreamResource(bis));
    }

    // Excel
    @GetMapping("/export/excel")
    public ResponseEntity<InputStreamResource> exportExcel() {
        ByteArrayInputStream bis = reportingService.generateExcelReport();

        HttpHeaders headers = new HttpHeaders();
        headers.add("Content-Disposition", "attachment; filename=ERP_Report.xlsx");

        return ResponseEntity.ok()
                .headers(headers)
                .contentType(
                        MediaType.parseMediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                .body(new InputStreamResource(bis));
    }

    // @GetMapping("/export/pdf")
    // public ResponseEntity<InputStreamResource> exportPdf() {
    // OverviewDto overview = reportingService.buildOverview();
    // ByteArrayInputStream bis = PdfReportGenerator.generatePdf(overview);

    // HttpHeaders headers = new HttpHeaders();
    // headers.add("Content-Disposition", "inline; filename=ERP_Report.pdf");

    // return ResponseEntity.ok()
    // .headers(headers)
    // .contentType(MediaType.APPLICATION_PDF)
    // .body(new InputStreamResource(bis));
    // }

    // @GetMapping("/export/excel")
    // public ResponseEntity<InputStreamResource> exportExcel() {
    // OverviewDto overview = reportingService.buildOverview();
    // ByteArrayInputStream bis = ExcelReportGenerator.generateExcel(overview);

    // HttpHeaders headers = new HttpHeaders();
    // headers.add("Content-Disposition", "attachment; filename=ERP_Report.xlsx");

    // return ResponseEntity.ok()
    // .headers(headers)
    // .contentType(
    // MediaType.parseMediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
    // .body(new InputStreamResource(bis));
    // }
}
