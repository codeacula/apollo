import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
});

export interface Configuration {
  key: string;
  value: string;
}

export const configurationService = {
  async getAll(): Promise<Configuration[]> {
    const response = await api.get('/configurations');
    return response.data;
  },

  async get(key: string): Promise<Configuration> {
    const response = await api.get(`/configurations/${key}`);
    return response.data;
  },

  async set(key: string, value: string): Promise<void> {
    await api.post('/configurations', { key, value });
  },

  async delete(key: string): Promise<void> {
    await api.delete(`/configurations/${key}`);
  },
};
