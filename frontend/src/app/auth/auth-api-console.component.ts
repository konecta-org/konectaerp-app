import { Component } from '@angular/core';
import { ApiConsoleComponent } from '../shared/api-console/api-console.component';
import { AUTH_API_ENDPOINTS } from '../shared/api-console/api-endpoints';

@Component({
  selector: 'app-auth-api-console',
  standalone: true,
  imports: [ApiConsoleComponent],
  template: `<app-api-console title="Authentication API Explorer" [endpoints]="endpoints"></app-api-console>`
})
export class AuthApiConsoleComponent {
  readonly endpoints = AUTH_API_ENDPOINTS;
}
