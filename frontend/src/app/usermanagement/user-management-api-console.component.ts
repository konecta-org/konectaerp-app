import { Component } from '@angular/core';
import { ApiConsoleComponent } from '../shared/api-console/api-console.component';
import { USER_MANAGEMENT_API_ENDPOINTS } from '../shared/api-console/api-endpoints';

@Component({
  selector: 'app-user-management-api-console',
  standalone: true,
  imports: [ApiConsoleComponent],
  template: `<app-api-console title="User Management API Explorer" [endpoints]="endpoints"></app-api-console>`
})
export class UserManagementApiConsoleComponent {
  readonly endpoints = USER_MANAGEMENT_API_ENDPOINTS;
}
