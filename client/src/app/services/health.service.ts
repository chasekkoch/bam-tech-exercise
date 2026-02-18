import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  HealthCheckResponse, 
  ExceptionResponse, 
  TrendingExceptionsResponse,
  ExceptionsListResponse,
  RequestStatsResponse,
  RequestsListResponse
} from '../models/health.model';

@Injectable({
  providedIn: 'root'
})
export class HealthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Health';

  getHealthStatus(): Observable<HealthCheckResponse> {
    return this.http.get<HealthCheckResponse>(`${this.apiUrl}/status`);
  }

  getLiveness(): Observable<{ status: string; timestamp: string; message: string }> {
    return this.http.get<{ status: string; timestamp: string; message: string }>(`${this.apiUrl}/live`);
  }

  getReadiness(): Observable<{ status: string; timestamp: string; ready: boolean }> {
    return this.http.get<{ status: string; timestamp: string; ready: boolean }>(`${this.apiUrl}/ready`);
  }

  forceException(): Observable<ExceptionResponse> {
    return this.http.post<ExceptionResponse>(`${this.apiUrl}/force-exception`, {});
  }

  getTrendingExceptions(hours: number = 24): Observable<TrendingExceptionsResponse> {
    const params = new HttpParams().set('hours', hours.toString());
    return this.http.get<TrendingExceptionsResponse>(`${this.apiUrl}/exceptions/trending`, { params });
  }

  getExceptions(page: number = 1, pageSize: number = 20, hours: number = 24): Observable<ExceptionsListResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('hours', hours.toString());
    return this.http.get<ExceptionsListResponse>(`${this.apiUrl}/exceptions`, { params });
  }

  getRequestStats(hours: number = 24): Observable<RequestStatsResponse> {
    const params = new HttpParams().set('hours', hours.toString());
    return this.http.get<RequestStatsResponse>(`${this.apiUrl}/requests/stats`, { params });
  }

  getRequests(page: number = 1, pageSize: number = 20, hours: number = 24): Observable<RequestsListResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('hours', hours.toString());
    return this.http.get<RequestsListResponse>(`${this.apiUrl}/requests`, { params });
  }
}
