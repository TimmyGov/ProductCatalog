import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-error-message',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-message" *ngIf="errorMessage">
      <p>{{ errorMessage }}</p>
    </div>
  `,
  styles: [`
    .error-message {
      background-color: #f8d7da;
      color: #721c24;
      padding: 1rem;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
      margin-bottom: 1rem;
    }
    .error-message p {
      margin: 0;
    }
  `]
})
export class ErrorMessageComponent {
  @Input() errorMessage: string | null = null;
}
