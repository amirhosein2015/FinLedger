import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { identityApi } from '../api';
import { LogIn, ShieldCheck } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('lead.engineer@finledger.com');
  const [password, setPassword] = useState('SecurePassword123!');

  const { mutate, isPending, error } = useMutation({
    mutationFn: identityApi.login,
    onSuccess: (data) => {
      localStorage.setItem('token', data.accessToken);
      window.location.reload();
    }
  });

  return (
    <div style={{ minHeight: '80vh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '20px' }}>
      <div style={{ width: '100%', maxWidth: '400px', background: 'white', padding: '40px', borderRadius: '16px', boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <div style={{ textAlign: 'center', marginBottom: '30px' }}>
          <ShieldCheck size={48} color="#2563eb" style={{ marginBottom: '15px' }} />
          <h2 style={{ margin: 0 }}>FinLedger Login</h2>
        </div>
        
        <form onSubmit={(e) => { e.preventDefault(); mutate({ email, password }); }}>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} style={{ width: '100%', padding: '12px', marginBottom: '15px', borderRadius: '8px', border: '1px solid #ddd', boxSizing: 'border-box' }} placeholder="Email" />
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} style={{ width: '100%', padding: '12px', marginBottom: '20px', borderRadius: '8px', border: '1px solid #ddd', boxSizing: 'border-box' }} placeholder="Password" />
          
          {/*  Centering the button container */}
          <div style={{ display: 'flex', justifyContent: 'center' }}>
            <button disabled={isPending} style={{ padding: '12px 40px', background: '#2563eb', color: 'white', border: 'none', borderRadius: '8px', fontWeight: 600, cursor: 'pointer' }}>
              {isPending ? 'Connecting...' : 'Login'}
            </button>
          </div>
          {error && <p style={{ color: 'red', textAlign: 'center', marginTop: '15px' }}>Invalid Credentials</p>}
        </form>
      </div>
    </div>
  );
};
