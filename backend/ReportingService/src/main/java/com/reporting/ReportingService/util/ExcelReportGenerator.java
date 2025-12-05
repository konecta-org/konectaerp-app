package com.reporting.ReportingService.util;

import com.reporting.ReportingService.dto.OverviewDto;
import org.apache.poi.ss.usermodel.*;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;

public class ExcelReportGenerator {

    public static ByteArrayInputStream generateExcel(OverviewDto overview) {
        try (Workbook workbook = new XSSFWorkbook()) {
            Sheet sheet = workbook.createSheet("ERP Overview");

            int rowIdx = 0;
            CellStyle headerStyle = workbook.createCellStyle();
            Font headerFont = workbook.createFont();
            headerFont.setBold(true);
            headerStyle.setFont(headerFont);

            // Finance 
            Row financeHeader = sheet.createRow(rowIdx++);
            financeHeader.createCell(0).setCellValue("Finance Summary");
            financeHeader.getCell(0).setCellStyle(headerStyle);

            Row financeRow = sheet.createRow(rowIdx++);
            financeRow.createCell(0).setCellValue("Outstanding Receivables");
            financeRow.createCell(1).setCellValue(overview.finance().outstandingReceivables().doubleValue());
            financeRow.createCell(2).setCellValue("Overdue Receivables");
            financeRow.createCell(3).setCellValue(overview.finance().overdueReceivables().doubleValue());
            financeRow.createCell(4).setCellValue("Current Month Expense Total");
            financeRow.createCell(5).setCellValue(overview.finance().currentMonthExpenseTotal().doubleValue());
            financeRow.createCell(6).setCellValue("Budget Utilization");
            financeRow.createCell(7).setCellValue(overview.finance().budgetUtilization().doubleValue());
            financeRow.createCell(8).setCellValue("Upcoming Payroll Commitment");
            financeRow.createCell(9).setCellValue(overview.finance().upcomingPayrollCommitment().doubleValue());
            rowIdx++;

            // HR 
            Row hrHeader = sheet.createRow(rowIdx++);
            hrHeader.createCell(0).setCellValue("HR Summary");
            hrHeader.getCell(0).setCellStyle(headerStyle);

            Row hrRow = sheet.createRow(rowIdx++);
            hrRow.createCell(0).setCellValue("Total Employees");
            hrRow.createCell(1).setCellValue(overview.hr().totalEmployees());
            hrRow.createCell(2).setCellValue("Active Employees");
            hrRow.createCell(3).setCellValue(overview.hr().activeEmployees());
            hrRow.createCell(4).setCellValue("Departments");
            hrRow.createCell(5).setCellValue(overview.hr().departments());
            hrRow.createCell(6).setCellValue("Pending Resignations");
            hrRow.createCell(7).setCellValue(overview.hr().pendingResignations());
            rowIdx++;

            // Inventory 
            Row invHeader = sheet.createRow(rowIdx++);
            invHeader.createCell(0).setCellValue("Inventory Summary");
            invHeader.getCell(0).setCellStyle(headerStyle);

            Row invRow = sheet.createRow(rowIdx++);
            invRow.createCell(0).setCellValue("Total Active Items");
            invRow.createCell(1).setCellValue(overview.inventory().totalActiveItems());
            invRow.createCell(2).setCellValue("Total Warehouses");
            invRow.createCell(3).setCellValue(overview.inventory().totalWarehouses());
            invRow.createCell(4).setCellValue("Total Quantity On Hand");
            invRow.createCell(5).setCellValue(overview.inventory().totalQuantityOnHand().doubleValue());
            invRow.createCell(6).setCellValue("Total Quantity Reserved");
            invRow.createCell(7).setCellValue(overview.inventory().totalQuantityReserved().doubleValue());
            invRow.createCell(8).setCellValue("Items Below Safety Stock");
            invRow.createCell(9).setCellValue(overview.inventory().itemsBelowSafetyStock());
            rowIdx++;

            // Generated Timestamp
            Row tsRow = sheet.createRow(rowIdx++);
            tsRow.createCell(0).setCellValue("Generated At (UTC)");
            tsRow.createCell(1).setCellValue(overview.lastUpdatedUtc().toString());

            // Autosize columns for clean output
            for (int i = 0; i < 10; i++) {
                sheet.autoSizeColumn(i);
            }

            ByteArrayOutputStream out = new ByteArrayOutputStream();
            workbook.write(out);
            return new ByteArrayInputStream(out.toByteArray());

        } catch (IOException e) {
            e.printStackTrace();
            return null;
        }
    }
}
