import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PersonService } from '../services/person.service';
import { AstronautDutyService } from '../services/astronaut-duty.service';
import { Person, AstronautDuty } from '../models/person.model';

@Component({
  selector: 'app-person-detail',
  imports: [CommonModule, RouterLink],
  templateUrl: './person-detail.html',
  styleUrl: './person-detail.scss'
})
export class PersonDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly personService = inject(PersonService);
  private readonly dutyService = inject(AstronautDutyService);

  protected readonly person = signal<Person | null>(null);
  protected readonly duties = signal<AstronautDuty[]>([]);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly dutiesLoading = signal<boolean>(false);
  protected readonly dutiesError = signal<string | null>(null);

  protected readonly isAstronaut = computed(() => {
    return this.person()?.careerStartDate !== null && this.person()?.careerStartDate !== undefined;
  });

  protected readonly isRetired = computed(() => {
    const p = this.person();
    return p?.careerEndDate !== undefined && p.careerEndDate !== null;
  });

  protected readonly activeDuty = computed(() => {
    return this.duties().find(d => !d.dutyEndDate);
  });

  protected readonly pastDuties = computed(() => {
    return this.duties()
      .filter(d => d.dutyEndDate)
      .sort((a, b) => new Date(b.dutyStartDate).getTime() - new Date(a.dutyStartDate).getTime());
  });

  protected readonly careerDurationDays = computed(() => {
    const p = this.person();
    if (!p?.careerStartDate) return null;

    const start = new Date(p.careerStartDate);
    const end = p.careerEndDate ? new Date(p.careerEndDate) : new Date();
    const diff = end.getTime() - start.getTime();
    return Math.floor(diff / (1000 * 60 * 60 * 24));
  });

  ngOnInit(): void {
    const name = this.route.snapshot.paramMap.get('name');
    if (name) {
      this.loadPerson(name);
      this.loadDuties(name);
    }
  }

  protected loadPerson(name: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.personService.getPersonByName(name).subscribe({
      next: (data: Person) => {
        this.person.set(data);
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.loading.set(false);
      }
    });
  }

  protected loadDuties(name: string): void {
    this.dutiesLoading.set(true);
    this.dutiesError.set(null);

    this.dutyService.getAstronautDutiesByName(name).subscribe({
      next: (data: AstronautDuty[]) => {
        this.duties.set(data || []);
        this.dutiesLoading.set(false);
      },
      error: (err: Error) => {
        this.dutiesError.set(err.message);
        this.dutiesLoading.set(false);
      }
    });
  }

  protected formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  protected formatDateShort(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  protected calculateDuration(startDate: string, endDate?: string | null): string {
    const start = new Date(startDate);
    const end = endDate ? new Date(endDate) : new Date();
    const diff = end.getTime() - start.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    
    if (days < 30) return `${days} days`;
    if (days < 365) return `${Math.floor(days / 30)} months`;
    
    const years = Math.floor(days / 365);
    const months = Math.floor((days % 365) / 30);
    return months > 0 ? `${years}y ${months}m` : `${years} years`;
  }
}
