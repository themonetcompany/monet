import { AsyncPipe, CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { DashboardActions } from '../../state/dashboard/dashboard.actions';
import { AccountBalanceReadModel, TransactionReadModel } from '../../state/dashboard/dashboard.models';
import {
  selectDashboardAccountBalances,
  selectDashboardError,
  selectDashboardLoading,
  selectDashboardTransactions
} from '../../state/dashboard/dashboard.selectors';

interface CurrencyTotal {
  currency: string;
  total: number;
}

@Component({
  selector: 'app-dashboard-page',
  imports: [AsyncPipe, CurrencyPipe, DatePipe],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPage implements OnInit {
  private readonly store = inject(Store);

  readonly loading$ = this.store.select(selectDashboardLoading);
  readonly error$ = this.store.select(selectDashboardError);
  readonly accountBalances$ = this.store.select(selectDashboardAccountBalances);
  readonly transactions$ = this.store.select(selectDashboardTransactions);
  readonly accountCount$ = this.accountBalances$.pipe(map((accountBalances) => accountBalances.length));
  readonly transactionCount$ = this.transactions$.pipe(map((transactions) => transactions.length));
  readonly totalBalanceByCurrency$ = this.accountBalances$.pipe(
    map((accountBalances) => this.computeTotalBalanceByCurrency(accountBalances))
  );

  ngOnInit(): void {
    this.store.dispatch(DashboardActions.loadDashboard());
  }

  refresh(): void {
    this.store.dispatch(DashboardActions.loadDashboard());
  }

  private computeTotalBalanceByCurrency(accountBalances: AccountBalanceReadModel[]): CurrencyTotal[] {
    const totals = new Map<string, number>();
    for (const account of accountBalances) {
      const current = totals.get(account.currency) ?? 0;
      totals.set(account.currency, current + account.balance);
    }

    return [...totals.entries()]
      .map(([currency, total]) => ({ currency, total }))
      .sort((a, b) => a.currency.localeCompare(b.currency));
  }
}
