import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PersonService } from '../services/person.service';
import { Person, ApiResponse } from '../models/person.model';

@Component({
  selector: 'app-add-person',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './add-person.html',
  styleUrl: './add-person.scss'
})
export class AddPerson {
  private readonly personService = inject(PersonService);
  private readonly router = inject(Router);

  protected readonly name = signal<string>('');
  protected readonly submitting = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<boolean>(false);

  protected onSubmit(event: Event): void {
    event.preventDefault();
    
    const nameValue = this.name().trim();
    if (!nameValue) {
      this.error.set('Name is required');
      return;
    }

    if (nameValue.length < 2) {
      this.error.set('Name must be at least 2 characters');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.personService.createPerson(nameValue).subscribe({
      next: (response: ApiResponse<Person>) => {
        this.success.set(true);
        this.submitting.set(false);
        
        // Redirect to the person detail page
        setTimeout(() => {
          this.router.navigate(['/personnel', nameValue]);
        }, 1500);
      },
      error: (err: Error) => {
        this.error.set(err.message || 'Failed to create person');
        this.submitting.set(false);
      }
    });
  }

  protected onNameChange(value: string): void {
    this.name.set(value);
    if (this.error()) {
      this.error.set(null);
    }
  }
}
