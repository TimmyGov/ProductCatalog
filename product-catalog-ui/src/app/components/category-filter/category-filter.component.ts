import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../../models';

@Component({
  selector: 'app-category-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="category-filter">
      <select
        [(ngModel)]="selectedCategoryId"
        (ngModelChange)="onCategoryChange($event)"
        class="category-select"
      >
        <option [value]="''">All Categories</option>
        <option *ngFor="let category of categories" [value]="category.id">
          {{ category.name }}
        </option>
      </select>
    </div>
  `,
  styles: [`
    .category-filter {
      margin-bottom: 1rem;
    }
    .category-select {
      width: 100%;
      padding: 0.5rem;
      font-size: 1rem;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
  `]
})
export class CategoryFilterComponent {
  @Input() categories: Category[] = [];
  @Output() categoryChange = new EventEmitter<number | null>();
  selectedCategoryId: string | number = '';

  onCategoryChange(categoryId: string | number): void {
    const numericCategoryId = categoryId === '' ? null : Number(categoryId);
    this.categoryChange.emit(numericCategoryId);
  }
}
