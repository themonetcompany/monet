import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';

import { DashboardActions } from '../state/dashboard/dashboard.actions';
import {
  selectDashboardActivity,
  selectDashboardAreas,
  selectDashboardError,
  selectDashboardLoading,
  selectDashboardProjects,
  selectDashboardSummary
} from '../state/dashboard/dashboard.selectors';
import type { ProjectSummary } from '../state/dashboard/dashboard.models';

@Component({
  selector: 'app-dashboard',
  imports: [AsyncPipe, CurrencyPipe],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  private readonly store = inject(Store);

  readonly loading$ = this.store.select(selectDashboardLoading);
  readonly error$ = this.store.select(selectDashboardError);
  readonly summary$ = this.store.select(selectDashboardSummary);
  readonly projects$ = this.store.select(selectDashboardProjects);
  readonly areas$ = this.store.select(selectDashboardAreas);
  readonly activity$ = this.store.select(selectDashboardActivity);

  ngOnInit(): void {
    this.store.dispatch(DashboardActions.loadDashboard());
  }

  projectProgress(project: ProjectSummary): number {
    if (project.budgetTotal <= 0) {
      return 0;
    }
    return Math.min(100, Math.round((project.budgetSpent / project.budgetTotal) * 100));
  }

}
