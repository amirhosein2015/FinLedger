import axios from 'axios';

// Creating a centralized Axios instance for global configuration
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor to automatically inject Auth and Tenant headers
apiClient.interceptors.request.use((config) => {
  // 1. Get token from local storage (we'll set this during login)
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  // 2. Get current tenant from local storage
  // This is the core of our Multi-tenancy support in the frontend
  const tenantId = localStorage.getItem('tenantId') || 'public';
  config.headers['X-Tenant-Id'] = tenantId;

  return config;
});

export default apiClient;
