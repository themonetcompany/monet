import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';

import { HealthActions } from './state/health/health.actions';
import {
  selectHealthData,
  selectHealthError,
  selectHealthLoading
} from './state/health/health.selectors';
import { VersionActions } from './state/version/version.actions';
import {
  selectVersionError,
  selectVersionLoading,
  selectVersionValue
} from './state/version/version.selectors';

@Component({
  selector: 'app-root',
  imports: [AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  private readonly store = inject(Store);

  readonly health$ = this.store.select(selectHealthData);
  readonly loading$ = this.store.select(selectHealthLoading);
  readonly error$ = this.store.select(selectHealthError);
  readonly version$ = this.store.select(selectVersionValue);
  readonly versionLoading$ = this.store.select(selectVersionLoading);
  readonly versionError$ = this.store.select(selectVersionError);

  ngOnInit(): void {
    this.store.dispatch(HealthActions.loadHealth());
    this.store.dispatch(VersionActions.loadVersion());
  }

  refresh(): void {
    this.store.dispatch(HealthActions.loadHealth());
    this.store.dispatch(VersionActions.loadVersion());
  }
}
