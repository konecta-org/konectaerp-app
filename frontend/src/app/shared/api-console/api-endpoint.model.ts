export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface ApiEndpoint {
  id: string;
  label: string;
  method: HttpMethod;
  pathTemplate: string;
  description?: string;
  queryParameters?: string[];
  sampleBody?: string;
}
