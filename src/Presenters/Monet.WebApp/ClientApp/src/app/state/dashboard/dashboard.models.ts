export interface AccountBalanceReadModel {
  accountNumber: string;
  balance: number;
  currency: string;
}

export interface Amount {
  value: number;
  currency: string;
}

export type TransactionFlowType = 'Expense' | 'Income' | 'Neutral';

export interface TransactionCategoryReadModel {
  categoryId: string;
  name: string;
  flowType: Exclude<TransactionFlowType, 'Neutral'>;
}

export interface TransactionReadModel {
  transactionId: string;
  amount: Amount;
  date: string;
  description: string;
  accountNumber: string;
  flowType: TransactionFlowType;
  categoryId: string | null;
  categoryName: string | null;
}

export interface DashboardState {
  accountBalances: AccountBalanceReadModel[];
  transactions: TransactionReadModel[];
  categories: TransactionCategoryReadModel[];
  loading: boolean;
  error: string | null;
}
