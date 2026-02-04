import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';

import { HealthActions } from './health.actions';
import { HealthStatus } from './health.models';

@Injectable({ providedIn: 'root' })
export class HealthEffects {
  private readonly actions$ = inject(Actions);
  private readonly http = inject(HttpClient);

  readonly loadHealth$ = createEffect(() =>
    this.actions$.pipe(
      ofType(HealthActions.loadHealth),
      switchMap(() =>
        this.http.get<HealthStatus>('/api/health').pipe(
          map((data) => HealthActions.loadHealthSuccess({ data })),
          catchError((error) =>
            of(
              HealthActions.loadHealthFailure({
                error: this.formatError(error)
              })
            )
          )
        )
      )
    )
  );

  private formatError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string') {
        return error.error;
      }

      if (error.message) {
        return error.message;
      }

      return `HTTP ${error.status}`;
    }

    if (error instanceof Error) {
      return error.message;
    }

    return 'Unexpected error while loading health status.';
  }
}
