import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Toast } from '../../services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss'
})
export class ToastComponent implements OnInit {
  toasts = signal<Toast[]>([]);

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.toast$.subscribe(toast => {
      this.addToast(toast);
    });
  }

  private addToast(toast: Toast): void {
    this.toasts.update(toasts => [...toasts, toast]);
    
    setTimeout(() => {
      this.removeToast(toast.id);
    }, toast.duration);
  }

  removeToast(id: string): void {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }

  getIcon(type: Toast['type']): string {
    const icons = {
      success: '✓',
      error: '✕',
      warning: '⚠',
      info: 'ℹ'
    };
    return icons[type];
  }
}
