import { httpClient } from '../http/httpClient';
import type { IUserApi } from '../../application/users/ports';
import type { UserSummary } from '../../domain/users/types';

export const userApi: IUserApi = {
  async getUsers() {
    const { data } = await httpClient.get<UserSummary[]>('/users');
    return data;
  },
  async activate(id) {
    await httpClient.post(`/users/activate`, { id });
  },
  async deactivate(id) {
    await httpClient.post(`/users/deactivate`, { id });
  },
};