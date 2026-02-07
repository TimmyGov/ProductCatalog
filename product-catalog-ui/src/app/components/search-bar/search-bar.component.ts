import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-bar">
      <input
        type="text"
        [(ngModel)]="searchTerm"
        (ngModelChange)="onSearchChange($event)"
        placeholder="Search products..."
        class="search-input"
      />
    </div>
  `,
  styles: [`
    .search-bar {
      margin-bottom: 1rem;
    }
    .search-input {
      width: 100%;
      padding: 0.5rem;
      font-size: 1rem;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
  `]
})
export class SearchBarComponent {
  @Output() searchChange = new EventEmitter<string>();
  searchTerm = '';

  onSearchChange(term: string): void {
    this.searchChange.emit(term);
  }
}
