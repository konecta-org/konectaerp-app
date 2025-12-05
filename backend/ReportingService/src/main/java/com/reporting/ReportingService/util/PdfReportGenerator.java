package com.reporting.ReportingService.util;

import com.itextpdf.text.*;
import com.itextpdf.text.pdf.*;
import com.reporting.ReportingService.dto.OverviewDto;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;

public class PdfReportGenerator {

    public static ByteArrayInputStream generatePdf(OverviewDto overview) {
        Document document = new Document();
        ByteArrayOutputStream out = new ByteArrayOutputStream();

        try {
            PdfWriter.getInstance(document, out);
            document.open();

            // Title
            Font titleFont = FontFactory.getFont(FontFactory.HELVETICA_BOLD, 18);
            Paragraph title = new Paragraph("ERP System Overview Report", titleFont);
            title.setAlignment(Element.ALIGN_CENTER);
            document.add(title);
            document.add(Chunk.NEWLINE);

            // Finance Summary
            addSectionHeader(document, "Finance Summary");
            document.add(new Paragraph("Outstanding Receivables: " + overview.finance().outstandingReceivables()));
            document.add(new Paragraph("Overdue Receivables: " + overview.finance().overdueReceivables()));
            document.add(new Paragraph("Current Month Expense Total: " + overview.finance().currentMonthExpenseTotal()));
            document.add(new Paragraph("Budget Utilization: " + overview.finance().budgetUtilization()));
            document.add(new Paragraph("Upcoming Payroll Commitment: " + overview.finance().upcomingPayrollCommitment()));
            document.add(Chunk.NEWLINE);

            // HR Summary
            addSectionHeader(document, "HR Summary");
            document.add(new Paragraph("Total Employees: " + overview.hr().totalEmployees()));
            document.add(new Paragraph("Active Employees: " + overview.hr().activeEmployees()));
            document.add(new Paragraph("Departments: " + overview.hr().departments()));
            document.add(new Paragraph("Pending Resignations: " + overview.hr().pendingResignations()));
            document.add(Chunk.NEWLINE);

            // Inventory Summary
            addSectionHeader(document, "Inventory Summary");
            document.add(new Paragraph("Total Active Items: " + overview.inventory().totalActiveItems()));
            document.add(new Paragraph("Total Warehouses: " + overview.inventory().totalWarehouses()));
            document.add(new Paragraph("Total Quantity On Hand: " + overview.inventory().totalQuantityOnHand()));
            document.add(new Paragraph("Total Quantity Reserved: " + overview.inventory().totalQuantityReserved()));
            document.add(new Paragraph("Items Below Safety Stock: " + overview.inventory().itemsBelowSafetyStock()));
            document.add(Chunk.NEWLINE);

            // Timestamp
            document.add(new Paragraph("Generated At (UTC): " + overview.lastUpdatedUtc()));

            document.close();
        } catch (DocumentException e) {
            e.printStackTrace();
        }

        return new ByteArrayInputStream(out.toByteArray());
    }

    private static void addSectionHeader(Document document, String headerText) throws DocumentException {
        Font headerFont = FontFactory.getFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLUE);
        Paragraph header = new Paragraph(headerText, headerFont);
        header.setSpacingBefore(10);
        header.setSpacingAfter(5);
        document.add(header);
    }
}
