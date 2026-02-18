import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, KeyValuePipe, TitleCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HealthService } from '../services/health.service';
import { 
  HealthCheckResponse, 
  HealthComponent, 
  HealthStatus, 
  ExceptionResponse,
  TrendingExceptionsResponse,
  ExceptionsListResponse,
  RequestStatsResponse,
  RequestsListResponse,
  ExceptionLogEntry
} from '../models/health.model';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, RouterLink, KeyValuePipe, TitleCasePipe],
  templateUrl: './admin.html',
  styleUrl: './admin.scss'
})
export class Admin implements OnInit, OnDestroy {
  private readonly healthService = inject(HealthService);
  private refreshInterval: any;

  protected readonly healthData = signal<HealthCheckResponse | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);
  protected readonly autoRefresh = signal<boolean>(true);
  protected readonly lastUpdated = signal<Date | null>(null);
  protected readonly exceptionInfo = signal<ExceptionResponse | null>(null);
  protected readonly triggeringException = signal<boolean>(false);

  // Trending exceptions
  protected readonly trendingExceptions = signal<TrendingExceptionsResponse | null>(null);
  protected readonly loadingTrending = signal<boolean>(false);
  protected readonly trendingExpanded = signal<boolean>(false);

  // Exceptions list
  protected readonly exceptionsList = signal<ExceptionsListResponse | null>(null);
  protected readonly loadingExceptions = signal<boolean>(false);
  protected readonly exceptionsPage = signal<number>(1);
  protected readonly selectedException = signal<ExceptionLogEntry | null>(null);

  // Request stats
  protected readonly requestStats = signal<RequestStatsResponse | null>(null);
  protected readonly loadingRequestStats = signal<boolean>(false);
  protected readonly requestStatsExpanded = signal<boolean>(false);

  // Request list
  protected readonly requestsList = signal<RequestsListResponse | null>(null);
  protected readonly loadingRequests = signal<boolean>(false);
  protected readonly requestsPage = signal<number>(1);

  ngOnInit(): void {
    this.loadHealthStatus();
    this.loadTrendingExceptions();
    this.loadRequestStats();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.stopAutoRefresh();
  }

  protected loadHealthStatus(): void {
    this.loading.set(true);
    this.error.set(null);

    this.healthService.getHealthStatus().subscribe({
      next: (data: HealthCheckResponse) => {
        this.healthData.set(data);
        this.lastUpdated.set(new Date());
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set('Failed to load health status. The API may be unavailable.');
        this.loading.set(false);
      }
    });
  }

  protected toggleAutoRefresh(): void {
    this.autoRefresh.update(val => !val);
    if (this.autoRefresh()) {
      this.startAutoRefresh();
    } else {
      this.stopAutoRefresh();
    }
  }

  private startAutoRefresh(): void {
    this.stopAutoRefresh(); // Clear any existing interval
    this.refreshInterval = setInterval(() => {
      if (this.autoRefresh()) {
        this.loadHealthStatus();
      }
    }, 30000); // Refresh every 30 seconds
  }

  private stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
      this.refreshInterval = null;
    }
  }

  protected forceException(): void {
    this.triggeringException.set(true);
    this.exceptionInfo.set(null);

    this.healthService.forceException().subscribe({
      next: (response: ExceptionResponse) => {
        // This shouldn't happen but handle it gracefully
        this.exceptionInfo.set(response);
        this.triggeringException.set(false);
      },
      error: (err: any) => {
        // Extract the exception info from the error response
        if (err.error && err.error.exceptionId) {
          this.exceptionInfo.set(err.error);
        } else {
          this.exceptionInfo.set({
            error: 'UnknownError',
            message: 'Failed to trigger exception',
            exceptionId: 'N/A',
            timestamp: new Date().toISOString(),
            traceId: 'N/A'
          });
        }
        this.triggeringException.set(false);
      }
    });
  }

  protected clearExceptionInfo(): void {
    this.exceptionInfo.set(null);
  }

  protected copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      // Success feedback could be added here if desired
    }).catch(err => {
      console.error('Failed to copy to clipboard:', err);
    });
  }

  protected getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'healthy':
        return 'status-healthy';
      case 'degraded':
        return 'status-degraded';
      case 'unhealthy':
        return 'status-unhealthy';
      default:
        return 'status-unknown';
    }
  }

  protected getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'healthy':
        return '✓';
      case 'degraded':
        return '⚠';
      case 'unhealthy':
        return '✗';
      default:
        return '?';
    }
  }

  protected formatDuration(ms: number): string {
    if (ms < 1) {
      return '<1ms';
    } else if (ms < 1000) {
      return `${Math.round(ms)}ms`;
    } else {
      return `${(ms / 1000).toFixed(2)}s`;
    }
  }

  protected formatTimestamp(timestamp: string): string {
    return new Date(timestamp).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  protected getRelativeTime(date: Date): string {
    const seconds = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    
    if (seconds < 10) return 'just now';
    if (seconds < 60) return `${seconds}s ago`;
    
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    
    const hours = Math.floor(minutes / 60);
    return `${hours}h ago`;
  }

  // Trending exceptions methods
  protected loadTrendingExceptions(): void {
    this.loadingTrending.set(true);

    this.healthService.getTrendingExceptions(24).subscribe({
      next: (data) => {
        this.trendingExceptions.set(data);
        this.loadingTrending.set(false);
      },
      error: () => {
        this.loadingTrending.set(false);
      }
    });
  }

  protected toggleTrendingExpanded(): void {
    const isExpanding = !this.trendingExpanded();
    this.trendingExpanded.set(isExpanding);
    
    if (isExpanding && !this.exceptionsList()) {
      this.loadExceptionsList();
    }
  }

  protected loadExceptionsList(page: number = 1): void {
    this.loadingExceptions.set(true);
    this.exceptionsPage.set(page);

    this.healthService.getExceptions(page, 20, 24).subscribe({
      next: (data) => {
        this.exceptionsList.set(data);
        this.loadingExceptions.set(false);
      },
      error: () => {
        this
.loadingExceptions.set(false);
      }
    });
  }

  protected selectException(exception: ExceptionLogEntry): void {
    this.selectedException.set(exception);
  }

  protected clearSelectedException(): void {
    this.selectedException.set(null);
  }

  protected goToExceptionsPage(page: number): void {
    if (page >= 1 && this.exceptionsList() && page <= this.exceptionsList()!.totalPages) {
      this.loadExceptionsList(page);
    }
  }

  // Request stats methods
  protected loadRequestStats(): void {
    this.loadingRequestStats.set(true);

    this.healthService.getRequestStats(24).subscribe({
      next: (data) => {
        this.requestStats.set(data);
        this.loadingRequestStats.set(false);
      },
      error: () => {
        this.loadingRequestStats.set(false);
      }
    });
  }

  protected toggleRequestStatsExpanded(): void {
    const isExpanding = !this.requestStatsExpanded();
    this.requestStatsExpanded.set(isExpanding);
    
    if (isExpanding && !this.requestsList()) {
      this.loadRequestsList();
    }
  }

  protected loadRequestsList(page: number = 1): void {
    this.loadingRequests.set(true);
    this.requestsPage.set(page);

    this.healthService.getRequests(page, 20, 24).subscribe({
      next: (data) => {
        this.requestsList.set(data);
        this.loadingRequests.set(false);
      },
      error: () => {
        this.loadingRequests.set(false);
      }
    });
  }

  protected goToRequestsPage(page: number): void {
    if (page >= 1 && this.requestsList() && page <= this.requestsList()!.totalPages) {
      this.loadRequestsList(page);
    }
  }

  protected getStatusCodeClass(statusCode: number): string {
    if (statusCode >= 200 && statusCode < 300) {
      return 'status-success';
    } else if (statusCode >= 300 && statusCode < 400) {
      return 'status-redirect';
    } else if (statusCode >= 400 && statusCode < 500) {
      return 'status-client-error';
    } else {
      return 'status-server-error';
    }
  }
}
