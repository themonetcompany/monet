import { createActionGroup, emptyProps, props } from '@ngrx/store';

import type { ActivityItem, AreaSummary, DashboardSummary, ProjectSummary } from './dashboard.models';

export const DashboardActions = createActionGroup({
  source: 'Dashboard',
  events: {
    'Load Dashboard': emptyProps(),
    'Load Dashboard Success': props<{
      summary: DashboardSummary;
      projects: ProjectSummary[];
      areas: AreaSummary[];
      recentActivity: ActivityItem[];
    }>(),
    'Load Dashboard Failure': props<{ error: string }>()
  }
});
