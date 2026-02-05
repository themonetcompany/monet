export type CategoryType = 'income' | 'expense';

export interface CategoryDefinition {
  id: string;
  name: string;
  type: CategoryType;
  colorClass: string;
}

export const categoryDefinitions: CategoryDefinition[] = [
  { id: 'cat-income-salary', name: 'Salaire', type: 'income', colorClass: 'bg-emerald-400' },
  { id: 'cat-income-bonus', name: 'Prime', type: 'income', colorClass: 'bg-emerald-300' },
  { id: 'cat-income-rental', name: 'Loyer perçu', type: 'income', colorClass: 'bg-emerald-200' },
  { id: 'cat-expense-groceries', name: 'Courses', type: 'expense', colorClass: 'bg-blue-400' },
  { id: 'cat-expense-housing', name: 'Logement', type: 'expense', colorClass: 'bg-blue-300' },
  { id: 'cat-expense-travel', name: 'Voyage', type: 'expense', colorClass: 'bg-amber-400' },
  { id: 'cat-expense-health', name: 'Santé', type: 'expense', colorClass: 'bg-rose-400' }
];

export const incomeCategories = categoryDefinitions.filter((item) => item.type === 'income');
export const expenseCategories = categoryDefinitions.filter((item) => item.type === 'expense');
