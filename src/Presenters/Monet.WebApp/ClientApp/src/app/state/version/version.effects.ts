import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';

import { VersionActions } from './version.actions';
import { VersionResponse } from './version.models';

@Injectable({ providedIn: 'root' })
export class VersionEffects {
  private readonly actions$ = inject(Actions);
  private readonly http = inject(HttpClient);

  readonly loadVersion$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VersionActions.loadVersion),
      switchMap(() =>
        this.http.get<VersionResponse>('/api/version').pipe(
          map((data) => VersionActions.loadVersionSuccess({ data })),
          catchError((error) =>
            of(
              VersionActions.loadVersionFailure({
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

    return 'Unexpected error while loading version.';
  }
}
