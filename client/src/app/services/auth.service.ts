import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, of, catchError, BehaviorSubject } from 'rxjs';
import { 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  UserInfo, 
  User 
} from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = '/api/Auth';
  private readonly tokenKey = 'stargate_token';
  
  // Reactive state using signals
  private userSignal = signal<User | null>(null);
  
  // Track initialization state
  private initializationSubject = new BehaviorSubject<boolean>(false);
  public initialized$ = this.initializationSubject.asObservable();
  
  // Public computed signals
  user = computed(() => this.userSignal());
  isAuthenticated = computed(() => this.userSignal()?.isAuthenticated ?? false);
  isAdmin = computed(() => this.userSignal()?.isAdmin ?? false);

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Initialize user from stored token
    this.initializeUser();
  }

  /**
   * Initialize user state from stored token
   */
  private initializeUser(): void {
    const token = this.getToken();
    if (token) {
      // Verify token is still valid by calling /me endpoint
      this.getCurrentUser().subscribe({
        next: (userInfo) => {
          this.setUser(userInfo);
          this.initializationSubject.next(true);
        },
        error: () => {
          // Token is invalid or expired
          this.clearToken();
          this.initializationSubject.next(true);
        }
      });
    } else {
      // No token, initialization complete
      this.initializationSubject.next(true);
    }
  }

  /**
   * Register a new user
   */
  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => {
        this.setToken(response.token);
        this.setUser({
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          roles: response.roles
        });
      })
    );
  }

  /**
   * Login with email and password
   */
  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        this.setToken(response.token);
        this.setUser({
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          roles: response.roles
        });
      })
    );
  }

  /**
   * Logout current user
   */
  logout(): void {
    this.clearToken();
    this.userSignal.set(null);
    this.router.navigate(['/']);
  }

  /**
   * Get current user from API
   */
  getCurrentUser(): Observable<UserInfo> {
    return this.http.get<UserInfo>(`${this.apiUrl}/me`);
  }

  /**
   * Get stored JWT token
   */
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * Store JWT token
   */
  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  /**
   * Clear stored JWT token
   */
  private clearToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  /**
   * Set user state
   */
  private setUser(userInfo: UserInfo): void {
    const user: User = {
      email: userInfo.email,
      firstName: userInfo.firstName,
      lastName: userInfo.lastName,
      roles: userInfo.roles,
      isAuthenticated: true,
      isAdmin: userInfo.roles.includes('Admin')
    };
    this.userSignal.set(user);
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.userSignal();
    return user?.roles.includes(role) ?? false;
  }
}
