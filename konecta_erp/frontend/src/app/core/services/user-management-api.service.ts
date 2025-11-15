import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';

export interface UserRoleDto {
  roleId: number;
  name: string;
  isSystemDefault: boolean;
}

export interface UserResponseDto {
  id: string;
  email: string;
  fullName: string;
  department?: string;
  jobTitle?: string;
  primaryRole: string;
  roles: UserRoleDto[];
  status: string;
  phoneNumber?: string;
  managerId?: string;
  isLocked: boolean;
}

export interface RoleResponseDto {
  id: number;
  name: string;
  description?: string;
  isSystemDefault: boolean;
  isActive: boolean;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
}

export interface UserQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  department?: string;
  includeDeleted?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserManagementApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.users}`;

  constructor(private readonly http: HttpClient) {}

  getUsers(params: UserQueryParams = {}): Observable<PagedResult<UserResponseDto>> {
    const httpParams = new HttpParams({
      fromObject: {
        Page: params.page?.toString() ?? '1',
        PageSize: params.pageSize?.toString() ?? '20',
        Search: params.search ?? '',
        Department: params.department ?? '',
        IncludeDeleted: params.includeDeleted ? 'true' : 'false'
      }
    });

    return this.http
      .get<PagedResult<UserResponseDto>>(`${this.baseUrl}/users`, { params: httpParams })
      .pipe(map(result => ({ ...result, items: result.items ?? [] })));
  }

  getRoles(): Observable<RoleResponseDto[]> {
    return this.http.get<RoleResponseDto[]>(`${this.baseUrl}/roles`);
  }

  setUserRoles(userId: string, roleIds: number[]): Observable<void> {
    const body = { roleIds, assignedBy: 'Frontend' };
    return this.http.put<void>(`${this.baseUrl}/users/${userId}/roles`, body);
  }
}
