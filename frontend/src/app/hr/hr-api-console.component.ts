import { Component } from '@angular/core';
import { ApiConsoleComponent } from '../shared/api-console/api-console.component';
import { HR_API_ENDPOINTS } from '../shared/api-console/api-endpoints';

@Component({
  selector: 'app-hr-api-console',
  standalone: true,
  imports: [ApiConsoleComponent],
  template: `<app-api-console title="HR Service API Explorer" [endpoints]="endpoints"></app-api-console>`
})
export class HrApiConsoleComponent {
  readonly endpoints = HR_API_ENDPOINTS;
}
