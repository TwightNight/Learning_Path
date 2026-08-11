// domain/errors/problemDetails.ts
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors: Record<string, string[]>;
}

export interface AppError {
  status: number;
  title: string;
  detail?: string;
  fieldErrors?: Record<string, string[]>;
}

export function isValidationProblemDetails(
  pd: ProblemDetails,
): pd is ValidationProblemDetails {
  return 'errors' in pd && typeof (pd as ValidationProblemDetails).errors === 'object';
}