package com.reporting.ReportingService.client;

import org.springframework.cloud.openfeign.FeignClient;
import org.springframework.web.bind.annotation.GetMapping;

import com.reporting.ReportingService.dto.FinanceSummaryDto;

@FeignClient(name = "financeClient", url = "${services.finance.url}")
public interface FinanceClient {
    @GetMapping("/api/finance-summary")
    FinanceSummaryDto getFinanceSummary();
}
