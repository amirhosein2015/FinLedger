import { useState, useEffect } from 'react';

export const useTenant = () => {
  // Ensuring the frontend state aligns with the allowed tenant context
  const [tenantId, setTenantId] = useState<string>(() => {
    const saved = localStorage.getItem('tenantId');
    // If the saved tenant is not our new production tenant, force it to amsterdam_hq
    return (saved === 'amsterdam_hq') ? saved : 'amsterdam_hq';
  });

  const switchTenant = (newTenantId: string) => {
    localStorage.setItem('tenantId', newTenantId);
    setTenantId(newTenantId);
    window.location.reload(); 
  };

  // Sync localStorage on first load
  useEffect(() => {
    localStorage.setItem('tenantId', tenantId);
  }, [tenantId]);

  return { tenantId, switchTenant };
};
