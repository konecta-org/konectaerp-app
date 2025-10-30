package com.reporting.ReportingService.dto;

import java.math.BigDecimal;

public record InventorySummaryDto(
    int totalActiveItems,
    int totalWarehouses,
    BigDecimal totalQuantityOnHand,
    BigDecimal totalQuantityReserved,
    int itemsBelowSafetyStock
) {}
