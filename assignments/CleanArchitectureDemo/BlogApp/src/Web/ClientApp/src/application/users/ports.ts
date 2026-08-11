import type { UserSummary } from '../../domain/users/types';

export interface IUserApi {
  getUsers(): Promise<UserSummary[]>;
  activate(id: number): Promise<void>;
  deactivate(id: number): Promise<void>;
}