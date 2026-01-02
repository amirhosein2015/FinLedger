import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { ledgerApi } from '../api';
import { useTenant } from '../../../shared/hooks/useTenant';
import { LayoutDashboard, Users, LogOut, BarChart3, History, Shield } from 'lucide-react';
import { CreateEntryPage } from './CreateEntryPage'; 

export const DashboardPage: React.FC = () => {
  const { tenantId, switchTenant } = useTenant();

  // Query 1: Real-time Account Balances
  const { data: balances, isLoading: isBalancesLoading } = useQuery({ 
    queryKey: ['balances', tenantId], 
    queryFn: ledgerApi.getBalances 
  });

  // Query 2: Live Audit Trail (System Logs)
  //  Correlating UI state with backend audit trails for full transparency
  const { data: logs, isLoading: isLogsLoading } = useQuery({ 
    queryKey: ['logs', tenantId], 
    queryFn: ledgerApi.getAuditLogs,
    refetchInterval: 5000 // Automatically refresh every 5 seconds to show live activity
  });

  return (
    <div style={{ padding: '30px', maxWidth: '1100px', margin: 'auto', fontFamily: 'Inter, system-ui, sans-serif' }}>
      
      {/* Header Section */}
      <header style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '40px', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <div style={{ background: '#2563eb', padding: '8px', borderRadius: '8px' }}><BarChart3 color="white" /></div>
          <h1 style={{ margin: 0, fontSize: '1.5rem', fontWeight: 700 }}>FinLedger SaaS</h1>
        </div>
        <div style={{ display: 'flex', gap: '15px' }}>
          <select 
            value={tenantId} 
            onChange={(e) => switchTenant(e.target.value)}
            style={{ padding: '6px 12px', borderRadius: '6px', border: '1px solid #ccc' }}
          >
            <option value="amsterdam_hq">Amsterdam HQ (Active)</option>
          </select>
          <button 
            onClick={() => {localStorage.clear(); window.location.reload();}} 
            style={{ padding: '8px 15px', borderRadius: '6px', color: '#dc2626', border: '1px solid #dc2626', background: 'none', cursor: 'pointer' }}
          >
            <LogOut size={16} style={{ verticalAlign: 'middle', marginRight: '5px' }} /> Sign Out
          </button>
        </div>
      </header>

      <main>
        {/* Section 1: Financial Report */}
        <section style={{ marginBottom: '50px' }}>
          <h2 style={{ fontSize: '1.1rem', marginBottom: '20px', color: '#475569', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <BarChart3 size={20} /> Real-time Asset Balances
          </h2>
          <table style={{ width: '100%', borderCollapse: 'collapse', borderRadius: '12px', overflow: 'hidden', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}>
            <thead style={{ background: '#f1f5f9' }}>
              <tr>
                <th style={{ padding: '15px', textAlign: 'left' }}>Account</th>
                <th style={{ padding: '15px', textAlign: 'right' }}>Net Balance</th>
              </tr>
            </thead>
            <tbody>
              {balances?.map(a => (
                <tr key={a.accountCode} style={{ borderBottom: '1px solid #f1f5f9', background: 'white' }}>
                  <td style={{ padding: '15px' }}>{a.accountCode} - {a.accountName}</td>
                  <td style={{ padding: '15px', textAlign: 'right', fontWeight: 600, color: a.balance >= 0 ? '#16a34a' : '#dc2626' }}>
                    {a.balance.toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        {/* Section 2: Data Entry (Transaction Form) */}
        <section style={{ marginBottom: '50px' }}>
          <CreateEntryPage accounts={balances} />
        </section>

        {/* Section 3: Live System Accountability (Audit Logs) */}
        <section style={{ padding: '25px', background: 'white', borderRadius: '12px', border: '1px solid #e2e8f0', boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.1)' }}>
          <h2 style={{ fontSize: '1.1rem', marginBottom: '20px', color: '#475569', display: 'flex', alignItems: 'center', gap: '8px', marginTop: 0 }}>
            <History size={20} color="#2563eb" /> Immutable Audit Trail (The Ledger's Memory)
          </h2>
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {logs?.slice(0, 5).map((log) => (
              <div key={log.id} style={{ padding: '15px', background: '#f8fafc', borderRadius: '8px', borderLeft: '4px solid #2563eb' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '5px' }}>
                  <strong style={{ fontSize: '0.85rem', color: '#1e293b' }}>
                    <Shield size={14} style={{ marginRight: '5px' }} /> 
                    Action: {log.action} on {log.entityName}
                  </strong>
                  <span style={{ fontSize: '0.75rem', color: '#64748b' }}>
                    {new Date(log.occurredOnUtc).toLocaleTimeString()}
                  </span>
                </div>
                <div style={{ fontSize: '0.8rem', color: '#475569', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  User: {log.userId} | Data: {log.changes}
                </div>
              </div>
            ))}
            
            {(!logs || logs.length === 0) && (
              <div style={{ textAlign: 'center', padding: '20px', color: '#94a3b8', fontStyle: 'italic' }}>
                No database activity recorded yet.
              </div>
            )}
          </div>
        </section>

      </main>
    </div>
  );
};
