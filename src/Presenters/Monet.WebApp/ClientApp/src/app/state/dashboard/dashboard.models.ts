export interface DashboardSummary {
  greetingName: string;
  linkedAccounts: number;
  totalBalance: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  balanceChangeLabel: string;
  balanceChangeDirection: 'up' | 'down' | 'flat';
}

export interface ProjectSummary {
  id: string;
  name: string;
  icon: string;
  iconBgClass: string;
  progressClass: string;
  budgetTotal: number;
  budgetSpent: number;
  incomeCategories: CategorySummary[];
  expenseCategories: CategorySummary[];
  incomeTransactions: TransactionSummary[];
  expenseTransactions: TransactionSummary[];
}

export interface AreaSummary {
  id: string;
  name: string;
  icon: string;
  iconBgClass: string;
  monthlyBudget: number;
  monthlySpent: number;
  trendLabel: string;
  trendClass: string;
  incomeCategories: CategorySummary[];
  expenseCategories: CategorySummary[];
  incomeTransactions: TransactionSummary[];
  expenseTransactions: TransactionSummary[];
}

export interface ActivityItem {
  id: string;
  label: string;
  amount: number;
  dateLabel: string;
  category: string;
  projectId?: string;
  areaId?: string;
}

export interface CategorySummary {
  id: string;
  name: string;
  amount: number;
  colorClass: string;
}

export interface TransactionSummary {
  id: string;
  label: string;
  amount: number;
  dateLabel: string;
  categoryName: string;
}

export interface DashboardState {
  loading: boolean;
  error: string | null;
  summary: DashboardSummary | null;
  projects: ProjectSummary[];
  areas: AreaSummary[];
  recentActivity: ActivityItem[];
}
