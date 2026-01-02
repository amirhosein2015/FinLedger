import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { identityApi } from '../api';
import { LogIn } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('lead.engineer@finledger.com');
  const [password, setPassword] = useState('SecurePassword123!');

  //  Using useMutation for clean async state handling (loading, error, etc.)
  const { mutate, isPending, error } = useMutation({
    mutationFn: identityApi.login,
    onSuccess: (data) => {
      localStorage.setItem('token', data.accessToken);
      alert('Login Successful! Token saved.');
      window.location.reload(); // Refresh to let the interceptor pick up the token
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutate({ email, password });
  };

  return (
    <div style={{ padding: '2rem', maxWidth: '400px', margin: 'auto' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <LogIn size={32} color="#2563eb" />
        <h2>FinLedger Login</h2>
      </div>
      
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '1rem' }}>
          <label>Email:</label>
          <input 
            type="email" 
            value={email} 
            onChange={(e) => setEmail(e.target.value)}
            style={{ width: '100%', padding: '8px', marginTop: '5px' }}
          />
        </div>
        <div style={{ marginBottom: '1rem' }}>
          <label>Password:</label>
          <input 
            type="password" 
            value={password} 
            onChange={(e) => setPassword(e.target.value)}
            style={{ width: '100%', padding: '8px', marginTop: '5px' }}
          />
        </div>
        <button 
          disabled={isPending}
          style={{ width: '100%', padding: '10px', background: '#2563eb', color: 'white', border: 'none', borderRadius: '4px' }}
        >
          {isPending ? 'Authenticating...' : 'Login to Engine'}
        </button>
        {error && <p style={{ color: 'red' }}>Error: {(error as any).response?.data?.title || 'Login failed'}</p>}
      </form>
    </div>
  );
};
