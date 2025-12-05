import { ApiEndpoint } from './api-endpoint.model';

export const AUTH_API_ENDPOINTS: ApiEndpoint[] = [
  {
    id: 'auth-login',
    label: 'Login',
    method: 'POST',
    pathTemplate: '/auth/login',
    sampleBody: JSON.stringify({ email: 'user@konecta.com', password: 'P@ssw0rd!' }, null, 2)
  },
  {
    id: 'auth-register',
    label: 'Register',
    method: 'POST',
    pathTemplate: '/auth/register',
    sampleBody: JSON.stringify({ fullName: 'Fatma Ibrahim', email: 'new@konecta.com', password: 'Temp123!' }, null, 2)
  },
  {
    id: 'auth-validate',
    label: 'Validate Token',
    method: 'POST',
    pathTemplate: '/auth/validate-token',
    sampleBody: JSON.stringify('paste-jwt-token', null, 2)
  },
  {
    id: 'auth-update-password',
    label: 'Update Password',
    method: 'PUT',
    pathTemplate: '/auth/update-password',
    sampleBody: JSON.stringify({ oldPassword: 'Old123!', newPassword: 'New123!', confirmPassword: 'New123!' }, null, 2)
  }
];

export const USER_MANAGEMENT_API_ENDPOINTS: ApiEndpoint[] = [
  { id: 'users-list', label: 'List Users', method: 'GET', pathTemplate: '/users/api/users', queryParameters: ['Page', 'PageSize', 'Search', 'Department', 'Role', 'IncludeDeleted', 'OnlyLocked'] },
  { id: 'users-all', label: 'Get All Users (raw)', method: 'GET', pathTemplate: '/users/api/users/all-users' },
  { id: 'users-get', label: 'Get User by Id', method: 'GET', pathTemplate: '/users/api/users/{id}', description: 'Replace {id} with the user identifier.' },
  {
    id: 'users-create',
    label: 'Create User',
    method: 'POST',
    pathTemplate: '/users/api/users',
    sampleBody: JSON.stringify({
      email: 'employee@konecta.com',
      fullName: 'Employee Name',
      department: 'Finance',
      jobTitle: 'Analyst',
      status: 'Active',
      roleIds: [1],
      createdBy: 'UI'
    }, null, 2)
  },
  {
    id: 'users-update',
    label: 'Update User',
    method: 'PUT',
    pathTemplate: '/users/api/users/{id}',
    sampleBody: JSON.stringify({
      fullName: 'Updated Name',
      department: 'Finance',
      jobTitle: 'Senior Analyst',
      status: 'Active',
      phoneNumber: '+20222222'
    }, null, 2)
  },
  {
    id: 'users-change-role',
    label: 'Change Primary Role',
    method: 'PATCH',
    pathTemplate: '/users/api/users/{id}/role',
    sampleBody: JSON.stringify({ newRole: 'Finance Manager', changedBy: 'UI' }, null, 2)
  },
  { id: 'users-get-roles', label: 'Get User Roles', method: 'GET', pathTemplate: '/users/api/users/{id}/roles' },
  {
    id: 'users-set-roles',
    label: 'Set User Roles',
    method: 'PUT',
    pathTemplate: '/users/api/users/{id}/roles',
    sampleBody: JSON.stringify({ roleIds: [1, 2], assignedBy: 'UI' }, null, 2)
  },
  {
    id: 'users-status',
    label: 'Update User Status',
    method: 'PATCH',
    pathTemplate: '/users/api/users/{id}/status',
    sampleBody: JSON.stringify({ status: 'Active', lockAccount: false }, null, 2)
  },
  { id: 'users-delete', label: 'Soft Delete User', method: 'DELETE', pathTemplate: '/users/api/users/{id}' },
  { id: 'users-restore', label: 'Restore User', method: 'POST', pathTemplate: '/users/api/users/{id}/restore' },
  { id: 'users-summary', label: 'Summary Snapshot', method: 'GET', pathTemplate: '/users/api/users/summary' },
  { id: 'users-authorizations', label: 'Authorization Profile', method: 'GET', pathTemplate: '/users/api/users/{id}/authorizations' },
  { id: 'roles-list', label: 'List Roles', method: 'GET', pathTemplate: '/users/api/roles' },
  { id: 'roles-get', label: 'Get Role', method: 'GET', pathTemplate: '/users/api/roles/{id}' },
  {
    id: 'roles-create',
    label: 'Create Role',
    method: 'POST',
    pathTemplate: '/users/api/roles',
    sampleBody: JSON.stringify({ name: 'Project Coordinator', description: 'Limited PM permissions', isSystemDefault: false }, null, 2)
  },
  {
    id: 'roles-update',
    label: 'Update Role',
    method: 'PUT',
    pathTemplate: '/users/api/roles/{id}',
    sampleBody: JSON.stringify({ name: 'Finance Staff', description: 'Updated description', isActive: true }, null, 2)
  },
  { id: 'roles-delete', label: 'Delete Role', method: 'DELETE', pathTemplate: '/users/api/roles/{id}' },
  {
    id: 'roles-permissions',
    label: 'Update Role Permissions',
    method: 'PUT',
    pathTemplate: '/users/api/roles/{id}/permissions',
    sampleBody: JSON.stringify({ permissionIds: [1, 2, 3] }, null, 2)
  },
  { id: 'permissions-list', label: 'List Permissions', method: 'GET', pathTemplate: '/users/api/permissions' },
  { id: 'permissions-get', label: 'Get Permission', method: 'GET', pathTemplate: '/users/api/permissions/{id}' },
  {
    id: 'permissions-create',
    label: 'Create Permission',
    method: 'POST',
    pathTemplate: '/users/api/permissions',
    sampleBody: JSON.stringify({ name: 'finance.budgets.approve', category: 'Finance', description: 'Allow budget approval' }, null, 2)
  },
  {
    id: 'permissions-update',
    label: 'Update Permission',
    method: 'PUT',
    pathTemplate: '/users/api/permissions/{id}',
    sampleBody: JSON.stringify({ name: 'finance.budgets.read', category: 'Finance', description: 'Read budgets', isActive: true }, null, 2)
  },
  { id: 'permissions-delete', label: 'Delete Permission', method: 'DELETE', pathTemplate: '/users/api/permissions/{id}' }
];

export const HR_API_ENDPOINTS: ApiEndpoint[] = [
  { id: 'attendance-list', label: 'List Attendance', method: 'GET', pathTemplate: '/hr/api/Attendance' },
  { id: 'attendance-get', label: 'Get Attendance', method: 'GET', pathTemplate: '/hr/api/Attendance/{id}' },
  { id: 'attendance-create', label: 'Create Attendance', method: 'POST', pathTemplate: '/hr/api/Attendance', sampleBody: '{ }' },
  { id: 'attendance-update', label: 'Update Attendance', method: 'PUT', pathTemplate: '/hr/api/Attendance/{id}', sampleBody: '{ }' },
  { id: 'attendance-delete', label: 'Delete Attendance', method: 'DELETE', pathTemplate: '/hr/api/Attendance/{id}' },

  { id: 'department-list', label: 'List Departments', method: 'GET', pathTemplate: '/hr/api/Department' },
  { id: 'department-get', label: 'Get Department', method: 'GET', pathTemplate: '/hr/api/Department/{id}' },
  { id: 'department-create', label: 'Create Department', method: 'POST', pathTemplate: '/hr/api/Department', sampleBody: '{ }' },
  { id: 'department-update', label: 'Update Department', method: 'PUT', pathTemplate: '/hr/api/Department/{id}', sampleBody: '{ }' },
  { id: 'department-delete', label: 'Delete Department', method: 'DELETE', pathTemplate: '/hr/api/Department/{id}' },
  { id: 'department-manager', label: 'Assign Manager', method: 'PUT', pathTemplate: '/hr/api/Department/{id}/manager', sampleBody: '{ "managerId": "" }' },

  { id: 'employee-list', label: 'List Employees', method: 'GET', pathTemplate: '/hr/api/Employee' },
  { id: 'employee-get', label: 'Get Employee', method: 'GET', pathTemplate: '/hr/api/Employee/{id}' },
  { id: 'employee-create', label: 'Create Employee', method: 'POST', pathTemplate: '/hr/api/Employee', sampleBody: '{ }' },
  { id: 'employee-update', label: 'Update Employee', method: 'PUT', pathTemplate: '/hr/api/Employee/{id}', sampleBody: '{ }' },
  { id: 'employee-bonuses', label: 'Issue Bonuses', method: 'POST', pathTemplate: '/hr/api/Employee/{id}/bonuses', sampleBody: '{ }' },
  { id: 'employee-deductions', label: 'Issue Deductions', method: 'POST', pathTemplate: '/hr/api/Employee/{id}/deductions', sampleBody: '{ }' },
  { id: 'employee-fire', label: 'Fire Employee', method: 'POST', pathTemplate: '/hr/api/Employee/{id}/fire', sampleBody: '{ "reason": "" }' },

  { id: 'hr-summary', label: 'HR Summary', method: 'GET', pathTemplate: '/hr/api/HrSummary' },

  { id: 'interviews-list', label: 'List Interviews', method: 'GET', pathTemplate: '/hr/api/Interviews' },
  { id: 'interviews-get', label: 'Get Interview', method: 'GET', pathTemplate: '/hr/api/Interviews/{id}' },
  { id: 'interviews-create', label: 'Schedule Interview', method: 'POST', pathTemplate: '/hr/api/Interviews', sampleBody: '{ }' },
  { id: 'interviews-update', label: 'Update Interview', method: 'PUT', pathTemplate: '/hr/api/Interviews/{id}', sampleBody: '{ }' },
  { id: 'interviews-delete', label: 'Cancel Interview', method: 'DELETE', pathTemplate: '/hr/api/Interviews/{id}' },

  { id: 'job-openings-list', label: 'List Job Openings', method: 'GET', pathTemplate: '/hr/api/JobOpenings' },
  { id: 'job-openings-get', label: 'Get Job Opening', method: 'GET', pathTemplate: '/hr/api/JobOpenings/{id}' },
  { id: 'job-openings-create', label: 'Create Job Opening', method: 'POST', pathTemplate: '/hr/api/JobOpenings', sampleBody: '{ }' },
  { id: 'job-openings-update', label: 'Update Job Opening', method: 'PUT', pathTemplate: '/hr/api/JobOpenings/{id}', sampleBody: '{ }' },
  { id: 'job-openings-delete', label: 'Delete Job Opening', method: 'DELETE', pathTemplate: '/hr/api/JobOpenings/{id}' },

  { id: 'job-applications-list', label: 'List Job Applications', method: 'GET', pathTemplate: '/hr/api/JobApplications' },
  { id: 'job-applications-get', label: 'Get Job Application', method: 'GET', pathTemplate: '/hr/api/JobApplications/{id}' },
  { id: 'job-applications-create', label: 'Create Job Application', method: 'POST', pathTemplate: '/hr/api/JobApplications', sampleBody: '{ }' },
  { id: 'job-applications-update', label: 'Update Job Application', method: 'PUT', pathTemplate: '/hr/api/JobApplications/{id}', sampleBody: '{ }' },
  { id: 'job-applications-delete', label: 'Delete Job Application', method: 'DELETE', pathTemplate: '/hr/api/JobApplications/{id}' },

  { id: 'leave-requests-list', label: 'List Leave Requests', method: 'GET', pathTemplate: '/hr/api/LeaveRequests' },
  { id: 'leave-requests-get', label: 'Get Leave Request', method: 'GET', pathTemplate: '/hr/api/LeaveRequests/{id}' },
  { id: 'leave-requests-create', label: 'Create Leave Request', method: 'POST', pathTemplate: '/hr/api/LeaveRequests', sampleBody: '{ }' },
  { id: 'leave-requests-update', label: 'Update Leave Request', method: 'PUT', pathTemplate: '/hr/api/LeaveRequests/{id}', sampleBody: '{ }' },
  { id: 'leave-requests-delete', label: 'Delete Leave Request', method: 'DELETE', pathTemplate: '/hr/api/LeaveRequests/{id}' },

  { id: 'resignations-create', label: 'Submit Resignation', method: 'POST', pathTemplate: '/hr/api/ResignationRequests', sampleBody: '{ }' },
  { id: 'resignations-list', label: 'List Resignations', method: 'GET', pathTemplate: '/hr/api/ResignationRequests' },
  { id: 'resignations-get', label: 'Get Resignation', method: 'GET', pathTemplate: '/hr/api/ResignationRequests/{id}' },
  { id: 'resignations-decision', label: 'Decide on Resignation', method: 'PUT', pathTemplate: '/hr/api/ResignationRequests/{id}/decision', sampleBody: '{ "status": "Approved", "reviewedBy": "" }' }
];
