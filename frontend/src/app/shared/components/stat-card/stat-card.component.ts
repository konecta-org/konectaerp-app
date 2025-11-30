import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stat-card.component.html',
  styleUrl: './stat-card.component.scss'
})
export class StatCardComponent {
  @Input() label: string = '';
  @Input() value: string | number = '';
  @Input() icon?: string;
  @Input() trend?: 'up' | 'down';
  @Input() trendValue?: string;
  @Input() color: 'blue' | 'green' | 'yellow' | 'red' | 'purple' = 'blue';
  @Input() clickable: boolean = false;
}
