import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss'
})
export class ModalComponent {
  @Input() title: string = '';
  @Input() size: 'small' | 'medium' | 'large' | 'full' = 'medium';
  @Input() closeOnBackdrop: boolean = true;
  @Input() showFooter: boolean = true;
  @Output() close = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.onClose();
  }

  onClose(): void {
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (this.closeOnBackdrop && (event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }
}
