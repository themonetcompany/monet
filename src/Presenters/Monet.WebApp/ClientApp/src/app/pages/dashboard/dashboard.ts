import { AsyncPipe, CurrencyPipe, DatePipe, JsonPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { catchError, map, Observable, of } from 'rxjs';

import { DashboardActions } from '../../state/dashboard/dashboard.actions';
import {
  AccountBalanceReadModel,
  TransactionCategoryReadModel,
  TransactionFlowType,
  TransactionReadModel
} from '../../state/dashboard/dashboard.models';
import {
  selectDashboardAccountBalances,
  selectDashboardExpenseCategories,
  selectDashboardError,
  selectDashboardIncomeCategories,
  selectDashboardLoading,
  selectDashboardTransactions
} from '../../state/dashboard/dashboard.selectors';
import { FormsModule } from '@angular/forms';

interface CurrencyTotal {
  currency: string;
  total: number;
}

@Component({
  selector: 'app-dashboard-page',
  imports: [AsyncPipe, CurrencyPipe, DatePipe, JsonPipe, FormsModule],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPage implements OnInit {
  private readonly store = inject(Store);

  readonly loading$ = this.store.select(selectDashboardLoading);
  readonly error$ = this.store.select(selectDashboardError);
  readonly accountBalances$ = this.store.select(selectDashboardAccountBalances);
  readonly transactions$ = this.store.select(selectDashboardTransactions);
  readonly expenseCategories$ = this.store.select(selectDashboardExpenseCategories);
  readonly incomeCategories$ = this.store.select(selectDashboardIncomeCategories);
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

  updateTransactionCategory(transaction: TransactionReadModel, event: Event): void {
    if (transaction.flowType === 'Neutral') {
      return;
    }

    const select = event.target as HTMLSelectElement;
    const categoryId = select.value === '' ? null : select.value;
    this.store.dispatch(
      DashboardActions.updateTransactionCategory({
        transactionId: transaction.transactionId,
        categoryId,
        previousCategoryId: transaction.categoryId,
        previousCategoryName: transaction.categoryName
      })
    );
  }

  categoriesForFlow(
    flowType: TransactionFlowType
  ): Observable<TransactionCategoryReadModel[]> {
    if (flowType.toLowerCase() === 'expense') {
      return this.expenseCategories$;
    }

    if (flowType.toLowerCase() === 'income') {
      return this.incomeCategories$
    }

    return of([]);
  }

  categoryOptionsForTransaction(
    transaction: TransactionReadModel
  ): Observable<TransactionCategoryReadModel[]> {
    const options = this.categoriesForFlow(transaction.flowType);
    if (!transaction.categoryId) {
      return options;
    }

    return options.pipe(map(o => {
      const alreadyPresent = o.some((category) => category.categoryId === transaction.categoryId);
      if (alreadyPresent) {
        return o;
      }

      return [
        {
          categoryId: transaction.categoryId,
          name: transaction.categoryName ?? 'Catégorie inconnue',
          flowType: transaction.flowType === 'Income' ? 'Income' : 'Expense'
        } as TransactionCategoryReadModel,
        ...o
      ];
    }), catchError(() => of([])))
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
