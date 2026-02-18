import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { RegisterRequest } from '../models/auth.model';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrls: ['./register.scss']
})
export class RegisterComponent {
  email = signal('');
  password = signal('');
  confirmPassword = signal('');
  firstName = signal('');
  lastName = signal('');
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    // Validation
    if (!this.email() || !this.password() || !this.confirmPassword()) {
      this.error.set('Please fill in all required fields');
      return;
    }

    if (this.password() !== this.confirmPassword()) {
      this.error.set('Passwords do not match');
      return;
    }

    if (this.password().length < 8) {
      this.error.set('Password must be at least 8 characters long');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const request: RegisterRequest = {
      email: this.email(),
      password: this.password(),
      firstName: this.firstName() || undefined,
      lastName: this.lastName() || undefined
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.loading.set(false);
        // Navigate to people page after successful registration
        this.router.navigate(['/people']);
      },
      error: (err) => {
        this.loading.set(false);
        const errors = err.error?.errors;
        if (errors && Array.isArray(errors)) {
          this.error.set(errors.join(', '));
        } else {
          this.error.set(err.error?.message || 'Registration failed. Please try again.');
        }
      }
    });
  }
}
