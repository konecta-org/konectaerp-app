import { CommonModule } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Component, Input, OnChanges, SimpleChanges, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { ApiEndpoint } from './api-endpoint.model';

@Component({
  selector: 'app-api-console',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './api-console.component.html',
  styleUrl: './api-console.component.scss'
})
export class ApiConsoleComponent implements OnChanges {
  @Input() title = 'API Console';
  @Input() endpoints: ApiEndpoint[] = [];

  readonly selectedEndpointId = signal<string | null>(null);
  readonly responseBody = signal<string>('');
  readonly errorMessage = signal<string>('');
  readonly loading = signal(false);

  private readonly fb = inject(FormBuilder);

  pathForm: FormGroup = this.fb.group({});
  queryForm: FormGroup = this.fb.group({});
  bodyControl = this.fb.control('', { nonNullable: true });

  readonly currentEndpoint = computed(() => {
    const targetId = this.selectedEndpointId();
    if (targetId) {
      return this.endpoints.find(endpoint => endpoint.id === targetId) ?? null;
    }

    return this.endpoints.length > 0 ? this.endpoints[0] : null;
  });

  private readonly http = inject(HttpClient);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['endpoints']) {
      const first = this.endpoints.at(0);
      if (first) {
        this.selectedEndpointId.set(first.id);
        this.rebuildForms(first);
      }
    }
  }

  onEndpointChange(endpointId: string): void {
    this.selectedEndpointId.set(endpointId);
    const endpoint = this.currentEndpoint();
    if (endpoint) {
      this.rebuildForms(endpoint);
    }
  }

  sendRequest(): void {
    const endpoint = this.currentEndpoint();
    if (!endpoint) {
      return;
    }

    this.errorMessage.set('');
    this.responseBody.set('');
    this.loading.set(true);

    const url = this.buildUrl(endpoint);
    const params = this.buildQueryParams(endpoint);
    const options: Record<string, unknown> = { params };
    const method = endpoint.method.toUpperCase();

    if (method !== 'GET' && method !== 'DELETE') {
      const parsedBody = this.parseBody();
      if (parsedBody === null && this.bodyControl.value.trim().length > 0) {
        this.errorMessage.set('Invalid JSON body.');
        this.loading.set(false);
        return;
      }
      options['body'] = parsedBody;
    }

    this.http.request(method, url, options).subscribe({
      next: response => {
        const pretty = typeof response === 'string'
          ? response
          : JSON.stringify(response, null, 2);
        this.responseBody.set(pretty);
      },
      error: error => {
        if (error.error instanceof Blob) {
          error.error.text().then((message: string) => this.errorMessage.set(message));
        } else if (typeof error.error === 'string') {
          this.errorMessage.set(error.error);
        } else if (error.error) {
          this.errorMessage.set(JSON.stringify(error.error, null, 2));
        } else {
          this.errorMessage.set(error.message ?? 'Request failed');
        }
      },
      complete: () => this.loading.set(false)
    });
  }

  private rebuildForms(endpoint: ApiEndpoint): void {
    const placeholders = Array.from(endpoint.pathTemplate.matchAll(/\{([^}]+)\}/g)).map(match => match[1]);
    const pathControls = placeholders.reduce<Record<string, FormControl<string>>>((acc, param) => {
      acc[param] = this.fb.control<string>('', { nonNullable: true });
      return acc;
    }, {});
    this.pathForm = this.fb.group(pathControls);

    const queryControls = (endpoint.queryParameters ?? []).reduce<Record<string, FormControl<string>>>((acc, param) => {
      acc[param] = this.fb.control<string>('', { nonNullable: true });
      return acc;
    }, {});
    this.queryForm = this.fb.group(queryControls);

    const needsBody = endpoint.method !== 'GET' && endpoint.method !== 'DELETE';
    this.bodyControl.setValue(needsBody ? (endpoint.sampleBody ?? '{\n\n}') : '');
  }

  private buildUrl(endpoint: ApiEndpoint): string {
    let finalPath = endpoint.pathTemplate;
    Object.entries(this.pathForm.value).forEach(([key, value]) => {
      const replacement = encodeURIComponent(String(value ?? ''));
      finalPath = finalPath.replace(new RegExp(`{${key}}`, 'g'), replacement);
    });

    return `${environment.apiBaseUrl}${finalPath}`;
  }

  private buildQueryParams(endpoint: ApiEndpoint): HttpParams {
    let params = new HttpParams();
    Object.entries(this.queryForm.value).forEach(([key, value]) => {
      if (value !== null && value !== undefined && `${value}`.length > 0) {
        params = params.set(key, value as string);
      }
    });
    return params;
  }

  private parseBody(): unknown {
    const raw = this.bodyControl.value.trim();
    if (!raw) {
      return undefined;
    }

    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  pathControl(key: string): FormControl<string> {
    return this.pathForm.get(key) as FormControl<string>;
  }

  queryControl(key: string): FormControl<string> {
    return this.queryForm.get(key) as FormControl<string>;
  }
}
