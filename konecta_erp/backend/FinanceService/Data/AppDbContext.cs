using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<BudgetLine> BudgetLines => Set<BudgetLine>();
        public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
        public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
        public DbSet<EmployeeCompensationAccount> EmployeeCompensationAccounts => Set<EmployeeCompensationAccount>();
        public DbSet<EmployeeBonus> EmployeeBonuses => Set<EmployeeBonus>();
        public DbSet<EmployeeDeduction> EmployeeDeductions => Set<EmployeeDeduction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Invoice>()
                .HasIndex(invoice => invoice.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceLine>()
                .HasOne(line => line.Invoice)
                .WithMany(invoice => invoice.Lines!)
                .HasForeignKey(line => line.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .HasIndex(expense => expense.ExpenseNumber)
                .IsUnique();

            modelBuilder.Entity<Expense>()
                .Property(expense => expense.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Budget>()
                .Property(budget => budget.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Budget>()
                .Property(budget => budget.SpentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Budget>()
                .HasMany(budget => budget.Lines!)
                .WithOne(line => line.Budget)
                .HasForeignKey(line => line.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BudgetLine>()
                .Property(line => line.AllocatedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BudgetLine>()
                .Property(line => line.SpentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollRun>()
                .HasIndex(run => run.PayrollNumber)
                .IsUnique();

            modelBuilder.Entity<PayrollRun>()
                .Property(run => run.TotalGrossPay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollRun>()
                .Property(run => run.TotalNetPay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollRun>()
                .HasMany(run => run.Entries!)
                .WithOne(entry => entry.PayrollRun)
                .HasForeignKey(entry => entry.PayrollRunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PayrollEntry>()
                .Property(entry => entry.GrossPay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollEntry>()
                .Property(entry => entry.NetPay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollEntry>()
                .Property(entry => entry.Deductions)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PayrollEntry>()
                .Property(entry => entry.Taxes)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeCompensationAccount>()
                .HasIndex(account => account.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<EmployeeCompensationAccount>()
                .Property(account => account.BaseSalary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeCompensationAccount>()
                .HasMany(account => account.Bonuses!)
                .WithOne(bonus => bonus.Account)
                .HasForeignKey(bonus => bonus.EmployeeCompensationAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeCompensationAccount>()
                .HasMany(account => account.Deductions!)
                .WithOne(deduction => deduction.Account)
                .HasForeignKey(deduction => deduction.EmployeeCompensationAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeBonus>()
                .Property(bonus => bonus.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EmployeeDeduction>()
                .Property(deduction => deduction.Amount)
                .HasPrecision(18, 2);
        }
    }
}
