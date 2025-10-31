package com.apigateway.apigate.controller;

import java.time.OffsetDateTime;
import java.util.Map;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ServerWebExchange;

import reactor.core.publisher.Mono;


@RestController
@RequestMapping("/fallback")
public class FallbackController {

    @GetMapping("/authentication-service")
    public Mono<ResponseEntity<Map<String, Object>>> authenticationFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("authentication-service", exchange));
    }

    @GetMapping("/hr-service")
    public Mono<ResponseEntity<Map<String, Object>>> hrFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("hr-service", exchange));
    }

    @GetMapping("/user-management-service")
    public Mono<ResponseEntity<Map<String, Object>>> userManagementFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("user-management-service", exchange));
    }

    @GetMapping("/inventory-service")
    public Mono<ResponseEntity<Map<String, Object>>> inventoryFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("inventory-service", exchange));
    }

    @GetMapping("/finance-service")
    public Mono<ResponseEntity<Map<String, Object>>> financeFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("finance-service", exchange));
    }

    @GetMapping("/reporting-service")
    public Mono<ResponseEntity<Map<String, Object>>> reportingFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("reporting-service", exchange));
    }

    @GetMapping("/default")
    public Mono<ResponseEntity<Map<String, Object>>> defaultFallback(ServerWebExchange exchange) {
        return Mono.just(buildFallbackResponse("generic-service", exchange));
    }

    private ResponseEntity<Map<String, Object>> buildFallbackResponse(String service, ServerWebExchange exchange) {
        Map<String, Object> payload = Map.of(
                "timestamp", OffsetDateTime.now().toString(),
                "service", service,
                "path", exchange.getRequest().getURI().getPath(),
                "message", "Service temporarily unavailable. Request served from fallback."
        );
        return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).body(payload);
    }
}
