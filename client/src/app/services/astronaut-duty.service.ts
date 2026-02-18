import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AstronautDuty, CreateAstronautDutyRequest, ApiResponse } from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class AstronautDutyService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/AstronautDuty';

  /**
   * Get astronaut duties by person name
   */
  getAstronautDutiesByName(name: string): Observable<AstronautDuty[]> {
    return this.http.get<{ astronautDuties: AstronautDuty[] }>(`${this.apiUrl}/${encodeURIComponent(name)}`).pipe(
      map(response => response.astronautDuties),
      catchError(this.handleError)
    );
  }

  /**
   * Create a new astronaut duty
   */
  createAstronautDuty(request: CreateAstronautDutyRequest): Observable<ApiResponse<AstronautDuty>> {
    return this.http.post<ApiResponse<AstronautDuty>>(this.apiUrl, request, {
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
