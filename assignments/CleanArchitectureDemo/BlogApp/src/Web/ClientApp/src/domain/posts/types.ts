export interface PostSummary {
  id: number;
  title: string;
  content: string;
  publishedDate: string | null;
  isPublished: boolean;
  authorId: number;
  authorFullName: string;
  commentsCount: number;
}

export interface PostDetail {
  id: number;
  title: string;
  content: string;
}

export interface PostFormValues {
  title: string;
  content: string;
}