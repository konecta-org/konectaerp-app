import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface EmployeeDto {
  id: string;
  fullName: string;
  email?: string;
  department?: string;
  jobTitle?: string;
  status?: string;
}

@Injectable({
  providedIn: 'root'
})
export class HrApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.hr}`;

  constructor(private readonly http: HttpClient) {}

  getEmployees(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(`${this.baseUrl}/employee`);
  }
}
