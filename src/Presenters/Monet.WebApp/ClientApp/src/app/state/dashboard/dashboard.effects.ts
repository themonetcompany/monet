import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';

import { DashboardActions } from './dashboard.actions';
import { AccountBalanceReadModel, TransactionReadModel } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardEffects {
  private readonly actions$ = inject(Actions);
  private readonly http = inject(HttpClient);

  readonly loadDashboard$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDashboard),
      switchMap(() =>
        forkJoin({
          accountBalances: this.http.get<AccountBalanceReadModel[]>('/api/accounts/balances'),
          transactions: this.http.get<TransactionReadModel[]>('/api/transactions')
        }).pipe(
          map(({ accountBalances, transactions }) =>
            DashboardActions.loadDashboardSuccess({
              accountBalances: [...accountBalances].sort((left, right) =>
                left.accountNumber.localeCompare(right.accountNumber)
              ),
              transactions: [...transactions].sort(
                (left, right) => new Date(right.date).getTime() - new Date(left.date).getTime()
              )
            })
          ),
          catchError((error) =>
            of(
              DashboardActions.loadDashboardFailure({
                error: this.formatError(error)
              })
            )
          )
        )
      )
    )
  );

  private formatError(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Vous devez être authentifié pour voir le dashboard.';
    }

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

    return 'Impossible de charger les données du dashboard.';
  }
}
