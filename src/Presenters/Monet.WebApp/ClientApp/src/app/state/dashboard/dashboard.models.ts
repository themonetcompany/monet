export interface AccountBalanceReadModel {
  accountNumber: string;
  balance: number;
  currency: string;
}

export interface Amount {
  value: number;
  currency: string;
}

export interface TransactionReadModel {
  transactionId: string;
  amount: Amount;
  date: string;
  description: string;
  accountNumber: string;
}

export interface DashboardState {
  accountBalances: AccountBalanceReadModel[];
  transactions: TransactionReadModel[];
  loading: boolean;
  error: string | null;
}
