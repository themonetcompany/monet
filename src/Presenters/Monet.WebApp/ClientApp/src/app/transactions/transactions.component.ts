import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { combineLatest, firstValueFrom, map } from 'rxjs';

import { categoryDefinitions } from '../data/categories.data';
import { AccountsService } from '../services/accounts.service';
import { FocusDefaultAccountService } from '../services/focus-default-account.service';
import { TransactionsApiService } from '../services/transactions-api.service';
import {
  selectDashboardAreas,
  selectDashboardProjects
} from '../state/dashboard/dashboard.selectors';

interface CategoryOption {
  id: string;
  name: string;
  type: 'income' | 'expense';
}

interface FocusOption {
  id: string;
  label: string;
  type: 'project' | 'area';
}

interface TransactionItem {
  id: string;
  label: string;
  amount: number;
  type: 'income' | 'expense';
  accountId: string;
  categoryId: string;
  focusType: 'project' | 'area';
  focusId: string;
}

@Component({
  selector: 'app-transactions',
  imports: [CurrencyPipe],
  templateUrl: './transactions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionsComponent {
  private readonly store = inject(Store);
  private readonly accountsService = inject(AccountsService);
  private readonly focusDefaults = inject(FocusDefaultAccountService);
  private readonly transactionsApi = inject(TransactionsApiService);

  readonly accounts = this.accountsService.accounts;

  readonly categories = signal<CategoryOption[]>(
    categoryDefinitions.map((category) => ({
      id: category.id,
      name: category.name,
      type: category.type
    }))
  );

  private readonly focusOptions$ = combineLatest([
    this.store.select(selectDashboardProjects),
    this.store.select(selectDashboardAreas)
  ]).pipe(
    map(([projects, areas]) => {
      const projectOptions = projects.map((project) => ({
        id: project.id,
        label: `Projet · ${project.name}`,
        type: 'project' as const
      }));
      const areaOptions = areas.map((area) => ({
        id: area.id,
        label: `Domaine · ${area.name}`,
        type: 'area' as const
      }));
      return [...projectOptions, ...areaOptions];
    })
  );

  readonly focusOptions = toSignal(this.focusOptions$, { initialValue: [] as FocusOption[] });

  readonly transactions = signal<TransactionItem[]>([
    {
      id: 'txn-1',
      label: 'Supermarché',
      amount: 128,
      type: 'expense',
      accountId: 'account-1',
      categoryId: 'cat-expense-groceries',
      focusType: 'area',
      focusId: 'area-family'
    },
    {
      id: 'txn-2',
      label: 'Réservation chalet',
      amount: 490,
      type: 'expense',
      accountId: 'account-2',
      categoryId: 'cat-expense-travel',
      focusType: 'project',
      focusId: 'project-ski-2026'
    }
  ]);

  readonly selectedTransaction = signal<TransactionItem | null>(null);
  readonly isSidePanelOpen = signal(false);

  readonly isModalOpen = signal(false);
  readonly isImportOpen = signal(false);
  readonly importFiles = signal<File[]>([]);
  readonly formError = signal('');
  readonly formLabel = signal('');
  readonly formAmount = signal('');
  readonly formType = signal<'income' | 'expense'>('expense');
  readonly formAccountId = signal('');
  readonly formCategoryId = signal('');
  readonly formFocusKey = signal('');

  readonly focusLabelById = computed(() => {
    const map = new Map<string, string>();
    this.focusOptions().forEach((item) => {
      map.set(`${item.type}:${item.id}`, item.label);
    });
    return map;
  });

  readonly accountLabelById = computed(() => {
    const map = new Map<string, string>();
    this.accounts().forEach((account) => map.set(account.id, account.name));
    return map;
  });

  readonly categoryLabelById = computed(() => {
    const map = new Map<string, string>();
    this.categories().forEach((category) => map.set(category.id, category.name));
    return map;
  });

  readonly filteredCategories = computed(() => {
    return this.categories().filter((category) => category.type === this.formType());
  });

  openModal(): void {
    this.formError.set('');
    this.formLabel.set('');
    this.formAmount.set('');
    this.formType.set('expense');
    this.formCategoryId.set(this.filteredCategories()[0]?.id ?? '');
    const firstFocus = this.focusOptions()[0];
    const focusKey = firstFocus ? `${firstFocus.type}:${firstFocus.id}` : '';
    this.formFocusKey.set(focusKey);
    this.applyDefaultAccount(focusKey);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  openSidePanel(transaction: TransactionItem): void {
    this.selectedTransaction.set({ ...transaction });
    this.isSidePanelOpen.set(true);
  }

  closeSidePanel(): void {
    this.isSidePanelOpen.set(false);
    this.selectedTransaction.set(null);
  }

  updateSelectedTransaction(patch: Partial<TransactionItem>): void {
    const current = this.selectedTransaction();
    if (!current) {
      return;
    }
    this.selectedTransaction.set({ ...current, ...patch });
  }

  saveSelectedTransaction(): void {
    const selected = this.selectedTransaction();
    if (!selected) {
      return;
    }

    this.transactions.update((items) =>
      items.map((item) => (item.id === selected.id ? { ...selected } : item))
    );
    this.closeSidePanel();
  }

  parseAmount(value: string): number {
    const parsed = Number(value.replace(',', '.'));
    return Number.isNaN(parsed) ? 0 : parsed;
  }

  openImport(): void {
    this.importFiles.set([]);
    this.isImportOpen.set(true);
  }

  closeImport(): void {
    this.isImportOpen.set(false);
  }

  onImportFilesChange(files: FileList | null): void {
    if (!files) {
      this.importFiles.set([]);
      return;
    }
    this.importFiles.set(Array.from(files));
  }

  async importOfx(): Promise<void> {
    const files = this.importFiles();
    if (files.length === 0) {
      return;
    }
    const defaultAccount = this.accounts()[0]?.id ?? '';
    const firstFocus = this.focusOptions()[0];
    const focusType = firstFocus?.type ?? 'project';
    const focusId = firstFocus?.id ?? '';
    const parsed = await firstValueFrom(this.transactionsApi.importOfx(files));

    this.transactions.update((items) => [
      ...parsed.map((entry, index) => ({
        id: `txn-import-${Date.now()}-${index}`,
        label: entry.label,
        amount: entry.amount,
        type: entry.type,
        accountId: defaultAccount,
        categoryId: entry.categoryId,
        focusType,
        focusId
      })),
      ...items
    ]);

    this.isImportOpen.set(false);
  }

  onFocusChange(value: string): void {
    this.formFocusKey.set(value);
    this.applyDefaultAccount(value);
  }

  onTypeChange(value: string): void {
    if (value !== 'income' && value !== 'expense') {
      return;
    }
    this.formType.set(value);
    this.formCategoryId.set(this.filteredCategories()[0]?.id ?? '');
  }

  private applyDefaultAccount(focusKey: string): void {
    if (!focusKey) {
      this.formAccountId.set(this.accounts()[0]?.id ?? '');
      return;
    }
    const [focusType, focusId] = focusKey.split(':');
    if (focusType !== 'project' && focusType !== 'area') {
      this.formAccountId.set(this.accounts()[0]?.id ?? '');
      return;
    }
    const defaultAccount = this.focusDefaults.getDefaultAccount(focusType, focusId);
    this.formAccountId.set(defaultAccount ?? this.accounts()[0]?.id ?? '');
  }

  submit(): void {
    const label = this.formLabel().trim();
    const amountValue = Number(this.formAmount().replace(',', '.'));
    const type = this.formType();
    const accountId = this.formAccountId();
    const categoryId = this.formCategoryId();
    const focusKey = this.formFocusKey();

    if (!label || Number.isNaN(amountValue) || amountValue <= 0 || !accountId || !categoryId || !focusKey) {
      this.formError.set('Merci de renseigner tous les champs avec un montant valide.');
      return;
    }

    const [focusType, focusId] = focusKey.split(':');

    if (focusType !== 'project' && focusType !== 'area') {
      this.formError.set('Merci de sélectionner un projet ou un domaine.');
      return;
    }

    this.transactions.update((items) => [
      {
        id: `txn-${items.length + 1}`,
        label,
        amount: amountValue,
        type,
        accountId,
        categoryId,
        focusType,
        focusId
      },
      ...items
    ]);

    this.isModalOpen.set(false);
  }

  formatFocusLabel(transaction: TransactionItem): string {
    return this.focusLabelById().get(`${transaction.focusType}:${transaction.focusId}`) ?? 'Non associé';
  }

  formatAccountLabel(transaction: TransactionItem): string {
    return this.accountLabelById().get(transaction.accountId) ?? 'Compte inconnu';
  }

  formatCategoryLabel(transaction: TransactionItem): string {
    return this.categoryLabelById().get(transaction.categoryId) ?? 'Catégorie inconnue';
  }
}
