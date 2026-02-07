import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private searchTermSubject = new Subject<string>();
  private currentSearchTerm = new BehaviorSubject<string>('');

  searchTerm$: Observable<string> = this.searchTermSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );

  currentSearchTerm$: Observable<string> = this.currentSearchTerm.asObservable();

  setSearchTerm(term: string): void {
    this.searchTermSubject.next(term);
    this.currentSearchTerm.next(term);
  }

  clearSearch(): void {
    this.setSearchTerm('');
  }

  getCurrentSearchTerm(): string {
    return this.currentSearchTerm.value;
  }
}
