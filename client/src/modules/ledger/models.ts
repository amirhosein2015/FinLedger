export interface AccountBalance {
  id: string; // The Guid ID
  accountCode: string;
  accountName: string;
  totalDebit: number;
  totalCredit: number;
  balance: number;
}

export interface JournalEntryLineRequest {
  accountId: string;
  debit: number;
  credit: number;
}

export interface CreateJournalEntryRequest {
  description: string;
  transactionDate: string;
  lines: JournalEntryLineRequest[];
}

export interface AuditLog {
  id: string;
  userId: string;
  action: string;
  entityName: string;
  changes: string;
  occurredOnUtc: string;
}

