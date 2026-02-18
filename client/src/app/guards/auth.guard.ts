import { inject } from '@angular/core';
import { Router, CanActivateFn, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable, map, filter, take } from 'rxjs';

/**
 * Guard to protect routes that require authentication
 */
export const authGuard: CanActivateFn = (): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for initialization to complete before checking authentication
  return authService.initialized$.pipe(
    filter(initialized => initialized),
    take(1),
    map(() => {
      if (authService.isAuthenticated()) {
        return true;
      }
      // Redirect to login if not authenticated
      return router.createUrlTree(['/login']);
    })
  );
};

/**
 * Guard to protect routes that require Admin role
 */
export const adminGuard: CanActivateFn = (): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for initialization to complete before checking admin status
  return authService.initialized$.pipe(
    filter(initialized => initialized),
    take(1),
    map(() => {
      if (authService.isAdmin()) {
        return true;
      }
      // Redirect to home if not admin
      return router.createUrlTree(['/']);
    })
  );
};

/**
 * Guard to redirect already authenticated users away from login/register
 */
export const guestGuard: CanActivateFn = (): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for initialization to complete before checking authentication
  return authService.initialized$.pipe(
    filter(initialized => initialized),
    take(1),
    map(() => {
      if (!authService.isAuthenticated()) {
        return true;
      }
      // Redirect to home if already authenticated
      return router.createUrlTree(['/']);
    })
  );
};
