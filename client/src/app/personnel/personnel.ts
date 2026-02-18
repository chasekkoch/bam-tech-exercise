import { Component, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PersonService } from '../services/person.service';
import { Person } from '../models/person.model';

@Component({
  selector: 'app-personnel',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './personnel.html',
  styleUrl: './personnel.scss'
})
export class Personnel {
  private readonly personService = inject(PersonService);

  // Signals for reactive state management
  protected readonly people = signal<Person[]>([]);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly searchTerm = signal<string>('');
  protected readonly sortField = signal<'name' | 'currentRank' | 'currentDutyTitle'>('name');
  protected readonly sortAscending = signal<boolean>(true);
  protected readonly currentPage = signal<number>(1);
  protected readonly itemsPerPage = signal<number>(10);

  // Computed values
  protected readonly filteredPeople = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const allPeople = this.people();

    if (!term) return allPeople;

    return allPeople.filter(person =>
      person.name.toLowerCase().includes(term) ||
      person.currentRank?.toLowerCase().includes(term) ||
      person.currentDutyTitle?.toLowerCase().includes(term)
    );
  });

  protected readonly sortedPeople = computed(() => {
    const filtered = [...this.filteredPeople()];
    const field = this.sortField();
    const ascending = this.sortAscending();

    filtered.sort((a, b) => {
      let aVal: string;
      let bVal: string;

      if (field === 'name') {
        aVal = a.name;
        bVal = b.name;
      } else {
        aVal = field === 'currentRank' 
          ? (a.currentRank || '') 
          : (a.currentDutyTitle || '');
        bVal = field === 'currentRank' 
          ? (b.currentRank || '') 
          : (b.currentDutyTitle || '');
      }

      const comparison = aVal.localeCompare(bVal);
      return ascending ? comparison : -comparison;
    });

    return filtered;
  });

  protected readonly paginatedPeople = computed(() => {
    const sorted = this.sortedPeople();
    const page = this.currentPage();
    const perPage = this.itemsPerPage();
    const start = (page - 1) * perPage;
    const end = start + perPage;
    
    return sorted.slice(start, end);
  });

  protected readonly totalPages = computed(() => {
    return Math.ceil(this.sortedPeople().length / this.itemsPerPage());
  });

  protected readonly astronautCount = computed(() => {
    // Only count active astronauts (have careerStartDate but no careerEndDate)
    return this.people().filter(p => p.careerStartDate && !p.careerEndDate).length;
  });

  protected readonly nonAstronautCount = computed(() => {
    return this.people().filter(p => !p.careerStartDate).length;
  });

  constructor() {
    this.loadPeople();
  }

  protected loadPeople(): void {
    this.loading.set(true);
    this.error.set(null);

    this.personService.getPeople().subscribe({
      next: (data: Person[]) => {
        this.people.set(data);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  protected onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1); // Reset to first page on search
  }

  protected sortBy(field: 'name' | 'currentRank' | 'currentDutyTitle'): void {
    if (this.sortField() === field) {
      this.sortAscending.update(val => !val);
    } else {
      this.sortField.set(field);
      this.sortAscending.set(true);
    }
  }

  protected goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      // Scroll to top of table
      document.querySelector('.personnel-table')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  protected getStatusClass(person: Person): string {
    if (!person.careerStartDate) return 'non-astronaut';
    if (person.careerEndDate) return 'retired';
    return 'active';
  }

  protected getStatusLabel(person: Person): string {
    if (!person.careerStartDate) return 'Personnel';
    if (person.careerEndDate) return 'Retired';
    return 'Active';
  }

  protected getSortIcon(field: string): string {
    if (this.sortField() !== field) return '↕️';
    return this.sortAscending() ? '↑' : '↓';
  }

  protected formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
