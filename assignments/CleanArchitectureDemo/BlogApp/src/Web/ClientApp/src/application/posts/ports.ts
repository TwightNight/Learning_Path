import type { PostSummary, PostDetail, PostFormValues } from '../../domain/posts/types';

export interface IPostApi {
  getPosts(authorId?: number): Promise<PostSummary[]>;
  getAllPostsForAdmin(): Promise<PostSummary[]>;
  getById(id: number): Promise<PostDetail>;
  create(payload: PostFormValues): Promise<PostDetail>;
  update(id: number, payload: PostFormValues): Promise<PostDetail>;
  remove(id: number): Promise<void>;
  publish(id: number): Promise<void>;
  unpublish(id: number): Promise<void>;
}