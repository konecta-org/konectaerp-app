import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { UserManagementApiService, UserResponseDto, RoleResponseDto } from '../core/services/user-management-api.service';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  readonly searchControl = new FormControl('');
  readonly loading = signal(false);
  readonly users = signal<UserResponseDto[]>([]);
  readonly selectedUser = signal<UserResponseDto | null>(null);
  readonly availableRoles = signal<RoleResponseDto[]>([]);
  readonly selectedRoleIds = new FormControl<number[]>([]);
  readonly includeDeleted = new FormControl(false);

  readonly statusLabel = computed(() => (status: string) => status ?? 'Unknown');

  private currentPage = 1;

  constructor(private readonly api: UserManagementApiService) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadUsers();

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });
  }

  loadUsers(): void {
    this.loading.set(true);
    this.api.getUsers({
      page: this.currentPage,
      search: this.searchControl.value ?? '',
      includeDeleted: this.includeDeleted.value ?? false
    }).subscribe({
      next: (paged) => {
        this.users.set(paged.items ?? []);
        const current = this.selectedUser();
        if (current) {
          const refreshed = paged.items.find(u => u.id === current.id);
          this.selectedUser.set(refreshed ?? null);
          if (refreshed) {
            this.selectedRoleIds.setValue(refreshed.roles.map(r => r.roleId), { emitEvent: false });
          }
        }
      },
      complete: () => this.loading.set(false),
      error: () => this.loading.set(false)
    });
  }

  loadRoles(): void {
    this.api.getRoles().subscribe(roles => this.availableRoles.set(roles));
  }

  selectUser(user: UserResponseDto): void {
    this.selectedUser.set(user);
    this.selectedRoleIds.setValue(user.roles.map(r => r.roleId));
  }

  saveRoles(): void {
    const user = this.selectedUser();
    const roleIds = this.selectedRoleIds.value ?? [];
    if (!user) {
      return;
    }

    this.api.setUserRoles(user.id, roleIds).subscribe({
      next: () => this.loadUsers()
    });
  }

  trackByUserId(_: number, user: UserResponseDto): string {
    return user.id;
  }
}
