package com.reporting.ReportingService.client;

import org.springframework.cloud.openfeign.FeignClient;
import org.springframework.web.bind.annotation.GetMapping;

import com.reporting.ReportingService.dto.HrSummaryDto;

@FeignClient(name = "hrClient", url = "${services.hr.url}")
public interface HrClient {
    @GetMapping("/api/hr-summary")
    HrSummaryDto getSummary();
}
