import { httpClient } from '../http/httpClient';
import type { IPostApi } from '../../application/posts/ports';
import type { PostSummary, PostDetail } from '../../domain/posts/types';

export const postApi: IPostApi = {
  async getPosts(authorId) {
    const { data } = await httpClient.get<PostSummary[]>('/posts', {
      params: authorId ? { authorId } : undefined,
    });
    return data;
  },
  async getAllPostsForAdmin() {
    const { data } = await httpClient.get<PostSummary[]>('/admin/posts');
    return data;
  },
  async getById(id) {
    const { data } = await httpClient.get<PostDetail>(`/posts/${id}`);
    return data;
  },
  async create(payload) {
    const { data } = await httpClient.post<PostDetail>('/posts', payload);
    return data;
  },
  async update(id, payload) {
    const { data } = await httpClient.put<PostDetail>(`/posts/${id}`, payload);
    return data;
  },
  async remove(id) {
    await httpClient.delete(`/posts/${id}`);
  },
  async publish(id) {
    await httpClient.post(`/posts/publish`, { id });
  },
  async unpublish(id) {
    await httpClient.post(`/posts/unpublish`, { id });
  },
};