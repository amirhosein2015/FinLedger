import apiClient from '../../api/client';
import type { AccountBalance, AuditLog, CreateJournalEntryRequest } from './models';

// Centralized financial API services with strict typing
export const ledgerApi = {
  getBalances: async (): Promise<AccountBalance[]> => {
    const response = await apiClient.get<AccountBalance[]>('/ledger/Reports/balances');
    return response.data;
  },

  getAuditLogs: async (): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>('/ledger/Reports/audit-logs');
    return response.data;
  },

  // Added this missing method to fix your error
  createEntry: async (data: CreateJournalEntryRequest): Promise<void> => {
    await apiClient.post('/ledger/Entries', data);
  }
};
