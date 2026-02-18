import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AstronautDutyService } from '../services/astronaut-duty.service';
import { PersonService } from '../services/person.service';
import { CreateAstronautDutyRequest, ApiResponse, AstronautDuty, Person } from '../models/person.model';

@Component({
  selector: 'app-add-duty',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './add-duty.html',
  styleUrl: './add-duty.scss'
})
export class AddDuty implements OnInit {
  private readonly dutyService = inject(AstronautDutyService);
  private readonly personService = inject(PersonService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly name = signal<string>('');
  protected readonly rank = signal<string>('');
  protected readonly dutyTitle = signal<string>('');
  protected readonly dutyStartDate = signal<string>('');
  protected readonly submitting = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<boolean>(false);
  protected readonly existingDuties = signal<AstronautDuty[]>([]);
  protected readonly allPeople = signal<Person[]>([]);
  protected readonly isLoadingPeople = signal<boolean>(true);
  protected readonly showDropdown = signal<boolean>(false);
  protected readonly filteredPeople = signal<Person[]>([]);
  protected readonly highlightedIndex = signal<number>(-1);

  // Predefined ranks and titles
  protected readonly ranks = [
    'O-1 Second Lieutenant',
    'O-2 First Lieutenant',
    'O-3 Captain',
    'O-4 Major',
    'O-5 Lieutenant Colonel',
    'O-6 Colonel',
    'O-7 Brigadier General',
    'O-8 Major General',
    'O-9 Lieutenant General',
    'O-10 General'
  ];

  protected readonly duties = [
    'Mission Specialist',
    'Pilot',
    'Commander',
    'Flight Engineer',
    'Science Officer',
    'Medical Officer',
    'Communications Officer',
    'Navigation Officer',
    'Security Officer',
    'RETIRED'
  ];

  ngOnInit(): void {
    // Load all people for the dropdown
    this.loadAllPeople();

    // Check if name is provided in query params
    const nameParam = this.route.snapshot.queryParamMap.get('name');
    if (nameParam) {
      this.name.set(nameParam);
      this.loadExistingDuties(nameParam);
    }
  }

  private loadAllPeople(): void {
    this.isLoadingPeople.set(true);
    this.personService.getPeople().subscribe({
      next: (people: Person[]) => {
        this.allPeople.set(people);
        this.filteredPeople.set(people);
        this.isLoadingPeople.set(false);
      },
      error: () => {
        this.allPeople.set([]);
        this.filteredPeople.set([]);
        this.isLoadingPeople.set(false);
        this.error.set('Failed to load personnel list');
      }
    });
  }

  private loadExistingDuties(name: string): void {
    if (!name.trim()) {
      this.existingDuties.set([]);
      return;
    }

    this.dutyService.getAstronautDutiesByName(name).subscribe({
      next: (duties: AstronautDuty[]) => {
        this.existingDuties.set(duties);
      },
      error: () => {
        // If person not found or no duties, just set empty array
        this.existingDuties.set([]);
      }
    });
  }

  protected onNameChange(): void {
    const nameValue = this.name();
    
    // Filter the people list based on input
    const filtered = this.allPeople().filter(person =>
      person.name.toLowerCase().includes(nameValue.toLowerCase())
    );
    this.filteredPeople.set(filtered);
    this.highlightedIndex.set(-1);
    this.showDropdown.set(true);
    
    // Only load duties if there's an exact match
    const exactMatch = this.allPeople().find(p => p.name === nameValue);
    if (exactMatch) {
      this.loadExistingDuties(nameValue);
    } else {
      this.existingDuties.set([]);
    }
    this.onFieldChange();
  }

  protected onNameFocus(): void {
    this.showDropdown.set(true);
    this.filteredPeople.set(this.allPeople());
    this.highlightedIndex.set(-1);
  }

  protected onNameKeyDown(event: KeyboardEvent): void {
    const filtered = this.filteredPeople();
    
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      const newIndex = Math.min(this.highlightedIndex() + 1, filtered.length - 1);
      this.highlightedIndex.set(newIndex);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      const newIndex = Math.max(this.highlightedIndex() - 1, -1);
      this.highlightedIndex.set(newIndex);
    } else if (event.key === 'Enter') {
      const index = this.highlightedIndex();
      if (index >= 0 && index < filtered.length) {
        event.preventDefault();
        this.selectPerson(filtered[index]);
      }
    } else if (event.key === 'Escape') {
      this.showDropdown.set(false);
      this.highlightedIndex.set(-1);
    }
  }

  protected selectPerson(person: Person): void {
    this.name.set(person.name);
    this.showDropdown.set(false);
    this.highlightedIndex.set(-1);
    this.loadExistingDuties(person.name);
    this.onFieldChange();
  }

  protected closeDropdown(): void {
    // Delay to allow click events to process
    setTimeout(() => {
      this.showDropdown.set(false);
      this.highlightedIndex.set(-1);
    }, 200);
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    
    // Validation
    const nameValue = this.name().trim();
    const rankValue = this.rank().trim();
    const titleValue = this.dutyTitle().trim();
    const dateValue = this.dutyStartDate();

    if (!nameValue) {
      this.error.set('Name is required');
      return;
    }

    // Validate that the selected name exists in the database
    const personExists = this.allPeople().some(p => p.name === nameValue);
    if (!personExists) {
      this.error.set('Please select a valid person from the dropdown');
      return;
    }

    if (!rankValue) {
      this.error.set('Rank is required');
      return;
    }

    if (!titleValue) {
      this.error.set('Duty title is required');
      return;
    }

    if (!dateValue) {
      this.error.set('Start date is required');
      return;
    }

    // Check date is not in future
    const selectedDate = new Date(dateValue);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    if (selectedDate > today) {
      this.error.set('Start date cannot be in the future');
      return;
    }

    // Validate against existing duties to prevent 0 or negative duration
    const currentDuty = this.existingDuties().find(d => !d.dutyEndDate);
    if (currentDuty) {
      const currentDutyStartDate = new Date(currentDuty.dutyStartDate);
      currentDutyStartDate.setHours(0, 0, 0, 0);
      selectedDate.setHours(0, 0, 0, 0);
      
      // New duty must start at least 1 day after current duty to ensure positive duration
      if (selectedDate <= currentDutyStartDate) {
        const formattedDate = currentDutyStartDate.toLocaleDateString();
        this.error.set(
          `New duty start date must be after the current duty start date (${formattedDate}). ` +
          `This ensures the current duty has a duration of at least 1 day.`
        );
        return;
      }
    }

    this.submitting.set(true);
    this.error.set(null);

    const request: CreateAstronautDutyRequest = {
      name: nameValue,
      rank: rankValue,
      dutyTitle: titleValue,
      dutyStartDate: dateValue
    };

    this.dutyService.createAstronautDuty(request).subscribe({
      next: (response: ApiResponse<AstronautDuty>) => {
        this.success.set(true);
        this.submitting.set(false);
        
        // Redirect to the person detail page
        setTimeout(() => {
          this.router.navigate(['/personnel', nameValue]);
        }, 1500);
      },
      error: (err: Error) => {
        this.error.set(err.message || 'Failed to create astronaut duty');
        this.submitting.set(false);
      }
    });
  }

  protected onFieldChange(): void {
    if (this.error()) {
      this.error.set(null);
    }
  }

  protected getTodayDate(): string {
    return new Date().toISOString().split('T')[0];
  }
}
