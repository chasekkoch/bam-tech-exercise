import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Person, CreatePersonRequest, ApiResponse } from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Person';

  /**
   * Get all people
   */
  getPeople(): Observable<Person[]> {
    return this.http.get<{ people: Person[] }>(this.apiUrl).pipe(
      map(response => response.people),
      catchError(this.handleError)
    );
  }

  /**
   * Get a person by name
   */
  getPersonByName(name: string): Observable<Person> {
    return this.http.get<{ person: Person }>(`${this.apiUrl}/${encodeURIComponent(name)}`).pipe(
      map(response => response.person),
      catchError(this.handleError)
    );
  }

  /**
   * Create a new person
   */
  createPerson(name: string): Observable<ApiResponse<Person>> {
    return this.http.post<ApiResponse<Person>>(this.apiUrl, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Server returned code ${error.status}: ${error.message}`;
      if (error.error?.message) {
        errorMessage = error.error.message;
      }
    }
    
    console.error('API Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
