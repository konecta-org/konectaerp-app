import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface HrSummaryDto {
  totalEmployees: number;
  activeEmployees: number;
  departments: number;
  pendingResignations: number;
}

export enum EmploymentStatus {
  Active = 0,
  OnLeave = 1,
  Resigned = 2,
  Terminated = 3
}

export interface EmployeeDto {
  id: string;
  fullName: string;
  workEmail: string;
  personalEmail: string;
  position: string;
  phoneNumber?: string;
  salary: number;
  hireDate: string;
  status: EmploymentStatus;
  departmentId: string;
  departmentName: string;
  userId?: string;
  createdAt: string;
  updatedAt?: string;
  exitDate?: string;
  exitReason?: string;
  eligibleForRehire?: boolean;
}

export interface AddEmployeeRequest {
  fullName: string;
  workEmail: string;
  personalEmail: string;
  position: string;
  phoneNumber?: string;
  salary: number;
  hireDate?: string;
  status: EmploymentStatus;
  departmentId: string;
}

export interface UpdateEmployeeRequest {
  id: string;
  fullName: string;
  workEmail: string;
  personalEmail: string;
  position: string;
  phoneNumber?: string;
  salary: number;
  hireDate?: string;
  status: EmploymentStatus;
  departmentId: string;
  userId?: string;
  exitDate?: string;
  exitReason?: string;
  eligibleForRehire?: boolean;
}

export interface FireEmployeeRequest {
  reason?: string;
  eligibleForRehire?: boolean;
}

export interface IssueEmployeeBonusesRequest {
  bonuses: EmployeeBonusItemRequest[];
  issuedBy?: string;
}

export interface EmployeeBonusItemRequest {
  bonusType: string;
  amount: number;
  awardedOn?: string;
  period?: string;
  reference?: string;
  awardedBy?: string;
  notes?: string;
  sourceSystem?: string;
}

export interface IssueEmployeeDeductionsRequest {
  deductions: EmployeeDeductionItemRequest[];
  issuedBy?: string;
}

export interface EmployeeDeductionItemRequest {
  deductionType: string;
  amount: number;
  appliedOn?: string;
  period?: string;
  reference?: string;
  appliedBy?: string;
  notes?: string;
  sourceSystem?: string;
  isRecurring?: boolean;
}

export interface DepartmentEmployeeDto {
  id: string;
  fullName: string;
  jobTitle?: string;
  status?: string;
}

export interface DepartmentDto {
  departmentId: string;
  departmentName: string;
  description?: string;
  managerId?: string;
  managerFullName?: string;
  employeeCount: number;
  employees: DepartmentEmployeeDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDepartmentRequest {
  departmentName: string;
  description?: string;
  managerId?: string | null;
}

export interface UpdateDepartmentRequest {
  departmentId: string;
  departmentName: string;
  description?: string;
  managerId?: string | null;
}

export interface AssignDepartmentManagerRequest {
  employeeId: string;
}

export interface AttendanceRecordDto {
  id: string;
  employeeId: string;
  employeeName: string;
  workDate: string;
  checkInTime?: string;
  checkOutTime?: string;
  status: string;
  notes?: string;
}

export interface LeaveRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  status: string;
  createdAt: string;
}

export interface JobOpeningDto {
  id: string;
  title: string;
  departmentId: string;
  departmentName?: string;
  location?: string;
  employmentType?: string;
  openings?: number;
  status: string;
  description?: string;
  requirements?: string;
  salaryMin?: number;
  salaryMax?: number;
  postedDate?: string;
  closingDate?: string;
  applicationCount?: number;
  createdAt: string;
}

export interface JobApplicationDto {
  id: string;
  jobOpeningId: string;
  jobTitle?: string;
  candidateName: string;
  candidateEmail: string;
  candidatePhone?: string;
  resumeUrl?: string;
  coverLetter?: string;
  status: string;
  appliedAt: string;
  updatedAt?: string;
}

export interface InterviewDto {
  id: string;
  scheduledAt: string;
  interviewerName?: string;
  interviewType?: string;
  status: string;
  jobApplicationId: string;
  jobApplicationTitle?: string;
  notes?: string;
}

export interface ResignationRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  submittedAt: string;
  lastWorkingDay: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class HrApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}${environment.endpoints.hr}`;

  constructor(private readonly http: HttpClient) {}

  getSummary(): Observable<HrSummaryDto> {
    return this.http.get<HrSummaryDto>(`${this.baseUrl}/HrSummary`);
  }

  getEmployees(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(`${this.baseUrl}/Employee`);
  }

  getEmployee(id: string): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.baseUrl}/Employee/${id}`);
  }

  createEmployee(request: AddEmployeeRequest): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(`${this.baseUrl}/Employee`, request);
  }

  updateEmployee(id: string, request: UpdateEmployeeRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Employee/${id}`, request);
  }

  issueEmployeeBonuses(id: string, request: IssueEmployeeBonusesRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/Employee/${id}/bonuses`, request);
  }

  issueEmployeeDeductions(id: string, request: IssueEmployeeDeductionsRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/Employee/${id}/deductions`, request);
  }

  fireEmployee(id: string, request: FireEmployeeRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/Employee/${id}/fire`, request);
  }

  getDepartments(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(`${this.baseUrl}/Department`);
  }

  createDepartment(request: CreateDepartmentRequest): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(`${this.baseUrl}/Department`, request);
  }

  updateDepartment(id: string, request: UpdateDepartmentRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Department/${id}`, request);
  }

  assignDepartmentManager(id: string, request: AssignDepartmentManagerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Department/${id}/manager`, request);
  }

  getAttendance(params?: { employeeId?: string; startDate?: string; endDate?: string }): Observable<AttendanceRecordDto[]> {
    const httpParams = this.createParams(params);
    return this.http.get<AttendanceRecordDto[]>(`${this.baseUrl}/Attendance`, { params: httpParams });
  }

  getLeaveRequests(params?: { employeeId?: string; pendingOnly?: boolean }): Observable<LeaveRequestDto[]> {
    const httpParams = this.createParams({
      employeeId: params?.employeeId,
      pendingOnly: params?.pendingOnly ? 'true' : undefined
    });
    return this.http.get<LeaveRequestDto[]>(`${this.baseUrl}/LeaveRequests`, { params: httpParams });
  }

  getJobOpenings(params?: { departmentId?: string; includeApplications?: boolean }): Observable<JobOpeningDto[]> {
    const httpParams = this.createParams({
      departmentId: params?.departmentId,
      includeApplications: params?.includeApplications ? 'true' : undefined
    });
    return this.http.get<JobOpeningDto[]>(`${this.baseUrl}/JobOpenings`, { params: httpParams });
  }

  getJobApplications(params?: { jobOpeningId?: string }): Observable<JobApplicationDto[]> {
    const httpParams = this.createParams({ jobOpeningId: params?.jobOpeningId });
    return this.http.get<JobApplicationDto[]>(`${this.baseUrl}/JobApplications`, { params: httpParams });
  }

  getInterviews(params?: { jobApplicationId?: string }): Observable<InterviewDto[]> {
    const httpParams = this.createParams({ jobApplicationId: params?.jobApplicationId });
    return this.http.get<InterviewDto[]>(`${this.baseUrl}/Interviews`, { params: httpParams });
  }

  getResignations(params?: { status?: string }): Observable<ResignationRequestDto[]> {
    const httpParams = this.createParams({ status: params?.status });
    return this.http.get<ResignationRequestDto[]>(`${this.baseUrl}/ResignationRequests`, { params: httpParams });
  }

  private createParams(params?: Record<string, string | undefined>): HttpParams | undefined {
    if (!params) {
      return undefined;
    }

    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, value);
      }
    });
    return httpParams.keys().length ? httpParams : undefined;
  }
}
