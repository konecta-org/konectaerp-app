using AutoMapper;
using FinanceService.Dtos;
using FinanceService.Models;
using FinanceService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.Authorization;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(IInvoiceRepository invoiceRepository, IMapper mapper, ILogger<InvoicesController> logger)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PermissionConstants.Finance.InvoicesRead)]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetInvoices([FromQuery] string? status = null, [FromQuery] bool includeLines = false, CancellationToken cancellationToken = default)
        {
            var invoices = await _invoiceRepository.GetAllAsync(status, includeLines, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices));
        }

        [HttpGet("{id:int}", Name = nameof(GetInvoiceById))]
        [Authorize(Policy = PermissionConstants.Finance.InvoicesRead)]
        public async Task<ActionResult<InvoiceResponseDto>> GetInvoiceById(int id, [FromQuery] bool includeLines = true, CancellationToken cancellationToken = default)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id, includeLines, cancellationToken);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<InvoiceResponseDto>(invoice));
        }

        [HttpPost]
        [Authorize(Policy = PermissionConstants.Finance.InvoicesManage)]
        public async Task<ActionResult<InvoiceResponseDto>> CreateInvoice([FromBody] InvoiceUpsertDto request, CancellationToken cancellationToken = default)
        {
            if (await _invoiceRepository.InvoiceNumberExistsAsync(request.InvoiceNumber, null, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.InvoiceNumber), "Invoice number already exists.");
                return ValidationProblem(ModelState);
            }

            var invoice = _mapper.Map<Invoice>(request);
            var lines = request.Lines.Select(lineDto => _mapper.Map<InvoiceLine>(lineDto)).ToList();
            ApplyInvoiceCalculations(invoice, request, lines);

            invoice.Lines = lines;

            await _invoiceRepository.AddAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<InvoiceResponseDto>(invoice);
            return CreatedAtRoute(nameof(GetInvoiceById), new { id = invoice.Id }, response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.InvoicesManage)]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceUpsertDto request, CancellationToken cancellationToken = default)
        {
            var existingInvoice = await _invoiceRepository.GetByIdAsync(id, includeLines: true, cancellationToken);
            if (existingInvoice == null)
            {
                return NotFound();
            }

            if (!string.Equals(existingInvoice.InvoiceNumber, request.InvoiceNumber, StringComparison.OrdinalIgnoreCase) &&
                await _invoiceRepository.InvoiceNumberExistsAsync(request.InvoiceNumber, id, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.InvoiceNumber), "Invoice number already exists.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, existingInvoice);
            var newLines = request.Lines.Select(lineDto => _mapper.Map<InvoiceLine>(lineDto)).ToList();
            ApplyInvoiceCalculations(existingInvoice, request, newLines);

            await _invoiceRepository.UpdateLinesAsync(existingInvoice, newLines, cancellationToken);
            existingInvoice.Lines = newLines;
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.InvoicesManage)]
        public async Task<IActionResult> DeleteInvoice(int id, CancellationToken cancellationToken = default)
        {
            var removed = await _invoiceRepository.DeleteAsync(id, cancellationToken);
            if (!removed)
            {
                return NotFound();
            }

            await _invoiceRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private static void ApplyInvoiceCalculations(Invoice invoice, InvoiceUpsertDto request, List<InvoiceLine> lines)
        {
            foreach (var line in lines)
            {
                line.LineTotal = RoundCurrency(line.Quantity * line.UnitPrice);
            }

            var subtotal = RoundCurrency(lines.Sum(line => line.LineTotal));
            invoice.Subtotal = subtotal;
            invoice.TaxAmount = RoundCurrency(request.TaxAmount);
            invoice.TotalAmount = RoundCurrency(subtotal + invoice.TaxAmount);
            invoice.PaidAmount = RoundCurrency(Math.Clamp(request.PaidAmount, 0, invoice.TotalAmount));
        }

        private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
