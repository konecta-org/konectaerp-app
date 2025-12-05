package com.reporting.ReportingService.client;

import com.reporting.ReportingService.dto.InventorySummaryDto;
import org.springframework.cloud.openfeign.FeignClient;
import org.springframework.web.bind.annotation.GetMapping;

@FeignClient(name = "inventoryClient", url = "${services.inventory.url}")
public interface InventoryClient {
    @GetMapping("/api/inventory-summary")
    InventorySummaryDto getSummary();
}
