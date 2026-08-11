// infrastructure/errors/parseApiError.ts
import type { AxiosError } from 'axios';
import type { ProblemDetails, AppError } from '../../domain/errors/problemDetails';
import { isValidationProblemDetails } from '../../domain/errors/problemDetails';

// Chuẩn hoá mọi lỗi (network, ProblemDetails, hoặc lỗi lạ) về 1 shape duy nhất
// -> presentation layer chỉ cần biết AppError, không cần biết axios/ProblemDetails là gì.
export function parseApiError(error: AxiosError): AppError {
  if (!error.response) {
    return {
      status: 0,
      title: 'Lỗi kết nối',
      detail: 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra mạng.',
    };
  }

  const data = error.response.data as ProblemDetails | undefined;

  if (!data || !data.title) {
    return {
      status: error.response.status,
      title: 'Đã có lỗi xảy ra',
      detail: 'Vui lòng thử lại sau.',
    };
  }

  if (isValidationProblemDetails(data)) {
    return {
      status: data.status,
      title: data.title,
      detail: data.detail,
      fieldErrors: data.errors,
    };
  }

  return {
    status: data.status,
    title: data.title,
    detail: data.detail,
  };
}