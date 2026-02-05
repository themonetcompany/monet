import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { combineLatest, map } from 'rxjs';

import { AccountsService } from '../services/accounts.service';
import { FocusDefaultAccountService } from '../services/focus-default-account.service';
import {
  selectDashboardAreas,
  selectDashboardProjects
} from '../state/dashboard/dashboard.selectors';

@Component({
  selector: 'app-focus',
  imports: [CurrencyPipe],
  templateUrl: './focus.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FocusComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly store = inject(Store);
  private readonly accountsService = inject(AccountsService);
  private readonly focusDefaults = inject(FocusDefaultAccountService);

  readonly activeTab = signal<'income' | 'expense'>('expense');
  readonly isConfigOpen = signal(false);
  readonly selectedAccountId = signal('');

  private readonly projects$ = this.store.select(selectDashboardProjects);
  private readonly areas$ = this.store.select(selectDashboardAreas);

  private readonly viewModel$ = combineLatest([
    this.route.queryParamMap,
    this.projects$,
    this.areas$
  ]).pipe(
    map(([params, projects, areas]) => {
      const type = params.get('type');
      const id = params.get('id');
      const project = type === 'project' ? projects.find((item) => item.id === id) : null;
      const area = type === 'area' ? areas.find((item) => item.id === id) : null;

      return {
        type,
        id,
        project,
        area
      };
    })
  );

  readonly viewModel = toSignal(this.viewModel$, {
    initialValue: { type: null, id: null, project: null, area: null }
  });

  readonly accounts = this.accountsService.accounts;

  readonly categories = computed(() => {
    const vm = this.viewModel();
    const selected = vm.project ?? vm.area;
    if (!selected) {
      return [] as Array<{ id: string; name: string; amount: number; colorClass: string }>;
    }
    return this.activeTab() === 'income'
      ? selected.incomeCategories
      : selected.expenseCategories;
  });

  readonly total = computed(() => {
    return this.categories().reduce((sum, item) => sum + item.amount, 0);
  });

  readonly transactions = computed(() => {
    const vm = this.viewModel();
    const selected = vm.project ?? vm.area;
    if (!selected) {
      return [] as Array<{ id: string; label: string; amount: number; dateLabel: string; categoryName: string }>;
    }
    return [...selected.incomeTransactions, ...selected.expenseTransactions];
  });

  setTab(tab: 'income' | 'expense'): void {
    this.activeTab.set(tab);
  }

  openConfig(): void {
    const vm = this.viewModel();
    if (!vm.type || !vm.id) {
      return;
    }
    const defaultAccount = this.focusDefaults.getDefaultAccount(vm.type as 'project' | 'area', vm.id);
    this.selectedAccountId.set(defaultAccount ?? this.accounts()[0]?.id ?? '');
    this.isConfigOpen.set(true);
  }

  closeConfig(): void {
    this.isConfigOpen.set(false);
  }

  saveConfig(): void {
    const vm = this.viewModel();
    if (!vm.type || !vm.id) {
      return;
    }
    if (!this.selectedAccountId()) {
      return;
    }
    this.focusDefaults.setDefaultAccount(vm.type as 'project' | 'area', vm.id, this.selectedAccountId());
    this.isConfigOpen.set(false);
  }

  donutBackground(categories: { amount: number; colorClass: string }[]): string {
    const colors: Record<string, string> = {
      'bg-emerald-400': '#34d399',
      'bg-emerald-300': '#6ee7b7',
      'bg-emerald-200': '#a7f3d0',
      'bg-blue-400': '#60a5fa',
      'bg-blue-300': '#93c5fd',
      'bg-blue-200': '#bfdbfe',
      'bg-amber-400': '#fbbf24',
      'bg-amber-300': '#fcd34d',
      'bg-amber-200': '#fde68a',
      'bg-rose-400': '#fb7185',
      'bg-rose-300': '#fda4af',
      'bg-rose-200': '#fecdd3',
      'bg-violet-400': '#a78bfa',
      'bg-violet-300': '#c4b5fd',
      'bg-violet-200': '#ddd6fe',
      'bg-stone-400': '#a8a29e',
      'bg-stone-300': '#d6d3d1',
      'bg-stone-200': '#e7e5e4'
    };
    const total = categories.reduce((sum, item) => sum + item.amount, 0);
    if (total <= 0) {
      return 'conic-gradient(#e7e5e4 0deg 360deg)';
    }
    let start = 0;
    const segments = categories.map((item) => {
      const angle = (item.amount / total) * 360;
      const end = start + angle;
      const color = colors[item.colorClass] ?? '#e7e5e4';
      const segment = `${color} ${start.toFixed(2)}deg ${end.toFixed(2)}deg`;
      start = end;
      return segment;
    });
    return `conic-gradient(${segments.join(',')})`;
  }

  categoryPercent(amount: number, total: number): number {
    if (total <= 0) {
      return 0;
    }
    return Math.round((amount / total) * 100);
  }
}
