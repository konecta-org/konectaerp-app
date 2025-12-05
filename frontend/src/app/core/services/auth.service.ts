import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../../environments/environment';
import { map, Observable, tap } from 'rxjs';

interface ApiResponse<T> {
  result?: T;
  Result?: T;
  code?: string;
  Code?: string;
  c_Message?: string;
  C_Message?: string;
  s_Message?: string;
  S_Message?: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

interface UpdatePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

interface LoginResult {
  accessToken: string;
  expiresAtUtc: string;
  keyId: string;
  userId: string;
  email: string;
  roles: string[];
  permissions?: string[];
}

export interface AuthSession {
  token: string;
  userId: string;
  email: string;
  fullName?: string;
  roles: string[];
  permissions: string[];
  expiresAtUtc: string;
}

const STORAGE_KEY = 'konecta.erp.session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly sessionSignal = signal<AuthSession | null>(this.restoreSession());

  readonly currentSession = computed(() => this.sessionSignal());
  readonly isAuthenticated = computed(() => {
    const session = this.sessionSignal();
    if (!session) {
      return false;
    }
    return new Date(session.expiresAtUtc).getTime() > Date.now();
  });

  defaultRoute(): string {
    const session = this.sessionSignal();
    if (!session) {
      return '/landing';
    }

    const roleMatches = (...roles: string[]) =>
      roles.some(role => this.hasRole(role));

    if (roleMatches('SystemAdmin')) {
      return '/landing';
    }

    if (roleMatches('FinanceManager', 'FinanceStaff')) {
      return '/finance';
    }

    if (roleMatches('HrAdmin', 'HrStaff')) {
      return '/hr';
    }

    if (roleMatches('DepartmentManager')) {
      return '/inventory';
    }

    if (roleMatches('ReportingManager', 'ReportingAnalyst', 'ReportingStaff')) {
      return '/reporting';
    }

    if (roleMatches('Employee')) {
      return '/landing';
    }

    if (session.permissions?.includes('user-management.users.read')) {
      return '/users';
    }

    return '/landing';
  }

  login(request: LoginRequest) {
    const url = `${environment.apiBaseUrl}${environment.endpoints.auth}/login`;
    return this.http.post<ApiResponse<LoginResult>>(url, request)
      .pipe(
        this.unwrapResponse<LoginResult>(),
        this.persistSession()
      );
  }

  register(request: RegisterRequest) {
    const url = `${environment.apiBaseUrl}${environment.endpoints.auth}/register`;
    return this.http.post<ApiResponse<unknown>>(url, request)
      .pipe(this.unwrapResponse());
  }

  validateToken(token: string) {
    const url = `${environment.apiBaseUrl}${environment.endpoints.auth}/validate-token`;
    return this.http.post<ApiResponse<{ email: string }>>(url, token)
      .pipe(this.unwrapResponse());
  }

  updatePassword(request: UpdatePasswordRequest) {
    const url = `${environment.apiBaseUrl}${environment.endpoints.auth}/update-password`;
    return this.http.put<ApiResponse<{ email: string }>>(url, request)
      .pipe(this.unwrapResponse());
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.sessionSignal.set(null);
  }

  currentToken(): string | null {
    const session = this.sessionSignal();
    return session ? session.token : null;
  }

  hasPermission(permission: string): boolean {
    const session = this.sessionSignal();
    return !!session?.permissions?.includes(permission);
  }

  hasPermissionPrefix(prefix: string): boolean {
    if (!prefix) {
      return false;
    }

    const session = this.sessionSignal();
    const normalizedPrefix = prefix.toLowerCase();

    return !!session?.permissions?.some(permission => {
      const normalizedPermission = permission.toLowerCase();
      return normalizedPermission === normalizedPrefix || normalizedPermission.startsWith(`${normalizedPrefix}.`);
    });
  }

  hasRole(role: string): boolean {
    const session = this.sessionSignal();
    if (!session?.roles?.length) {
      return false;
    }

    const normalize = (value: string) => value.toLowerCase().replace(/\s+/g, '');
    const target = normalize(role);

    return session.roles.some(r => normalize(r) === target);
  }

  private unwrapResponse<T>() {
    return (source: Observable<ApiResponse<T>>) =>
      source.pipe(
        map(response => {
          const result = response.result ?? (response as any).Result ?? (response as any).data ?? null;
          return result as T;
        })
      );
  }

  private persistSession() {
    return (source: Observable<LoginResult>) =>
      source.pipe(
        tap(result => {
          const permissions = result.permissions ?? [];
          const session: AuthSession = {
            token: result.accessToken,
            userId: result.userId,
            email: result.email,
            roles: result.roles ?? [],
            permissions,
            expiresAtUtc: result.expiresAtUtc,
            fullName: this.extractFullName(result.accessToken)
          };
          localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
          this.sessionSignal.set(session);
        })
      );
  }

  private restoreSession(): AuthSession | null {
    const cached = localStorage.getItem(STORAGE_KEY);
    if (!cached) {
      return null;
    }

    try {
      const parsed = JSON.parse(cached) as AuthSession;
      if (new Date(parsed.expiresAtUtc).getTime() < Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return parsed;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }

  private extractFullName(token: string): string | undefined {
    try {
      const decoded = jwtDecode<{ full_name?: string }>(token);
      return decoded.full_name;
    } catch {
      return undefined;
    }
  }
}
