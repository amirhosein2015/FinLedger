import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { ledgerApi } from '../api';
import { useTenant } from '../../../shared/hooks/useTenant';
import { LayoutDashboard, Users, LogOut, BarChart3 } from 'lucide-react';
import { CreateEntryPage } from './CreateEntryPage'; 

export const DashboardPage: React.FC = () => {
  const { tenantId, switchTenant } = useTenant();
  const { data: balances, isLoading } = useQuery({ queryKey: ['balances', tenantId], queryFn: ledgerApi.getBalances });

  return (
    <div style={{ padding: '30px', maxWidth: '1100px', margin: 'auto', fontFamily: 'Inter, system-ui, sans-serif' }}>
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




          <button onClick={() => {localStorage.clear(); window.location.reload();}} style={{ padding: '8px 15px', borderRadius: '6px', color: '#dc2626', border: '1px solid #dc2626', background: 'none', cursor: 'pointer' }}>Sign Out</button>
        </div>
      </header>

      <main>
        <section style={{ marginBottom: '50px' }}>
          <h2 style={{ fontSize: '1.1rem', marginBottom: '20px', color: '#475569' }}>Real-time Asset Balances</h2>
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
                  <td style={{ padding: '15px', textAlign: 'right', fontWeight: 600, color: a.balance >= 0 ? '#16a34a' : '#dc2626' }}>{a.balance.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        <section>
          {/* Passing the balances to the form to populate dropdowns automatically */}
          <CreateEntryPage accounts={balances} />
        </section>
      </main>
    </div>
  );
};
