import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, delay, map, switchMap } from 'rxjs/operators';

import { DashboardActions } from './dashboard.actions';
import { dashboardMockData } from './dashboard.mock';

@Injectable()
export class DashboardEffects {
  private readonly actions$ = inject(Actions);

  readonly loadDashboard$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboard),
      switchMap(() =>
        of(dashboardMockData).pipe(
          delay(250),
          map((payload) => DashboardActions.loadDashboardSuccess(payload)),
          catchError(() =>
            of(
              DashboardActions.loadDashboardFailure({
                error: 'Unable to load dashboard data.'
              })
            )
          )
        )
      )
    )
  );

}
