import { expenseCategories, incomeCategories } from '../../data/categories.data';
import type {
  ActivityItem,
  AreaSummary,
  DashboardSummary,
  ProjectSummary
} from './dashboard.models';

const summary: DashboardSummary = {
  greetingName: 'Marie',
  linkedAccounts: 2,
  totalBalance: 12847.5,
  monthlyIncome: 4120,
  monthlyExpenses: 2865,
  balanceChangeLabel: '+3.2% ce mois',
  balanceChangeDirection: 'up'
};

const projects: ProjectSummary[] = [
  {
    id: 'project-ski-2026',
    name: 'Vacances ski 2026',
    icon: '🎿',
    iconBgClass: 'bg-blue-100',
    progressClass: 'bg-blue-500',
    budgetTotal: 3000,
    budgetSpent: 1950,
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [1200, 900, 300][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [820, 640, 490, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-ski-t1', label: 'Épargne mensuelle', amount: 800, dateLabel: '02 fév.', categoryName: 'Salaire' },
      { id: 'income-ski-t2', label: 'Prime hiver', amount: 900, dateLabel: '26 janv.', categoryName: 'Prime' },
      { id: 'income-ski-t3', label: 'Cashback vol', amount: 300, dateLabel: '12 janv.', categoryName: 'Loyer perçu' }
    ],
    expenseTransactions: [
      { id: 'expense-ski-t1', label: 'Chaussures de ski', amount: 320, dateLabel: '04 fév.', categoryName: 'Voyage' },
      { id: 'expense-ski-t2', label: 'Location casiers', amount: 140, dateLabel: '01 fév.', categoryName: 'Logement' },
      { id: 'expense-ski-t3', label: 'Arrhes chalet', amount: 490, dateLabel: '22 janv.', categoryName: 'Voyage' }
    ]
  },
  {
    id: 'project-kitchen',
    name: 'Rénovation cuisine',
    icon: '🍳',
    iconBgClass: 'bg-amber-100',
    progressClass: 'bg-amber-500',
    budgetTotal: 8000,
    budgetSpent: 2400,
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [4200, 1200, 0][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [1200, 800, 400, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-kitchen-t1', label: 'Transfert épargne', amount: 2200, dateLabel: '03 fév.', categoryName: 'Salaire' },
      { id: 'income-kitchen-t2', label: 'Cagnotte famille', amount: 1200, dateLabel: '18 janv.', categoryName: 'Prime' }
    ],
    expenseTransactions: [
      { id: 'expense-kitchen-t1', label: 'Meubles bas', amount: 700, dateLabel: '30 janv.', categoryName: 'Logement' },
      { id: 'expense-kitchen-t2', label: 'Four encastrable', amount: 800, dateLabel: '27 janv.', categoryName: 'Logement' },
      { id: 'expense-kitchen-t3', label: 'Pose crédence', amount: 400, dateLabel: '20 janv.', categoryName: 'Logement' }
    ]
  },
  {
    id: 'project-wedding',
    name: 'Week-end mariage',
    icon: '💍',
    iconBgClass: 'bg-rose-100',
    progressClass: 'bg-rose-500',
    budgetTotal: 5200,
    budgetSpent: 1880,
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [2600, 1400, 600][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [880, 620, 380, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-wedding-t1', label: 'Épargne couple', amount: 1800, dateLabel: '01 fév.', categoryName: 'Salaire' },
      { id: 'income-wedding-t2', label: 'Participation parents', amount: 1400, dateLabel: '15 janv.', categoryName: 'Prime' },
      { id: 'income-wedding-t3', label: 'Cadeaux', amount: 600, dateLabel: '05 janv.', categoryName: 'Loyer perçu' }
    ],
    expenseTransactions: [
      { id: 'expense-wedding-t1', label: 'Arrhes lieu', amount: 880, dateLabel: '28 janv.', categoryName: 'Voyage' },
      { id: 'expense-wedding-t2', label: 'Menu dégustation', amount: 320, dateLabel: '22 janv.', categoryName: 'Voyage' },
      { id: 'expense-wedding-t3', label: 'Décoration', amount: 380, dateLabel: '18 janv.', categoryName: 'Logement' }
    ]
  },
  {
    id: 'project-thailand',
    name: 'Voyage Thaïlande',
    icon: '🌴',
    iconBgClass: 'bg-emerald-100',
    progressClass: 'bg-emerald-500',
    budgetTotal: 2400,
    budgetSpent: 500,
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [1500, 600, 0][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [300, 140, 60, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-thai-t1', label: 'Virement épargne', amount: 900, dateLabel: '02 fév.', categoryName: 'Salaire' },
      { id: 'income-thai-t2', label: 'Mission freelance', amount: 600, dateLabel: '21 janv.', categoryName: 'Prime' }
    ],
    expenseTransactions: [
      { id: 'expense-thai-t1', label: 'Acompte vols', amount: 300, dateLabel: '31 janv.', categoryName: 'Voyage' },
      { id: 'expense-thai-t2', label: 'Réservation hôtel', amount: 140, dateLabel: '25 janv.', categoryName: 'Voyage' },
      { id: 'expense-thai-t3', label: 'Assurance voyage', amount: 60, dateLabel: '12 janv.', categoryName: 'Santé' }
    ]
  }
];

const areas: AreaSummary[] = [
  {
    id: 'area-family',
    name: 'Vie de famille',
    icon: '👨‍👩‍👧',
    iconBgClass: 'bg-emerald-100',
    monthlyBudget: 1800,
    monthlySpent: 1234,
    trendLabel: '-8% vs. mois dernier',
    trendClass: 'text-emerald-600',
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [3200, 420, 0][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [640, 340, 254, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-family-t1', label: 'Salaire principal', amount: 2400, dateLabel: '01 fév.', categoryName: 'Salaire' },
      { id: 'income-family-t2', label: 'Salaire secondaire', amount: 800, dateLabel: '02 fév.', categoryName: 'Salaire' },
      { id: 'income-family-t3', label: 'Allocations', amount: 420, dateLabel: '05 fév.', categoryName: 'Prime' }
    ],
    expenseTransactions: [
      { id: 'expense-family-t1', label: 'Supermarché', amount: 260, dateLabel: '03 fév.', categoryName: 'Courses' },
      { id: 'expense-family-t2', label: 'Cantine', amount: 140, dateLabel: '28 janv.', categoryName: 'Logement' },
      { id: 'expense-family-t3', label: 'Transport', amount: 254, dateLabel: '25 janv.', categoryName: 'Voyage' }
    ]
  },
  {
    id: 'area-rental',
    name: 'Appart. rue Voltaire',
    icon: '🏠',
    iconBgClass: 'bg-violet-100',
    monthlyBudget: 920,
    monthlySpent: 740,
    trendLabel: '+4.8% net',
    trendClass: 'text-emerald-600',
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [980, 0, 0][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [320, 210, 210, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-rental-t1', label: 'Loyer janvier', amount: 980, dateLabel: '01 fév.', categoryName: 'Loyer perçu' }
    ],
    expenseTransactions: [
      { id: 'expense-rental-t1', label: 'Charges copro', amount: 320, dateLabel: '28 janv.', categoryName: 'Logement' },
      { id: 'expense-rental-t2', label: 'Assurance', amount: 210, dateLabel: '20 janv.', categoryName: 'Logement' },
      { id: 'expense-rental-t3', label: 'Entretien chaudière', amount: 210, dateLabel: '15 janv.', categoryName: 'Logement' }
    ]
  },
  {
    id: 'area-health',
    name: 'Santé & bien-être',
    icon: '🧘',
    iconBgClass: 'bg-rose-100',
    monthlyBudget: 650,
    monthlySpent: 580,
    trendLabel: 'Stable',
    trendClass: 'text-stone-500',
    incomeCategories: incomeCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [650, 0, 0][index] ?? 0
    })),
    expenseCategories: expenseCategories.map((category, index) => ({
      id: category.id,
      name: category.name,
      colorClass: category.colorClass,
      amount: [260, 190, 130, 0][index] ?? 0
    })),
    incomeTransactions: [
      { id: 'income-health-t1', label: 'Budget mensuel', amount: 650, dateLabel: '01 fév.', categoryName: 'Salaire' }
    ],
    expenseTransactions: [
      { id: 'expense-health-t1', label: 'Cours de sport', amount: 260, dateLabel: '02 fév.', categoryName: 'Santé' },
      { id: 'expense-health-t2', label: 'Consultation', amount: 190, dateLabel: '27 janv.', categoryName: 'Santé' },
      { id: 'expense-health-t3', label: 'Massage', amount: 130, dateLabel: '19 janv.', categoryName: 'Santé' }
    ]
  }
];

const recentActivity: ActivityItem[] = [
  {
    id: 'activity-1',
    label: 'Ski boots deposit',
    amount: 320,
    dateLabel: 'Today',
    category: 'Gear',
    projectId: 'project-ski-2026'
  },
  {
    id: 'activity-2',
    label: 'Family market run',
    amount: 128,
    dateLabel: 'Yesterday',
    category: 'Groceries',
    areaId: 'area-family'
  },
  {
    id: 'activity-3',
    label: 'Kitchen backsplash tiles',
    amount: 460,
    dateLabel: 'Jan 31',
    category: 'Materials',
    projectId: 'project-kitchen'
  },
  {
    id: 'activity-4',
    label: 'Rental plumbing check',
    amount: 210,
    dateLabel: 'Jan 30',
    category: 'Maintenance',
    areaId: 'area-rental'
  },
  {
    id: 'activity-5',
    label: 'Wedding venue deposit',
    amount: 850,
    dateLabel: 'Jan 29',
    category: 'Venue',
    projectId: 'project-wedding'
  },
  {
    id: 'activity-6',
    label: 'Yoga studio membership',
    amount: 95,
    dateLabel: 'Jan 28',
    category: 'Wellness',
    areaId: 'area-health'
  }
];

export const dashboardMockData = {
  summary,
  projects,
  areas,
  recentActivity
};
