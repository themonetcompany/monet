import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';

import { DashboardActions } from './state/dashboard/dashboard.actions';
import {
  selectDashboardAreas,
  selectDashboardProjects
} from './state/dashboard/dashboard.selectors';
import { VersionActions } from './state/version/version.actions';
import {
  selectVersionError,
  selectVersionLoading,
  selectVersionValue
} from './state/version/version.selectors';

@Component({
  selector: 'app-root',
  imports: [AsyncPipe, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  private readonly store = inject(Store);

  readonly version$ = this.store.select(selectVersionValue);
  readonly versionLoading$ = this.store.select(selectVersionLoading);
  readonly versionError$ = this.store.select(selectVersionError);
  readonly projects$ = this.store.select(selectDashboardProjects);
  readonly areas$ = this.store.select(selectDashboardAreas);

  ngOnInit(): void {
    this.store.dispatch(DashboardActions.loadDashboard());
    this.store.dispatch(VersionActions.loadVersion());
  }
}
