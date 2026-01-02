import React, { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ledgerApi } from '../api';
// Using type-only import to satisfy strict compiler 'verbatimModuleSyntax'
import type { AccountBalance } from '../models'; 
import { Plus, Info } from 'lucide-react';

interface Props { accounts: AccountBalance[] | undefined; }

export const CreateEntryPage: React.FC<Props> = ({ accounts }) => {
  const queryClient = useQueryClient();
  const [description, setDescription] = useState('Monthly Operational Entry');
  
  // Initialize with two empty lines for double-entry
  const [lines, setLines] = useState([
    { accountId: '', debit: 0, credit: 0 },
    { accountId: '', debit: 0, credit: 0 }
  ]);





  const { mutate, isPending } = useMutation({
    mutationFn: ledgerApi.createEntry,
    onSuccess: () => {
      alert('✅ Transaction successfully posted!');
      queryClient.invalidateQueries({ queryKey: ['balances'] });
      setLines([{ accountId: '', debit: 0, credit: 0 }, { accountId: '', debit: 0, credit: 0 }]);
    },
    onError: (err: any) => {
      //  Displaying the actual error from the backend for faster debugging
      const errorMessage = err.response?.data?.detail || err.message || 'Unknown Error';
      alert(`❌ Transaction Failed: ${errorMessage}`);
    }
  });




  // Calculate totals for real-time balancing validation
  const totalDebit = lines.reduce((sum, l) => sum + l.debit, 0);
  const totalCredit = lines.reduce((sum, l) => sum + l.credit, 0);
  const isBalanced = totalDebit === totalCredit && totalDebit > 0 && lines.every(l => l.accountId !== '');

  const handleSave = () => { 
    if (isBalanced) {
      mutate({ 
        description, 
        transactionDate: new Date().toISOString(), 
        lines 
      }); 
    }
  };

  return (
    <div style={{ padding: '25px', background: '#f8fafc', borderRadius: '12px', border: '1px solid #e2e8f0', boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.1)' }}>
      <h3 style={{ display: 'flex', alignItems: 'center', gap: '8px', marginTop: 0, color: '#1e293b' }}>
        <Plus size={20} color="#2563eb"/> Create New Journal Entry
      </h3>
      
      {/* 1. Entry Description */}
      <div style={{ marginBottom: '20px' }}>
        <label style={{ display: 'block', marginBottom: '5px', fontSize: '0.85rem', fontWeight: 600, color: '#64748b' }}>Description</label>
        <input 
          type="text" 
          placeholder="e.g., Office Rent Payment"
          value={description} 
          onChange={e => setDescription(e.target.value)} 
          style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #cbd5e1', outlineColor: '#2563eb' }} 
        />
      </div>

      {/* 2. Column Headers for Clarity */}
      <div style={{ display: 'flex', gap: '10px', marginBottom: '8px', padding: '0 5px' }}>
        <div style={{ flex: 2, fontSize: '0.75rem', fontWeight: 700, color: '#475569', textTransform: 'uppercase' }}>Select Account</div>
        <div style={{ flex: 1, fontSize: '0.75rem', fontWeight: 700, color: '#475569', textTransform: 'uppercase' }}>Debit (Incoming +)</div>
        <div style={{ flex: 1, fontSize: '0.75rem', fontWeight: 700, color: '#475569', textTransform: 'uppercase' }}>Credit (Outgoing -)</div>
      </div>
      
      {/* 3. Transaction Lines */}
      {lines.map((line, i) => (
        <div key={i} style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
          <select 
            value={line.accountId} 
            onChange={e => {const n=[...lines]; n[i].accountId=e.target.value; setLines(n);}} 
            style={{ flex: 2, padding: '10px', borderRadius: '6px', border: '1px solid #cbd5e1', background: 'white' }}
          >
            <option value="">-- Choose Account --</option>
            {accounts?.map(a => <option key={a.id} value={a.id}>{a.accountCode} - {a.accountName}</option>)}
          </select>
          <input 
            type="number" 
            placeholder="0.00"
            value={line.debit === 0 ? '' : line.debit}
            onChange={e => {const n=[...lines]; n[i].debit=Number(e.target.value); setLines(n);}} 
            style={{ flex: 1, padding: '10px', borderRadius: '6px', border: '1px solid #cbd5e1' }} 
          />
          <input 
            type="number" 
            placeholder="0.00"
            value={line.credit === 0 ? '' : line.credit}
            onChange={e => {const n=[...lines]; n[i].credit=Number(e.target.value); setLines(n);}} 
            style={{ flex: 1, padding: '10px', borderRadius: '6px', border: '1px solid #cbd5e1' }} 
          />
        </div>
      ))}

      {/* 4. Footer: Status and Action */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '25px', padding: '15px', background: '#fff', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <div style={{ 
            padding: '4px 12px', 
            borderRadius: '20px', 
            fontSize: '0.8rem', 
            fontWeight: 700,
            background: isBalanced ? '#dcfce7' : '#fee2e2',
            color: isBalanced ? '#166534' : '#991b1b'
          }}>
            {isBalanced ? '✓ Balanced' : '✗ Unbalanced'}
          </div>
          <span style={{ fontSize: '0.9rem', color: '#64748b' }}>
            Sum: <strong>{totalDebit.toLocaleString()}</strong> / <strong>{totalCredit.toLocaleString()}</strong>
          </span>
        </div>

        <button 
          onClick={handleSave} 
          disabled={!isBalanced || isPending} 
          style={{ 
            padding: '12px 28px', 
            background: isBalanced ? '#2563eb' : '#94a3b8', 
            color: 'white', 
            border: 'none', 
            borderRadius: '8px', 
            cursor: isBalanced ? 'pointer' : 'not-allowed',
            fontWeight: 700,
            transition: 'all 0.2s'
          }}
        >
          {isPending ? 'Processing...' : 'Confirm & Post'}
        </button>
      </div>
    </div>
  );
};

