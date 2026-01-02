import apiClient from '../../api/client';
// Using type-only imports for better tree-shaking and build optimization
import type { LoginRequest, LoginResponse, RegisterRequest } from './models'; 

export const identityApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/identity/Users/login', data);
    return response.data;
  },
  
  register: async (data: RegisterRequest): Promise<void> => {
    await apiClient.post('/identity/Users/register', data);
  }
};
