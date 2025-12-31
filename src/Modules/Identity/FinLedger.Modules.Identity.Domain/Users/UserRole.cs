namespace FinLedger.Modules.Identity.Domain.Users;

public enum UserRole
{
    Admin = 1,      // System/Tenant Owner
    Accountant = 2, // Can create journal entries
    Auditor = 3     // Read-only access to reports
}
